using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SmartEstimate.IntegrationTests.Knowledge;

public sealed class KnowledgeStudioTests : IAsyncLifetime, IDisposable
{
    private static readonly string[] PaintingTags = ["painting", "walls"];
    private static readonly string[] MaterialTags = ["painting"];
    private static readonly string[] LocalizedSnapshotTags = ["drywall"];
    private static readonly string[] UpdatedPaintingTags = ["painting", "interior"];
    private static readonly string[] EstimateZones = ["Кухня", "Ванна"];
    private static readonly string[] SingleEstimateZone = ["Кухня"];

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

        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Інтеграційний замовник",
            phone = "+380000000000",
            email = "customer@example.test",
            note = "Created by integration test"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Київський тестовий об'єкт",
            objectType = "Apartment",
            address = "Київ, тестовий об'єкт",
            totalArea = 54.2m,
            description = "Object created in test"
        });

        var estimateResponse = await client.PostAsJsonAsync("/api/v1/estimates", new
        {
            estimateNumber = "KNOWLEDGE-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
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

        var templatesResponse = await client.GetAsync("/api/v1/estimates/document-templates");
        Assert.Equal(HttpStatusCode.OK, templatesResponse.StatusCode);
        using var templatesDocument = JsonDocument.Parse(await templatesResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            3,
            templatesDocument.RootElement.GetProperty("data").EnumerateArray().Count());

        var germanTemplatesResponse = await client.GetAsync("/api/v1/estimates/document-templates?locale=de");
        Assert.Equal(HttpStatusCode.OK, germanTemplatesResponse.StatusCode);
        using var germanTemplatesDocument = JsonDocument.Parse(await germanTemplatesResponse.Content.ReadAsStringAsync());
        Assert.Contains(
            germanTemplatesDocument.RootElement.GetProperty("data").EnumerateArray(),
            template => template.GetProperty("code").GetString() == "commercial-proposal"
                && template.GetProperty("name").GetString() == "Angebot");

        var invalidLocaleTemplatesResponse = await client.GetAsync("/api/v1/estimates/document-templates?locale=fr");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidLocaleTemplatesResponse.StatusCode);

        var pdfResponse = await client.GetAsync(
            $"/api/v1/estimates/{estimateId}/documents/pdf?template=full-estimate");
        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", pdfResponse.Content.Headers.ContentDisposition?.DispositionType);
        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);

        var inlinePreviewResponse = await client.GetAsync(
            $"/api/v1/estimates/{estimateId}/documents/pdf?template=commercial-proposal&locale=de&disposition=inline");
        Assert.Equal(HttpStatusCode.OK, inlinePreviewResponse.StatusCode);
        Assert.Equal("application/pdf", inlinePreviewResponse.Content.Headers.ContentType?.MediaType);
        Assert.Null(inlinePreviewResponse.Content.Headers.ContentDisposition);
        var inlineBytes = await inlinePreviewResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF"u8.ToArray(), inlineBytes[..4]);

        var invalidLocalePdfResponse = await client.GetAsync(
            $"/api/v1/estimates/{estimateId}/documents/pdf?template=full-estimate&locale=fr");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidLocalePdfResponse.StatusCode);

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
    public async Task EstimateItemsUseLocalizedSnapshotIndependentFromKnowledgeChanges()
    {
        using var client = Factory.CreateClient();
        var unit = await CreateAsync(client, "/api/v1/knowledge/units", new
        {
            symbol = "test-board",
            name = new { uk = "Тестовий лист", en = "Test board", de = "Testplatte" },
            status = "Active"
        });
        var category = await CreateAsync(client, "/api/v1/knowledge/categories", new
        {
            name = new { uk = "Тестові локалізовані матеріали", en = "Localized test materials", de = "Lokalisierte Testmaterialien" },
            status = "Active"
        });
        var material = await CreateAsync(client, "/api/v1/knowledge/materials", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Гіпсокартон", en = "Drywall", de = "Gipskarton" },
            tags = LocalizedSnapshotTags,
            status = "Active"
        });
        var work = await CreateAsync(client, "/api/v1/knowledge/construction-works", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Монтаж гіпсокартону", en = "Drywall installation", de = "Gipskarton montieren" },
            tags = LocalizedSnapshotTags,
            status = "Active"
        });
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Localized estimate customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Localized estimate object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LOC-SNAPSHOT-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });
        var estimateId = estimate.GetProperty("id").GetGuid();
        var zoneId = estimate.GetProperty("zones").EnumerateArray().Single().GetProperty("id").GetGuid();

        var addMaterial = await client.PostAsJsonAsync($"/api/v1/estimates/{estimateId}/material-items", new
        {
            materialId = material.GetProperty("id").GetString(),
            quantity = 3m,
            unitPrice = 250m,
            zoneId
        });
        Assert.Equal(HttpStatusCode.OK, addMaterial.StatusCode);
        var addWork = await client.PostAsJsonAsync($"/api/v1/estimates/{estimateId}/work-items", new
        {
            constructionWorkId = work.GetProperty("id").GetString(),
            quantity = 3m,
            unitPrice = 500m,
            zoneId
        });
        Assert.Equal(HttpStatusCode.OK, addWork.StatusCode);

        var databaseSnapshot = await GetLocalizedItemSnapshotAsync(estimateId);
        Assert.NotNull(databaseSnapshot.MaterialNameSnapshot);
        Assert.Equal("Гіпсокартон", databaseSnapshot.MaterialNameSnapshot!.Uk);
        Assert.Equal("Drywall", databaseSnapshot.MaterialNameSnapshot.En);
        Assert.Equal("Gipskarton", databaseSnapshot.MaterialNameSnapshot.De);
        Assert.Equal(EstimateItemNameSource.KnowledgeSnapshot, databaseSnapshot.MaterialNameSource);
        Assert.NotNull(databaseSnapshot.WorkNameSnapshot);
        Assert.Equal("Drywall installation", databaseSnapshot.WorkNameSnapshot!.En);
        Assert.Equal(EstimateItemNameSource.KnowledgeSnapshot, databaseSnapshot.WorkNameSource);

        AssertEstimateNames(
            await GetEstimateDataAsync(client, estimateId, "uk-UA"),
            "Гіпсокартон",
            "Монтаж гіпсокартону");
        AssertEstimateNames(
            await GetEstimateDataAsync(client, estimateId, "en-US"),
            "Drywall",
            "Drywall installation");
        AssertEstimateNames(
            await GetEstimateDataAsync(client, estimateId, "de-DE"),
            "Gipskarton",
            "Gipskarton montieren");

        await PutOkAsync(client, $"/api/v1/knowledge/materials/{material.GetProperty("id").GetString()}", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Вологостійкий гіпсокартон", en = "Moisture-resistant drywall", de = "Feuchtraum-Gipskarton" },
            tags = LocalizedSnapshotTags,
            status = "Archived"
        });
        await PutOkAsync(client, $"/api/v1/knowledge/construction-works/{work.GetProperty("id").GetString()}", new
        {
            categoryId = category.GetProperty("id").GetString(),
            unitId = unit.GetProperty("id").GetString(),
            name = new { uk = "Новий монтаж гіпсокартону", en = "Updated drywall installation", de = "Aktualisierte Gipskartonmontage" },
            tags = LocalizedSnapshotTags,
            status = "Archived"
        });

        AssertEstimateNames(
            await GetEstimateDataAsync(client, estimateId, "en-US"),
            "Drywall",
            "Drywall installation");

        var materialItemId = (await GetEstimateDataAsync(client, estimateId, "de-DE"))
            .GetProperty("materialItems")
            .EnumerateArray()
            .Single()
            .GetProperty("id")
            .GetGuid();
        var duplicateResponse = await client.PostAsync($"/api/v1/estimates/{estimateId}/material-items/{materialItemId}/duplicate", null);
        Assert.True(
            duplicateResponse.StatusCode == HttpStatusCode.OK,
            await duplicateResponse.Content.ReadAsStringAsync());
        using var duplicateDocument = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        Assert.All(
            duplicateDocument.RootElement.GetProperty("data").GetProperty("materialItems").EnumerateArray(),
            item =>
            {
                Assert.Equal("Гіпсокартон", item.GetProperty("name").GetString());
                Assert.Equal("KnowledgeSnapshot", item.GetProperty("nameSource").GetString());
            });

        var germanPdf = await client.GetAsync($"/api/v1/estimates/{estimateId}/documents/pdf?template=full-estimate&locale=de");
        Assert.Equal(HttpStatusCode.OK, germanPdf.StatusCode);
        var pdfBytes = await germanPdf.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
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

    [Fact]
    public async Task OverviewAndCustomerDetailsUseRealBusinessData()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Dashboard customer",
            phone = "+380111111111",
            email = "dashboard@example.test",
            note = "Overview test"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Dashboard object",
            objectType = "Apartment",
            address = "Dashboard street",
            totalArea = 42.5m,
            description = "Overview object"
        });

        var estimateResponse = await client.PostAsJsonAsync("/api/v1/estimates", new
        {
            estimateNumber = "DASHBOARD-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });
        Assert.Equal(HttpStatusCode.Created, estimateResponse.StatusCode);

        var customerResponse = await client.GetAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, customerResponse.StatusCode);
        using var customerDocument = JsonDocument.Parse(await customerResponse.Content.ReadAsStringAsync());
        Assert.Equal("Dashboard customer", customerDocument.RootElement.GetProperty("data").GetProperty("name").GetString());

        var overviewResponse = await client.GetAsync("/api/v1/overview");
        Assert.Equal(HttpStatusCode.OK, overviewResponse.StatusCode);
        using var overviewDocument = JsonDocument.Parse(await overviewResponse.Content.ReadAsStringAsync());
        var overview = overviewDocument.RootElement.GetProperty("data");

        Assert.True(overview.GetProperty("estimates").GetProperty("total").GetInt32() >= 1);
        Assert.True(overview.GetProperty("estimates").GetProperty("draft").GetInt32() >= 1);
        Assert.Contains(
            overview.GetProperty("recentEstimates").EnumerateArray(),
            estimate => estimate.GetProperty("estimateNumber").GetString() == "DASHBOARD-001");
        Assert.Contains(
            overview.GetProperty("recentObjects").EnumerateArray(),
            item => item.GetProperty("name").GetString() == "Dashboard object");
    }

    [Fact]
    public async Task CustomerWithoutObjectsCanBeDeletedPermanently()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Delete customer",
            phone = "+380222222222"
        });

        var deleteResponse = await client.DeleteAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.False(await CustomerExistsInDatabaseAsync(customer.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task CustomerWithObjectsCannotBeDeletedPermanently()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Protected customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Protected object",
            objectType = "Apartment"
        });

        var deleteResponse = await client.DeleteAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
        await AssertApiErrorAsync(deleteResponse, "CustomerHasObjects");

        var customerResponse = await client.GetAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, customerResponse.StatusCode);
        var objectResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, objectResponse.StatusCode);
        Assert.True(await CustomerExistsInDatabaseAsync(customer.GetProperty("id").GetGuid()));
        Assert.True(await ObjectExistsInDatabaseAsync(estimateObject.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ArchivedCustomerAppearsOnlyInArchivedFilterAndCanBeRestored()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Archived customer"
        });

        var archiveResponse = await client.PatchAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);

        Assert.DoesNotContain(
            await GetBusinessIdsAsync(client, "/api/v1/customers?status=active"),
            id => id == customer.GetProperty("id").GetGuid());
        Assert.Contains(
            await GetBusinessIdsAsync(client, "/api/v1/customers?status=archived"),
            id => id == customer.GetProperty("id").GetGuid());

        var restoreResponse = await client.PatchAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        Assert.Contains(
            await GetBusinessIdsAsync(client, "/api/v1/customers?status=active"),
            id => id == customer.GetProperty("id").GetGuid());
        Assert.DoesNotContain(
            await GetBusinessIdsAsync(client, "/api/v1/customers?status=archived"),
            id => id == customer.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task RestoreCustomerDoesNotRestoreArchivedObjects()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Restore customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Still archived object",
            objectType = "Apartment"
        });

        var archiveObjectResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveObjectResponse.StatusCode);

        var archiveCustomerResponse = await client.PatchAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveCustomerResponse.StatusCode);

        var restoreCustomerResponse = await client.PatchAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreCustomerResponse.StatusCode);

        var objectResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, objectResponse.StatusCode);
        using var objectDocument = JsonDocument.Parse(await objectResponse.Content.ReadAsStringAsync());
        Assert.True(objectDocument.RootElement.GetProperty("data").GetProperty("isArchived").GetBoolean());
    }

    [Fact]
    public async Task ObjectWithoutEstimatesCanBeDeletedPermanently()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Object owner"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Delete object",
            objectType = "Apartment"
        });

        var deleteResponse = await client.DeleteAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.False(await ObjectExistsInDatabaseAsync(estimateObject.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ActiveEstimateDeleteSoftDeletesButKeepsDatabaseRow()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Estimate lifecycle customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Estimate lifecycle object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = EstimateZones,
            workItems = new[]
            {
                new
                {
                    name = "Painting",
                    quantity = 10m,
                    measurementUnit = "m2",
                    unitPrice = 100m
                }
            },
            materialItems = new[]
            {
                new
                {
                    name = "Paint",
                    quantity = 3m,
                    measurementUnit = "l",
                    unitPrice = 250m
                }
            }
        });

        var deleteResponse = await client.DeleteAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var detailResponse = await client.GetAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        using var detailDocument = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        var detail = detailDocument.RootElement.GetProperty("data");
        Assert.True(detail.GetProperty("isDeleted").GetBoolean());
        Assert.True(detail.TryGetProperty("deletedAt", out var deletedAt));
        Assert.Equal(JsonValueKind.String, deletedAt.ValueKind);

        var snapshot = await GetEstimateSnapshotAsync(estimate.GetProperty("id").GetGuid());
        Assert.NotNull(snapshot);
        Assert.True(snapshot!.IsDeleted);
        Assert.Equal(2, snapshot.ZoneCount);
        Assert.Equal(1, snapshot.WorkItemCount);
        Assert.Equal(1, snapshot.MaterialItemCount);
    }

    [Fact]
    public async Task SoftDeletedEstimatePermanentDeleteRemovesAggregateRows()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Permanent delete customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Permanent delete object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-002",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = EstimateZones,
            workItems = new[]
            {
                new
                {
                    name = "Plastering",
                    quantity = 5m,
                    measurementUnit = "m2",
                    unitPrice = 180m
                }
            },
            materialItems = new[]
            {
                new
                {
                    name = "Primer",
                    quantity = 2m,
                    measurementUnit = "l",
                    unitPrice = 90m
                }
            }
        });

        var softDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, softDeleteResponse.StatusCode);

        var permanentDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}/permanent");
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        Assert.False(await EstimateExistsInDatabaseAsync(estimate.GetProperty("id").GetGuid()));
        Assert.Equal(0, await CountEstimateZonesAsync(estimate.GetProperty("id").GetGuid()));
        Assert.Equal(0, await CountEstimateWorkItemsAsync(estimate.GetProperty("id").GetGuid()));
        Assert.Equal(0, await CountEstimateMaterialItemsAsync(estimate.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ObjectWithEstimateCannotBeDeletedPermanently()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Estimate owner"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Protected estimate object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "DELETE-GUARD-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        var deleteResponse = await client.DeleteAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
        await AssertApiErrorAsync(deleteResponse, "ObjectHasEstimates");

        var estimateResponse = await client.GetAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, estimateResponse.StatusCode);

        var objectResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, objectResponse.StatusCode);
        Assert.True(await ObjectExistsInDatabaseAsync(estimateObject.GetProperty("id").GetGuid()));
        Assert.True(await EstimateExistsInDatabaseAsync(estimate.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ObjectWithSoftDeletedEstimateReturnsConflictInsteadOfServerError()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Soft delete owner"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Soft delete protected object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "DELETE-GUARD-ARCHIVED-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        var deleteEstimateResponse = await client.DeleteAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, deleteEstimateResponse.StatusCode);

        var deleteObjectResponse = await client.DeleteAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.Conflict, deleteObjectResponse.StatusCode);
        await AssertApiErrorAsync(deleteObjectResponse, "ObjectHasEstimates");

        var getObjectResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, getObjectResponse.StatusCode);
        Assert.True(await ObjectExistsInDatabaseAsync(estimateObject.GetProperty("id").GetGuid()));
        Assert.True(await EstimateExistsInDatabaseAsync(estimate.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ArchivedObjectAppearsOnlyInArchivedFilterAndCanBeRestored()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Archived object customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Archived object",
            objectType = "Apartment"
        });

        var archiveResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);

        Assert.DoesNotContain(
            await GetBusinessIdsAsync(client, "/api/v1/objects?status=active"),
            id => id == estimateObject.GetProperty("id").GetGuid());
        Assert.Contains(
            await GetBusinessIdsAsync(client, "/api/v1/objects?status=archived"),
            id => id == estimateObject.GetProperty("id").GetGuid());

        var restoreResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        Assert.Contains(
            await GetBusinessIdsAsync(client, "/api/v1/objects?status=active"),
            id => id == estimateObject.GetProperty("id").GetGuid());
        Assert.DoesNotContain(
            await GetBusinessIdsAsync(client, "/api/v1/objects?status=archived"),
            id => id == estimateObject.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task RestoreObjectDoesNotRestoreSoftDeletedEstimate()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Restore object customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Restore object target",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "RESTORE-OBJECT-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        var softDeleteEstimateResponse = await client.DeleteAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, softDeleteEstimateResponse.StatusCode);

        var archiveObjectResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveObjectResponse.StatusCode);

        var restoreObjectResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreObjectResponse.StatusCode);

        var estimateSnapshot = await GetEstimateSnapshotAsync(estimate.GetProperty("id").GetGuid());
        Assert.NotNull(estimateSnapshot);
        Assert.True(estimateSnapshot!.IsDeleted);
    }

    [Fact]
    public async Task ObjectCanBeDeletedAfterPermanentDeletionOfLastEstimate()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Last estimate customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Last estimate object",
            objectType = "Apartment"
        });
        var firstEstimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-003",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });
        var secondEstimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-004",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        foreach (var estimateId in new[]
                 {
                     firstEstimate.GetProperty("id").GetString(),
                     secondEstimate.GetProperty("id").GetString()
                 })
        {
            var softDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{estimateId}");
            Assert.Equal(HttpStatusCode.NoContent, softDeleteResponse.StatusCode);

            var permanentDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{estimateId}/permanent");
            Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);
        }

        var deleteObjectResponse = await client.DeleteAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, deleteObjectResponse.StatusCode);
        Assert.False(await ObjectExistsInDatabaseAsync(estimateObject.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task PermanentDeleteDoesNotAffectOtherEstimateOrObject()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Isolation customer"
        });
        var firstObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Isolation object one",
            objectType = "Apartment"
        });
        var secondObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Isolation object two",
            objectType = "Office"
        });
        var deletedEstimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-005",
            objectId = firstObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });
        var survivingEstimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "EST-LIFECYCLE-006",
            objectId = secondObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        var softDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{deletedEstimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, softDeleteResponse.StatusCode);

        var permanentDeleteResponse = await client.DeleteAsync($"/api/v1/estimates/{deletedEstimate.GetProperty("id").GetString()}/permanent");
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        Assert.False(await EstimateExistsInDatabaseAsync(deletedEstimate.GetProperty("id").GetGuid()));
        Assert.True(await EstimateExistsInDatabaseAsync(survivingEstimate.GetProperty("id").GetGuid()));
        Assert.True(await ObjectExistsInDatabaseAsync(firstObject.GetProperty("id").GetGuid()));
        Assert.True(await ObjectExistsInDatabaseAsync(secondObject.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task ArchivingCustomerDoesNotDeleteObjects()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Archive customer"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Object survives archive",
            objectType = "Apartment"
        });

        var archiveResponse = await client.PatchAsync($"/api/v1/customers/{customer.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        using var archiveDocument = JsonDocument.Parse(await archiveResponse.Content.ReadAsStringAsync());
        Assert.True(archiveDocument.RootElement.GetProperty("data").GetProperty("isArchived").GetBoolean());

        var objectResponse = await client.GetAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, objectResponse.StatusCode);
    }

    [Fact]
    public async Task ArchivingObjectDoesNotDeleteEstimates()
    {
        using var client = Factory.CreateClient();
        var customer = await CreateAsync(client, "/api/v1/customers", new
        {
            name = "Archive object owner"
        });
        var estimateObject = await CreateAsync(client, "/api/v1/objects", new
        {
            customerId = customer.GetProperty("id").GetString(),
            name = "Archive object",
            objectType = "Apartment"
        });
        var estimate = await CreateAsync(client, "/api/v1/estimates", new
        {
            estimateNumber = "ARCHIVE-OBJECT-001",
            objectId = estimateObject.GetProperty("id").GetString(),
            currency = "UAH",
            zones = SingleEstimateZone
        });

        var archiveResponse = await client.PatchAsync($"/api/v1/objects/{estimateObject.GetProperty("id").GetString()}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        using var archiveDocument = JsonDocument.Parse(await archiveResponse.Content.ReadAsStringAsync());
        Assert.True(archiveDocument.RootElement.GetProperty("data").GetProperty("isArchived").GetBoolean());

        var estimateResponse = await client.GetAsync($"/api/v1/estimates/{estimate.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.OK, estimateResponse.StatusCode);
    }

    private static async Task<JsonElement> CreateAsync(HttpClient client, string path, object request)
    {
        var response = await client.PostAsJsonAsync(path, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private static async Task PutOkAsync(HttpClient client, string path, object request)
    {
        var response = await client.PutAsJsonAsync(path, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonElement> GetEstimateDataAsync(HttpClient client, Guid estimateId, string acceptLanguage)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/estimates/{estimateId}");
        request.Headers.AcceptLanguage.ParseAdd(acceptLanguage);

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private static void AssertEstimateNames(JsonElement estimate, string materialName, string workName)
    {
        var material = Assert.Single(estimate.GetProperty("materialItems").EnumerateArray());
        var work = Assert.Single(estimate.GetProperty("workItems").EnumerateArray());

        Assert.Equal(materialName, material.GetProperty("name").GetString());
        Assert.Equal("KnowledgeSnapshot", material.GetProperty("nameSource").GetString());
        Assert.Equal(workName, work.GetProperty("name").GetString());
        Assert.Equal("KnowledgeSnapshot", work.GetProperty("nameSource").GetString());
    }

    private static async Task<int> GetTotalCountAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32();
    }

    private static async Task<IReadOnlyCollection<Guid>> GetBusinessIdsAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data")
            .GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .ToArray();
    }

    private static async Task AssertApiErrorAsync(HttpResponseMessage response, string code)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = document.RootElement.GetProperty("error");

        Assert.Equal(code, error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("message").GetString()));
    }

    private async Task<bool> CustomerExistsInDatabaseAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.Customers.IgnoreQueryFilters().AnyAsync(customer => customer.Id == id);
    }

    private async Task<bool> ObjectExistsInDatabaseAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.EstimateObjects.IgnoreQueryFilters().AnyAsync(estimateObject => estimateObject.Id == id);
    }

    private async Task<bool> EstimateExistsInDatabaseAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.Estimates.IgnoreQueryFilters().AnyAsync(estimate => estimate.Id == id);
    }

    private async Task<EstimateSnapshot?> GetEstimateSnapshotAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();

        var estimate = await dbContext.Estimates
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(value => value.Id == id);

        if (estimate is null)
        {
            return null;
        }

        var zoneCount = await dbContext.EstimateZones.CountAsync(zone => zone.EstimateId == id);
        var workItemCount = await dbContext.Set<SmartEstimate.Domain.Estimates.EstimateWorkItem>().CountAsync(item => item.EstimateId == id);
        var materialItemCount = await dbContext.Set<SmartEstimate.Domain.Estimates.EstimateMaterialItem>().CountAsync(item => item.EstimateId == id);

        return new EstimateSnapshot(
            estimate.Id,
            estimate.IsDeleted,
            estimate.ObjectId,
            zoneCount,
            workItemCount,
            materialItemCount);
    }

    private async Task<int> CountEstimateZonesAsync(Guid estimateId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.EstimateZones.CountAsync(zone => zone.EstimateId == estimateId);
    }

    private async Task<int> CountEstimateWorkItemsAsync(Guid estimateId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.Set<SmartEstimate.Domain.Estimates.EstimateWorkItem>().CountAsync(item => item.EstimateId == estimateId);
    }

    private async Task<int> CountEstimateMaterialItemsAsync(Guid estimateId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        return await dbContext.Set<SmartEstimate.Domain.Estimates.EstimateMaterialItem>().CountAsync(item => item.EstimateId == estimateId);
    }

    private async Task<LocalizedItemSnapshot> GetLocalizedItemSnapshotAsync(Guid estimateId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
        var material = await dbContext.Set<EstimateMaterialItem>().SingleAsync(item => item.EstimateId == estimateId);
        var work = await dbContext.Set<EstimateWorkItem>().SingleAsync(item => item.EstimateId == estimateId);

        return new LocalizedItemSnapshot(
            material.NameSnapshot,
            material.NameSource,
            work.NameSnapshot,
            work.NameSource);
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

    private sealed record EstimateSnapshot(
        Guid Id,
        bool IsDeleted,
        Guid ObjectId,
        int ZoneCount,
        int WorkItemCount,
        int MaterialItemCount);

    private sealed record LocalizedItemSnapshot(
        LocalizedNameSnapshot? MaterialNameSnapshot,
        EstimateItemNameSource MaterialNameSource,
        LocalizedNameSnapshot? WorkNameSnapshot,
        EstimateItemNameSource WorkNameSource);

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
