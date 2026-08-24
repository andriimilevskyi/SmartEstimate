using SmartEstimate.Application.Estimates;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Domain.Objects;
using Xunit;

namespace SmartEstimate.UnitTests.Estimates;

public sealed class EstimateTests
{
    [Fact]
    public void AddItemsRecalculatesTotalsAcrossWorkAndMaterials()
    {
        var createdAt = new DateTimeOffset(2026, 8, 3, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-0001"), Guid.NewGuid(), "uah", null, createdAt);

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
            Guid.NewGuid(),
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
    public void EstimateObjectStoresConstructionObjectFields()
    {
        var createdAt = new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero);

        var estimateObject = EstimateObject.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " Квартира Антоновича ",
            EstimateObjectType.Apartment,
            " Київ, вул. Антоновича, 44 ",
            86.5m,
            " Капітальний ремонт ",
            createdAt);

        Assert.Equal("Квартира Антоновича", estimateObject.Name);
        Assert.Equal(EstimateObjectType.Apartment, estimateObject.ObjectType);
        Assert.Equal("Київ, вул. Антоновича, 44", estimateObject.Address);
        Assert.Equal(86.5m, estimateObject.TotalArea);
        Assert.Equal("Капітальний ремонт", estimateObject.Description);
    }

    [Fact]
    public void CreateInitializesZonesWithoutVersionBump()
    {
        var createdAt = new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero);

        var estimate = Estimate.Create(
            new EstimateNumber("EST-ZONES-001"),
            Guid.NewGuid(),
            "UAH",
            " Капітальний ремонт ",
            ["Кухня", "Спальня", "спальня"],
            createdAt);

        Assert.Equal(EstimateStatus.Draft, estimate.Status);
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
            Guid.NewGuid(),
            "UAH",
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
        var estimate = Estimate.Create(new EstimateNumber("EST-0003"), Guid.NewGuid(), "UAH", null, createdAt);

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

    [Fact]
    public void EstimateMaterialItemKeepsPriceSourceSnapshotAndTracksManualOverride()
    {
        var createdAt = new DateTimeOffset(2026, 8, 14, 9, 0, 0, TimeSpan.Zero);
        var sourcePriceId = Guid.NewGuid();
        var estimate = Estimate.Create(new EstimateNumber("EST-PRICE-001"), Guid.NewGuid(), "UAH", null, createdAt);

        estimate.AddMaterialItem(
            "Ґрунтовка",
            new Quantity(10m),
            new MeasurementUnit("л"),
            new Money(130m, "UAH"),
            null,
            createdAt,
            "primer",
            sourcePriceId: sourcePriceId,
            priceCapturedAt: createdAt);

        var item = Assert.Single(estimate.MaterialItems);
        Assert.Equal(sourcePriceId, item.SourcePriceId);
        Assert.Equal(createdAt, item.PriceCapturedAt);
        Assert.False(item.IsUnitPriceManuallyOverridden);

        estimate.UpdateMaterialItem(item.Id, new Quantity(10m), new Money(125m, "UAH"), null, createdAt.AddMinutes(1));

        Assert.True(item.IsUnitPriceManuallyOverridden);
        Assert.Equal(sourcePriceId, item.SourcePriceId);
    }

    [Fact]
    public void CatalogMaterialStoresLocalizedNameSnapshotAndSource()
    {
        var createdAt = new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-LOC-001"), Guid.NewGuid(), "UAH", null, createdAt);
        var snapshot = new LocalizedNameSnapshot("Гіпсокартон", "Drywall", "Gipskarton");

        estimate.AddMaterialItem(
            snapshot.Uk,
            new Quantity(12m),
            new MeasurementUnit("m²"),
            new Money(180m, "UAH"),
            null,
            createdAt,
            "material-drywall",
            nameSnapshot: snapshot,
            nameSource: EstimateItemNameSource.KnowledgeSnapshot);

        var item = Assert.Single(estimate.MaterialItems);
        Assert.Equal(EstimateItemNameSource.KnowledgeSnapshot, item.NameSource);
        Assert.NotNull(item.NameSnapshot);
        Assert.Equal("Гіпсокартон", item.NameSnapshot.Uk);
        Assert.Equal("Drywall", item.NameSnapshot.En);
        Assert.Equal("Gipskarton", item.NameSnapshot.De);
        Assert.Equal("Гіпсокартон", EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, EstimateDisplayLocale.Uk));
        Assert.Equal("Drywall", EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, EstimateDisplayLocale.En));
        Assert.Equal("Gipskarton", EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, EstimateDisplayLocale.De));
    }

    [Fact]
    public void CatalogWorkStoresLocalizedNameSnapshotAndSource()
    {
        var createdAt = new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-LOC-002"), Guid.NewGuid(), "UAH", null, createdAt);
        var snapshot = new LocalizedNameSnapshot("Монтаж гіпсокартону", "Drywall installation", "Gipskarton montieren");

        estimate.AddWorkItem(
            snapshot.Uk,
            new Quantity(12m),
            new MeasurementUnit("m²"),
            new Money(300m, "UAH"),
            null,
            createdAt,
            "work-drywall-installation",
            nameSnapshot: snapshot,
            nameSource: EstimateItemNameSource.KnowledgeSnapshot);

        var item = Assert.Single(estimate.WorkItems);
        Assert.Equal(EstimateItemNameSource.KnowledgeSnapshot, item.NameSource);
        Assert.NotNull(item.NameSnapshot);
        Assert.Equal("Монтаж гіпсокартону", item.NameSnapshot.Uk);
        Assert.Equal("Drywall installation", item.NameSnapshot.En);
        Assert.Equal("Gipskarton montieren", item.NameSnapshot.De);
        Assert.Equal("Drywall installation", EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, EstimateDisplayLocale.En));
    }

    [Fact]
    public void LegacyAndCustomNamesFallbackToStoredNameForEveryLocale()
    {
        var createdAt = new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-LOC-003"), Guid.NewGuid(), "UAH", null, createdAt);

        estimate.AddMaterialItem(
            "Гіпсокартон",
            new Quantity(1m),
            new MeasurementUnit("лист"),
            new Money(100m, "UAH"),
            null,
            createdAt,
            "legacy-material");
        estimate.AddWorkItem(
            "Авторська назва роботи",
            new Quantity(1m),
            new MeasurementUnit("шт"),
            new Money(100m, "UAH"),
            null,
            createdAt);

        var legacyItem = Assert.Single(estimate.MaterialItems);
        var customItem = Assert.Single(estimate.WorkItems);

        Assert.Equal(EstimateItemNameSource.Legacy, legacyItem.NameSource);
        Assert.Null(legacyItem.NameSnapshot);
        Assert.All(
            new[] { EstimateDisplayLocale.Uk, EstimateDisplayLocale.En, EstimateDisplayLocale.De },
            locale => Assert.Equal("Гіпсокартон", EstimateDisplayNameResolver.Resolve(legacyItem.Name, legacyItem.NameSnapshot, locale)));

        Assert.Equal(EstimateItemNameSource.Custom, customItem.NameSource);
        Assert.Null(customItem.NameSnapshot);
        Assert.All(
            new[] { EstimateDisplayLocale.Uk, EstimateDisplayLocale.En, EstimateDisplayLocale.De },
            locale => Assert.Equal("Авторська назва роботи", EstimateDisplayNameResolver.Resolve(customItem.Name, customItem.NameSnapshot, locale)));
    }

    [Fact]
    public void DuplicatePreservesLocalizedSnapshotAndNameSource()
    {
        var createdAt = new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);
        var estimate = Estimate.Create(new EstimateNumber("EST-LOC-004"), Guid.NewGuid(), "UAH", null, createdAt);
        var snapshot = new LocalizedNameSnapshot("Гіпсокартон", "Drywall", "Gipskarton");

        estimate.AddMaterialItem(
            snapshot.Uk,
            new Quantity(2m),
            new MeasurementUnit("лист"),
            new Money(240m, "UAH"),
            null,
            createdAt,
            "material-drywall",
            nameSnapshot: snapshot,
            nameSource: EstimateItemNameSource.KnowledgeSnapshot);

        var original = Assert.Single(estimate.MaterialItems);
        estimate.DuplicateMaterialItem(original.Id, createdAt.AddMinutes(1));

        Assert.All(estimate.MaterialItems, item =>
        {
            Assert.Equal(EstimateItemNameSource.KnowledgeSnapshot, item.NameSource);
            Assert.NotNull(item.NameSnapshot);
            Assert.Equal("Drywall", item.NameSnapshot.En);
            Assert.Equal("Gipskarton", item.NameSnapshot.De);
        });
    }
}
