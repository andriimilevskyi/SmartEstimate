using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SmartEstimate.Domain.Pricing;
using SmartEstimate.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SmartEstimate.IntegrationTests.Pricing;

public sealed class PricingFoundationTests : IAsyncLifetime, IDisposable
{
    private static readonly string[] SingleEstimateZone = ["Основна"];

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartestimate_pricing_tests")
        .WithUsername("smartestimate")
        .WithPassword("smartestimate")
        .WithPortBinding(5432, true)
        .Build();

    private SmartEstimateApiFactory? factory;
    private bool disposed;

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        factory = new SmartEstimateApiFactory(CreateConnectionString(database));
        await Factory.MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        factory?.Dispose();
        await database.DisposeAsync();
    }

    [Fact]
    public async Task PricingApiCreatesHistoryAndResolvesSpecificCurrencyAndEffectivePrices()
    {
        using var client = Factory.CreateClient();
        var catalog = await CreatePricingKnowledgeAsync(client, "resolution");
        var basePrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 100m, "UAH", "2026-08-01", null, null);
        var regionPrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 120m, "UAH", "2026-08-01", "UA-32", null);
        var supplierPrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 130m, "UAH", "2026-08-01", null, "Supplier A");
        var supplierRegionPrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 140m, "UAH", "2026-08-01", "UA-32", "Supplier A");
        await CreatePriceAsync(client, catalog.MaterialId, "Material", 77m, "EUR", "2026-08-01", null, null);
        await CreatePriceAsync(client, catalog.MaterialId, "Material", 90m, "UAH", "2026-07-01", "UA-12", null);
        var futurePrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 170m, "UAH", "2026-09-01", "UA-32", "Supplier A");
        var archivedPrice = await CreatePriceAsync(client, catalog.WorkId, "ConstructionWork", 55m, "UAH", "2026-08-01", null, null);

        Assert.NotEqual(Guid.Empty, basePrice.GetProperty("id").GetGuid());
        Assert.NotEqual(Guid.Empty, supplierRegionPrice.GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/pricing/prices/{archivedPrice.GetProperty("id").GetString()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/pricing/resolve/ConstructionWork/{catalog.WorkId}?currency=UAH")).StatusCode);

        Assert.Equal(140m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", "UA-32", "Supplier A"));
        Assert.Equal(130m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", null, "Supplier A"));
        Assert.Equal(120m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", "UA-32", null));
        Assert.Equal(100m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", null, null));
        Assert.Equal(77m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "EUR", null, null));
        Assert.Equal(140m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", "UA-32", "Supplier A", "2026-08-14T00:00:00Z"));
        Assert.Equal(170m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", "UA-32", "Supplier A", "2026-09-14T00:00:00Z"));

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/pricing/prices/{basePrice.GetProperty("id").GetString()}", new
        {
            targetType = "Material",
            targetId = catalog.MaterialId,
            amount = 105m,
            currency = "UAH",
            effectiveFrom = "2026-08-10T00:00:00Z",
            sourceType = "Manual"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var history = await GetDataAsync(client, $"/api/v1/pricing/history/Material/{catalog.MaterialId}");
        Assert.True(history.GetProperty("prices").EnumerateArray().Count() >= 7);
        Assert.Contains(
            history.GetProperty("events").EnumerateArray(),
            item => item.GetProperty("changeType").GetString() == "Updated"
                && item.GetProperty("catalogPriceId").GetGuid() == basePrice.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task EstimateAddUsesPricingSnapshotAndManualOverridesDoNotMutateCatalogPrice()
    {
        using var client = Factory.CreateClient();
        var catalog = await CreatePricingKnowledgeAsync(client, "estimate");
        var materialPrice = await CreatePriceAsync(client, catalog.MaterialId, "Material", 135m, "UAH", "2026-08-01", null, null);
        var workPrice = await CreatePriceAsync(client, catalog.WorkId, "ConstructionWork", 280m, "UAH", "2026-08-01", null, null);
        var estimate = await CreateEstimateAsync(client, "PRICING-EST-001");
        var estimateId = estimate.GetProperty("id").GetGuid();
        var zoneId = estimate.GetProperty("zones").EnumerateArray().First().GetProperty("id").GetGuid();

        var addMaterial = await client.PostAsJsonAsync($"/api/v1/estimates/{estimateId}/material-items", new
        {
            materialId = catalog.MaterialId,
            quantity = 2m,
            zoneId
        });
        Assert.Equal(HttpStatusCode.OK, addMaterial.StatusCode);
        var materialLine = Assert.Single((await ReadDataAsync(addMaterial)).GetProperty("materialItems").EnumerateArray());
        Assert.Equal(135m, materialLine.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(materialPrice.GetProperty("id").GetGuid(), materialLine.GetProperty("sourcePriceId").GetGuid());
        Assert.Equal(JsonValueKind.String, materialLine.GetProperty("priceCapturedAt").ValueKind);
        Assert.False(materialLine.GetProperty("isUnitPriceManuallyOverridden").GetBoolean());

        var addWork = await client.PostAsJsonAsync($"/api/v1/estimates/{estimateId}/work-items", new
        {
            constructionWorkId = catalog.WorkId,
            quantity = 3m,
            zoneId
        });
        Assert.Equal(HttpStatusCode.OK, addWork.StatusCode);
        var workLine = Assert.Single((await ReadDataAsync(addWork)).GetProperty("workItems").EnumerateArray());
        Assert.Equal(280m, workLine.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(workPrice.GetProperty("id").GetGuid(), workLine.GetProperty("sourcePriceId").GetGuid());
        Assert.Equal(JsonValueKind.String, workLine.GetProperty("priceCapturedAt").ValueKind);

        await CreatePriceAsync(client, catalog.MaterialId, "Material", 150m, "UAH", "2026-08-10", null, null);
        var unchangedEstimate = await GetDataAsync(client, $"/api/v1/estimates/{estimateId}");
        Assert.Equal(135m, Assert.Single(unchangedEstimate.GetProperty("materialItems").EnumerateArray()).GetProperty("unitPrice").GetDecimal());

        var materialItemId = materialLine.GetProperty("id").GetGuid();
        var updateLineResponse = await client.PatchAsJsonAsync($"/api/v1/estimates/{estimateId}/material-items/{materialItemId}", new
        {
            quantity = 2m,
            unitPrice = 125m,
            notes = "Manual price"
        });
        Assert.Equal(HttpStatusCode.OK, updateLineResponse.StatusCode);
        var updatedLine = Assert.Single((await ReadDataAsync(updateLineResponse)).GetProperty("materialItems").EnumerateArray());
        Assert.Equal(125m, updatedLine.GetProperty("unitPrice").GetDecimal());
        Assert.True(updatedLine.GetProperty("isUnitPriceManuallyOverridden").GetBoolean());
        Assert.Equal(150m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", null, null));

        var unpriced = await CreatePricingKnowledgeAsync(client, "manual-only");
        var manualEstimate = await CreateEstimateAsync(client, "PRICING-EST-002");
        var manualEstimateId = manualEstimate.GetProperty("id").GetGuid();
        var manualZoneId = manualEstimate.GetProperty("zones").EnumerateArray().First().GetProperty("id").GetGuid();
        var manualMaterialResponse = await client.PostAsJsonAsync($"/api/v1/estimates/{manualEstimateId}/material-items", new
        {
            materialId = unpriced.MaterialId,
            quantity = 1m,
            unitPrice = 44m,
            zoneId = manualZoneId
        });
        var manualWorkResponse = await client.PostAsJsonAsync($"/api/v1/estimates/{manualEstimateId}/work-items", new
        {
            constructionWorkId = unpriced.WorkId,
            quantity = 1m,
            unitPrice = 66m,
            zoneId = manualZoneId
        });

        Assert.Equal(HttpStatusCode.OK, manualMaterialResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, manualWorkResponse.StatusCode);
        Assert.Equal(44m, Assert.Single((await ReadDataAsync(manualMaterialResponse)).GetProperty("materialItems").EnumerateArray()).GetProperty("unitPrice").GetDecimal());
        Assert.Equal(66m, Assert.Single((await ReadDataAsync(manualWorkResponse)).GetProperty("workItems").EnumerateArray()).GetProperty("unitPrice").GetDecimal());
    }

    [Theory]
    [InlineData("uk-UA", "Матеріал", "Робота", "Категорія")]
    [InlineData("en-US", "Material", "Work", "Category")]
    [InlineData("de-DE", "Material", "Arbeit", "Kategorie")]
    public async Task PricingCatalogUsesLocalizedKnowledgeNamesFromAcceptLanguage(
        string acceptLanguage,
        string expectedMaterialPrefix,
        string expectedWorkPrefix,
        string expectedCategoryPrefix)
    {
        using var client = Factory.CreateClient();
        var catalog = await CreatePricingKnowledgeAsync(client, "localized");

        var materialCatalog = await GetDataAsync(
            client,
            $"/api/v1/pricing/catalog?targetType=Material&categoryId={catalog.CategoryId}",
            acceptLanguage);
        var material = Assert.Single(materialCatalog.GetProperty("items").EnumerateArray());
        Assert.Equal(catalog.MaterialId, material.GetProperty("targetId").GetGuid());
        Assert.StartsWith(expectedMaterialPrefix, material.GetProperty("name").GetString());
        Assert.StartsWith(expectedCategoryPrefix, material.GetProperty("categoryName").GetString());

        var workCatalog = await GetDataAsync(
            client,
            $"/api/v1/pricing/catalog?targetType=ConstructionWork&categoryId={catalog.CategoryId}",
            acceptLanguage);
        var work = Assert.Single(workCatalog.GetProperty("items").EnumerateArray());
        Assert.Equal(catalog.WorkId, work.GetProperty("targetId").GetGuid());
        Assert.StartsWith(expectedWorkPrefix, work.GetProperty("name").GetString());
        Assert.StartsWith(expectedCategoryPrefix, work.GetProperty("categoryName").GetString());
    }

    [Fact]
    public async Task PricingCatalogFallsBackToUkrainianForUnsupportedAcceptLanguage()
    {
        using var client = Factory.CreateClient();
        var catalog = await CreatePricingKnowledgeAsync(client, "locale-fallback");

        var materialCatalog = await GetDataAsync(
            client,
            $"/api/v1/pricing/catalog?targetType=Material&categoryId={catalog.CategoryId}",
            "fr-FR");
        var material = Assert.Single(materialCatalog.GetProperty("items").EnumerateArray());

        Assert.Equal(catalog.MaterialId, material.GetProperty("targetId").GetGuid());
        Assert.StartsWith("Матеріал", material.GetProperty("name").GetString());
        Assert.StartsWith("Категорія", material.GetProperty("categoryName").GetString());
    }

    [Fact]
    public async Task PostgreSqlPreventsTwoActivePricesForSameOpenScope()
    {
        using var client = Factory.CreateClient();
        var catalog = await CreatePricingKnowledgeAsync(client, "constraint");
        var price = await CreatePriceAsync(client, catalog.MaterialId, "Material", 100m, "UAH", "2026-08-01", null, null);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        dbContext.CatalogPrices.Add(CatalogPrice.Create(
            Guid.NewGuid(),
            new PriceTarget(PriceTargetType.Material, catalog.MaterialId),
            101m,
            new PriceScope("UAH", null, null, null),
            new DateTimeOffset(2026, 8, 2, 0, 0, 0, TimeSpan.Zero),
            PriceSourceType.Manual,
            null,
            DateTimeOffset.UtcNow));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.Contains("UX_CatalogPrices_OpenScope", exception.InnerException?.Message ?? exception.Message);
        Assert.Equal(100m, await ResolveAmountAsync(client, catalog.MaterialId, "Material", "UAH", null, null));
        Assert.NotEqual(Guid.Empty, price.GetProperty("id").GetGuid());
    }

    private static async Task<PricingKnowledge> CreatePricingKnowledgeAsync(HttpClient client, string suffix)
    {
        var unique = $"{suffix}-{Guid.NewGuid():N}"[..24];
        var symbol = $"u{Guid.NewGuid():N}"[..12];
        var unit = await CreateAsync(client, "/api/v1/knowledge/units", new
        {
            symbol,
            name = new { uk = $"Одиниця {unique}", en = $"Unit {unique}", de = $"Einheit {unique}" },
            status = "Active"
        });
        var category = await CreateAsync(client, "/api/v1/knowledge/categories", new
        {
            name = new { uk = $"Категорія {unique}", en = $"Category {unique}", de = $"Kategorie {unique}" },
            status = "Active"
        });
        var material = await CreateAsync(client, "/api/v1/knowledge/materials", new
        {
            categoryId = category.GetProperty("id").GetGuid(),
            unitId = unit.GetProperty("id").GetGuid(),
            name = new { uk = $"Матеріал {unique}", en = $"Material {unique}", de = $"Material {unique}" },
            status = "Active"
        });
        var work = await CreateAsync(client, "/api/v1/knowledge/construction-works", new
        {
            categoryId = category.GetProperty("id").GetGuid(),
            unitId = unit.GetProperty("id").GetGuid(),
            name = new { uk = $"Робота {unique}", en = $"Work {unique}", de = $"Arbeit {unique}" },
            status = "Active"
        });

        return new PricingKnowledge(
            category.GetProperty("id").GetGuid(),
            material.GetProperty("id").GetGuid(),
            work.GetProperty("id").GetGuid());
    }

    private static async Task<JsonElement> CreatePriceAsync(
        HttpClient client,
        Guid targetId,
        string targetType,
        decimal amount,
        string currency,
        string effectiveFrom,
        string? regionCode,
        string? supplierName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/pricing/prices", new
        {
            targetType,
            targetId,
            amount,
            currency,
            effectiveFrom = $"{effectiveFrom}T00:00:00Z",
            sourceType = "Manual",
            regionCode,
            supplierName
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync(response);
    }

    private static async Task<decimal> ResolveAmountAsync(
        HttpClient client,
        Guid targetId,
        string targetType,
        string currency,
        string? regionCode,
        string? supplierName,
        string? effectiveDate = null)
    {
        var query = new List<string> { $"currency={currency}" };
        if (!string.IsNullOrWhiteSpace(regionCode))
        {
            query.Add($"regionCode={regionCode}");
        }

        if (!string.IsNullOrWhiteSpace(supplierName))
        {
            query.Add($"supplierName={Uri.EscapeDataString(supplierName)}");
        }

        if (!string.IsNullOrWhiteSpace(effectiveDate))
        {
            query.Add($"date={Uri.EscapeDataString(effectiveDate)}");
        }

        var data = await GetDataAsync(client, $"/api/v1/pricing/resolve/{targetType}/{targetId}?{string.Join('&', query)}");
        return data.GetProperty("amount").GetDecimal();
    }

    private static async Task<JsonElement> CreateEstimateAsync(HttpClient client, string estimateNumber)
    {
        var customer = await CreateAsync(client, "/api/v1/customers", new { name = $"Pricing customer {estimateNumber}" });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetGuid(),
            name = $"Pricing object {estimateNumber}",
            objectType = "Apartment"
        });

        return await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber,
            objectId = estimateObject.GetProperty("id").GetGuid(),
            currency = "UAH",
            zones = SingleEstimateZone
        });
    }

    private static async Task<JsonElement> CreateAsync(HttpClient client, string path, object request)
    {
        var response = await client.PostAsJsonAsync(path, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync(response);
    }

    private static async Task<JsonElement> GetDataAsync(
        HttpClient client,
        string path,
        string? acceptLanguage = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            request.Headers.AcceptLanguage.ParseAdd(acceptLanguage);
        }

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadDataAsync(response);
    }

    private static async Task<JsonElement> ReadDataAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private SmartEstimateApiFactory Factory => factory ?? throw new InvalidOperationException("The API factory is not initialized.");

    private static string CreateConnectionString(PostgreSqlContainer container)
    {
        var builder = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Host = Environment.GetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE") ?? container.Hostname,
            Port = container.GetMappedPublicPort(5432)
        };

        return builder.ConnectionString;
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    private sealed record PricingKnowledge(Guid CategoryId, Guid MaterialId, Guid WorkId);

    private sealed class SmartEstimateApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        public async Task MigrateDatabaseAsync()
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<SmartEstimateDbContext>>();
                services.RemoveAll<SmartEstimateDbContext>();
                services.AddDbContext<SmartEstimateDbContext>(options => options.UseNpgsql(connectionString));
            });
        }
    }
}
