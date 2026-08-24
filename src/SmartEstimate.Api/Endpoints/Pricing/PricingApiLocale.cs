using SmartEstimate.Application.Pricing;

namespace SmartEstimate.Api.Endpoints.Pricing;

internal static class PricingApiLocale
{
    public static PricingDisplayLocale Resolve(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        return Parse(header);
    }

    private static PricingDisplayLocale Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return PricingDisplayLocale.Uk;
        }

        var language = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .ToLowerInvariant();

        return language switch
        {
            "en" => PricingDisplayLocale.En,
            "de" => PricingDisplayLocale.De,
            _ => PricingDisplayLocale.Uk
        };
    }
}
