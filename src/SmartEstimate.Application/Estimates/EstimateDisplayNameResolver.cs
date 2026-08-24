using SmartEstimate.Domain.Estimates;

namespace SmartEstimate.Application.Estimates;

public static class EstimateDisplayNameResolver
{
    public static string Resolve(
        string legacyName,
        LocalizedNameSnapshot? snapshot,
        EstimateDisplayLocale locale)
    {
        if (snapshot is null)
        {
            return legacyName;
        }

        var value = locale switch
        {
            EstimateDisplayLocale.En => snapshot.En,
            EstimateDisplayLocale.De => snapshot.De,
            _ => snapshot.Uk
        };

        return string.IsNullOrWhiteSpace(value) ? legacyName : value;
    }
}
