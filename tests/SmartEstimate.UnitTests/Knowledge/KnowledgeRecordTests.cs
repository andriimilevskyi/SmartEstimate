using SmartEstimate.Domain.Knowledge;
using Xunit;

namespace SmartEstimate.UnitTests.Knowledge;

public sealed class KnowledgeRecordTests
{
    [Fact]
    public void LocalizedTextUsesApprovedFallbackChain()
    {
        var text = new LocalizedText("Фарбування", null, null);

        Assert.Equal("Фарбування", text.Uk);
        Assert.Equal("Фарбування", text.En);
        Assert.Equal("Фарбування", text.De);
    }

    [Fact]
    public void ConstructionWorkDeduplicatesTagsAndTracksVersionWhenChanged()
    {
        var createdAt = new DateTimeOffset(2026, 8, 3, 10, 0, 0, TimeSpan.Zero);
        var work = ConstructionWork.Create(
            Guid.NewGuid(),
            new LocalizedText("Фарбування стін", "Wall painting", "Wände streichen"),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            ["painting", "Painting", "walls"],
            KnowledgeStatus.Draft,
            createdAt,
            null);

        work.Update(
            new LocalizedText("Фарбування стін", "Wall painting", "Wände streichen"),
            "Two coats",
            work.CategoryId,
            work.UnitId,
            ["painting", "walls"],
            KnowledgeStatus.Active,
            createdAt.AddMinutes(1),
            null);

        Assert.Equal(KnowledgeStatus.Active, work.Status);
        Assert.Equal(2, work.Version);
        Assert.Equal(["painting", "walls"], work.TagValues);
        Assert.Equal(createdAt.AddMinutes(1), work.UpdatedAt);
    }
}
