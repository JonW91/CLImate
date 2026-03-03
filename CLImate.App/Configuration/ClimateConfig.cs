namespace CLImate.App.Configuration;

public sealed class ClimateConfig
{
    public Models.Units DefaultUnits { get; set; } = Models.Units.Metric;
    public string? DefaultCountry { get; set; }
    public bool ShowArt { get; set; } = true;
    public bool UseColour { get; set; } = true;
    public List<FavouriteLocation> FavouriteLocations { get; set; } = new();
}

public sealed class FavouriteLocation
{
    public required string Name { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
}
