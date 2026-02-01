using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface ILocationFormatter
{
    string Format(GeoResult result);
}

public sealed class LocationFormatter : ILocationFormatter
{
    public string Format(GeoResult result)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(result.Name)) parts.Add(result.Name);
        if (!string.IsNullOrWhiteSpace(result.Admin1)) parts.Add(result.Admin1);
        if (!string.IsNullOrWhiteSpace(result.Country)) parts.Add(result.Country);
        return string.Join(", ", parts);
    }
}
