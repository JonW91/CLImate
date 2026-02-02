using CLImate.App.Services;

namespace CLImate.Tests.Services;

public sealed class CountryCodeCatalogueTests
{
    private readonly CountryCodeCatalogue _catalogue = new();

    [Theory]
    [InlineData("GB")]
    [InlineData("US")]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("JP")]
    [InlineData("AU")]
    [InlineData("CA")]
    [InlineData("NZ")]
    public void IsValidCode_WithValidIsoCodes_ReturnsTrue(string code)
    {
        Assert.True(_catalogue.IsValidCode(code));
    }

    [Theory]
    [InlineData("gb")]
    [InlineData("us")]
    [InlineData("Gb")]
    [InlineData("gB")]
    public void IsValidCode_IsCaseInsensitive(string code)
    {
        Assert.True(_catalogue.IsValidCode(code));
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("ZZ")]
    [InlineData("AA")]
    public void IsValidCode_WithInvalidCodes_ReturnsFalse(string code)
    {
        Assert.False(_catalogue.IsValidCode(code));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsValidCode_WithEmptyOrNull_ReturnsFalse(string? code)
    {
        Assert.False(_catalogue.IsValidCode(code!));
    }

    [Theory]
    [InlineData("GBR")]
    [InlineData("USA")]
    [InlineData("G")]
    public void IsValidCode_WithWrongLength_ReturnsFalse(string code)
    {
        Assert.False(_catalogue.IsValidCode(code));
    }

    [Theory]
    [InlineData("united states", "US")]
    [InlineData("usa", "US")]
    [InlineData("United Kingdom", "GB")]
    [InlineData("uk", "GB")]
    [InlineData("england", "GB")]
    [InlineData("scotland", "GB")]
    [InlineData("wales", "GB")]
    [InlineData("ireland", "IE")]
    public void GetCode_WithCommonNames_ReturnsCorrectCode(string name, string expected)
    {
        var result = _catalogue.GetCode(name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetCode_IsCaseInsensitive()
    {
        Assert.Equal("US", _catalogue.GetCode("UNITED STATES"));
        Assert.Equal("US", _catalogue.GetCode("United States"));
        Assert.Equal("US", _catalogue.GetCode("united states"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void GetCode_WithEmptyOrWhitespace_ReturnsNull(string name)
    {
        Assert.Null(_catalogue.GetCode(name));
    }

    [Fact]
    public void GetCode_WithUnknownName_ReturnsNull()
    {
        Assert.Null(_catalogue.GetCode("Not A Real Country"));
    }

    [Fact]
    public void GetAllCodes_ReturnsNonEmptySet()
    {
        var codes = _catalogue.GetAllCodes();
        
        Assert.NotEmpty(codes);
        Assert.Contains("GB", codes);
        Assert.Contains("US", codes);
        Assert.Contains("DE", codes);
    }

    [Fact]
    public void GetAllCodes_ContainsAllMajorCountries()
    {
        var codes = _catalogue.GetAllCodes();
        
        var majorCountries = new[] { "GB", "US", "CA", "AU", "NZ", "DE", "FR", "ES", "IT", "JP", "CN", "IN", "BR", "MX" };
        
        foreach (var country in majorCountries)
        {
            Assert.Contains(country, codes);
        }
    }

    [Fact]
    public void GetAllCodes_AllCodesAreTwoCharacters()
    {
        var codes = _catalogue.GetAllCodes();
        
        foreach (var code in codes)
        {
            Assert.Equal(2, code.Length);
            Assert.True(code.All(char.IsLetter));
        }
    }
}
