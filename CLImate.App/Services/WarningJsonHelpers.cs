using System.Text.Json;

namespace CLImate.App.Services;

internal static class WarningJsonHelpers
{
    internal static string? GetString(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }
        return null;
    }

    internal static DateTimeOffset? ParseDate(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var raw = GetString(element, name);
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            if (DateTimeOffset.TryParse(raw, out var dto))
                return dto;
        }
        return null;
    }
}
