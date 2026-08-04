using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SmartEstimate.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SmartEstimate.IntegrationTests.Knowledge;

public sealed class KnowledgeStudioTests : IAsyncLifetime, IDisposable
{
    private static readonly string[] PaintingTags = ["painting", "walls"];
    private static readonly string[] MaterialTags = ["painting"];
    private static readonly string[] UpdatedPaintingTags = ["painting", "interior"];
    private static readonly string[] EstimateZones = ["Кухня", "Ванна"];

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartestimate_tests")
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
        await factory.MigrateDatabaseAsync();
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
    public async Task MvpKnowledgeSeedIsAvailableAfterMigrations()
    {
        using var client = Factory.CreateClient();

        Assert.True(await GetTotalCountAsync(client, "/api/v1/knowledge/categories?activeOnly=false&pageSize=1000") >= 26);
        Assert.True(await GetTotalCountAsync(client, "/api/v1/knowledge/units?activeOnly=false&pageSize=1000") >= 36);
        Assert.True(await GetTotalCountAsync(client, "/api/v1/knowledge/construction-works?activeOnly=false&pageSize=1000") >= 422);
        Assert.True(await GetTotalCountAsync(client, "/api/v1/knowledge/materials?activeOnly=false&pageSize=1000") >= 260);
    }

    [Fact]
    public async Task ActiveKnowledgeCreatedThroughApiIsImmediatelyAvailableToEstimateEditor()
    {
        using var client = Factory.CreateClient();
        var unit = await CreateAsync(client, "/api/v1/knowledge/units", new
        {
            symbol = "test-m2",
            name = new { uk = "Тестова одиниця площі", en = "Test area unit", de = "Test-Flächeneinheit" },
            status = "Active"
        });
        var category = await CreateAsync(client, "/api/v1/knowledge/categories", new
        {
            name = new { uk = "Тестові стіни інтеграції", en = "Integration test walls", de = "Integrationstest-Wände" },
            description = "Wall work",
            status = "Active"
        });
        var work = await CreateAsync(client, "/api/v1/knowledge/construction-works", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Інтеграційне фарбування стін", en = "Integration wall painting", de = "Integrationstest Wände streichen" },
            description = "Two coats",
            tags = PaintingTags,
            status = "Active"
        });
        await CreateAsync(client, "/api/v1/knowledge/materials", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Інтеграційна фарба", en = "Integration paint", de = "Integrationstest-Farbe" },
            tags = MaterialTags,
            status = "Active"
        });

        var listResponse = await client.GetAsync("/api/v1/knowledge/construction-works?search=інтеграційне");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Single(listDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/knowledge/construction-works/{work.GetProperty("id").GetString()}",
            new
            {
                categoryId = category.GetProperty("id").GetString(),
                unitId = unit.GetProperty("id").GetString(),
                name = new { uk = "Інтеграційне фарбування стін", en = "Integration wall painting", de = "Integrationstest Wände streichen" },
                description = "Updated work description",
                tags = UpdatedPaintingTags,
                status = "Active"
            });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var estimateResponse = await client.PostAsJsonAsync("/api/v1/estimates", new
        {
            estimateNumber = "KNOWLEDGE-001",
            currency = "UAH",
            objectType = "Apartment",
            objectAddress = "Київ, тестовий об'єкт",
            totalArea = 54.2m,
            zones = EstimateZones
        });
        Assert.Equal(HttpStatusCode.Created, estimateResponse.StatusCode);
        using var estimateDocument = JsonDocument.Parse(await estimateResponse.Content.ReadAsStringAsync());
        var estimateData = estimateDocument.RootElement.GetProperty("data");
        var estimateId = estimateData.GetProperty("id").GetString();
        var zoneId = estimateData.GetProperty("zones").EnumerateArray().First().GetProperty("id").GetString();

        var addResponse = await client.PostAsJsonAsync(
            $"/api/v1/estimates/{estimateId}/work-items",
            new
            {
                constructionWorkId = work.GetProperty("id").GetString(),
                quantity = 20m,
                unitPrice = 250m,
                zoneId
            });
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var archiveResponse = await client.DeleteAsync($"/api/v1/knowledge/construction-works/{work.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var activeResponse = await client.GetAsync("/api/v1/knowledge/construction-works?search=інтеграційне");
        using var activeDocument = JsonDocument.Parse(await activeResponse.Content.ReadAsStringAsync());
        Assert.Empty(activeDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());

        var studioResponse = await client.GetAsync("/api/v1/knowledge/construction-works?search=інтеграційне&activeOnly=false&status=Archived");
        using var studioDocument = JsonDocument.Parse(await studioResponse.Content.ReadAsStringAsync());
        JsonElement archived = Assert.Single(studioDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
        Assert.Equal("Archived", archived.GetProperty("status").GetString());
        Assert.True(archived.GetProperty("version").GetInt32() >= 3);
    }

    [Fact]
    public async Task KnowledgeApiRejectsDuplicateNamesAndInvalidReferences()
    {
        using var client = Factory.CreateClient();
        var unit = new { symbol = "test-l", name = new { uk = "Тестовий літр інтеграції", en = "Integration test litre", de = "Integrationstest-Liter" }, status = "Draft" };
        await CreateAsync(client, "/api/v1/knowledge/units", unit);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/knowledge/units", unit);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var invalidWorkResponse = await client.PostAsJsonAsync("/api/v1/knowledge/construction-works", new
        {
            categoryId = Guid.NewGuid(),
            unitId = Guid.NewGuid(),
            name = new { uk = "Неіснуюча робота" },
            status = "Draft"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidWorkResponse.StatusCode);
    }

    private static async Task<JsonElement> CreateAsync(HttpClient client, string path, object request)
    {
        var response = await client.PostAsJsonAsync(path, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private static async Task<int> GetTotalCountAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32();
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
