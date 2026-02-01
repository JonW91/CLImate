namespace CLImate.App.Models;

public sealed class GeocodeResponse
{
    public List<GeoResult>? Results { get; set; }
}

public sealed class GeoResult
{
    public string? Name { get; set; }
    public string? Admin1 { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
}
