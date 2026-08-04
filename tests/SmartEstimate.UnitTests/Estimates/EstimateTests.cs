using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using Xunit;

namespace SmartEstimate.UnitTests.Estimates;

public sealed class EstimateTests
{
    [Fact]
    public void AddItemsRecalculatesTotalsAcrossWorkAndMaterials()
    {
        var createdAt = new DateTimeOffset(2026, 8, 3, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-0001"), "uah", null, createdAt);

        estimate.AddWorkItem(
            "Wall painting",
            new Quantity(12.5m),
            new MeasurementUnit("m2"),
            new Money(80m, "UAH"),
            null,
            createdAt.AddMinutes(1));
        estimate.AddMaterialItem(
            "Interior paint",
            new Quantity(2m),
            new MeasurementUnit("l"),
            new Money(350m, "UAH"),
            null,
            createdAt.AddMinutes(2));

        Assert.Equal(1_000m, estimate.TotalLabor.Amount);
        Assert.Equal(700m, estimate.TotalMaterials.Amount);
        Assert.Equal(1_700m, estimate.GrandTotal.Amount);
        Assert.Equal("UAH", estimate.GrandTotal.Currency);
        Assert.Single(estimate.WorkItems);
        Assert.Single(estimate.MaterialItems);
    }

    [Fact]
    public void AddWorkItemWithDifferentCurrencyRejectsAggregateInvariant()
    {
        var estimate = Estimate.Create(
            new EstimateNumber("EST-0002"),
            "UAH",
            null,
            DateTimeOffset.UtcNow);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => estimate.AddWorkItem(
            "Wall painting",
            new Quantity(1m),
            new MeasurementUnit("m2"),
            new Money(100m, "USD"),
            null,
            DateTimeOffset.UtcNow));

        Assert.Equal("unitPrice", exception.ParamName);
        Assert.Empty(estimate.WorkItems);
    }

    [Fact]
    public void CreateWithObjectConfigurationInitializesZonesWithoutVersionBump()
    {
        var createdAt = new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero);

        var estimate = Estimate.Create(
            new EstimateNumber("EST-ZONES-001"),
            "UAH",
            EstimateObjectType.Apartment,
            " Київ, вул. Антоновича, 44 ",
            86.5m,
            " Капітальний ремонт ",
            ["Кухня", "Спальня", "спальня"],
            createdAt);

        Assert.Equal(EstimateObjectType.Apartment, estimate.ObjectType);
        Assert.Equal("Київ, вул. Антоновича, 44", estimate.ObjectAddress);
        Assert.Equal(86.5m, estimate.TotalArea);
        Assert.Equal("Капітальний ремонт", estimate.Notes);
        Assert.Equal(1, estimate.Version);
        Assert.Collection(
            estimate.Zones.OrderBy(zone => zone.SortOrder),
            zone => Assert.Equal("Кухня", zone.Name),
            zone => Assert.Equal("Спальня", zone.Name));
    }

    [Fact]
    public void ZoneOperationsKeepItemsScopedAndRecalculateTotals()
    {
        var createdAt = new DateTimeOffset(2026, 8, 4, 10, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(
            new EstimateNumber("EST-ZONES-002"),
            "UAH",
            EstimateObjectType.Apartment,
            null,
            null,
            null,
            ["Кухня", "Спальня"],
            createdAt);
        var kitchenId = estimate.Zones.Single(zone => zone.Name == "Кухня").Id;
        var bedroomId = estimate.Zones.Single(zone => zone.Name == "Спальня").Id;

        estimate.AddWorkItem(
            "Штукатурка стін",
            new Quantity(20m),
            new MeasurementUnit("м²"),
            new Money(280m, "UAH"),
            "Під маяки",
            createdAt.AddMinutes(1),
            zoneId: kitchenId);
        estimate.AddMaterialItem(
            "Штукатурна суміш",
            new Quantity(10m),
            new MeasurementUnit("міш"),
            new Money(190m, "UAH"),
            null,
            createdAt.AddMinutes(2),
            zoneId: bedroomId);

        var workItem = Assert.Single(estimate.WorkItems);
        estimate.DuplicateWorkItem(workItem.Id, createdAt.AddMinutes(3));
        estimate.RenameZone(kitchenId, "Кухня-студія", createdAt.AddMinutes(4));
        estimate.ReorderZones([bedroomId, kitchenId], createdAt.AddMinutes(5));
        estimate.RemoveZone(bedroomId, createdAt.AddMinutes(6));

        Assert.Equal("Кухня-студія", Assert.Single(estimate.Zones).Name);
        Assert.Equal(2, estimate.WorkItems.Count);
        Assert.Empty(estimate.MaterialItems);
        Assert.All(estimate.WorkItems, item => Assert.Equal(kitchenId, item.ZoneId));
        Assert.Equal(11_200m, estimate.TotalLabor.Amount);
        Assert.Equal(0m, estimate.TotalMaterials.Amount);
        Assert.Equal(11_200m, estimate.GrandTotal.Amount);
    }

    [Fact]
    public void UpdateAndRemoveItemsRecalculateTotalsAndKeepCatalogSnapshot()
    {
        var createdAt = new DateTimeOffset(2026, 8, 3, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-0003"), "UAH", null, createdAt);

        estimate.AddWorkItem(
            "Фарбування стін",
            new Quantity(20m),
            new MeasurementUnit("м²"),
            new Money(250m, "UAH"),
            null,
            createdAt.AddMinutes(1),
            "painting");
        estimate.AddMaterialItem(
            "Фарба",
            new Quantity(6m),
            new MeasurementUnit("л"),
            new Money(350m, "UAH"),
            null,
            createdAt.AddMinutes(2),
            "paint");

        EstimateWorkItem workItem = Assert.Single(estimate.WorkItems);
        EstimateMaterialItem materialItem = Assert.Single(estimate.MaterialItems);

        estimate.UpdateWorkItem(
            workItem.Id,
            new Quantity(20m),
            new Money(275m, "UAH"),
            "Two coats",
            createdAt.AddMinutes(3));
        estimate.RemoveMaterialItem(materialItem.Id, createdAt.AddMinutes(4));

        Assert.Equal("painting", workItem.KnowledgeItemId);
        Assert.Equal("Фарбування стін", workItem.Name);
        Assert.Equal("м²", workItem.MeasurementUnit.Value);
        Assert.Equal(5_500m, estimate.TotalLabor.Amount);
        Assert.Equal(0m, estimate.TotalMaterials.Amount);
        Assert.Equal(5_500m, estimate.GrandTotal.Amount);
        Assert.Empty(estimate.MaterialItems);
    }
}
