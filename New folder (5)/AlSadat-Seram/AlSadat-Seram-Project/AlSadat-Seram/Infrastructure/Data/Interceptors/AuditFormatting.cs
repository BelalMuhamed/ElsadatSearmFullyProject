using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Infrastructure.Data.Interceptors;

/// <summary>How values are written into <c>AuditEntry</c> text columns.</summary>
internal static class AuditFormatting
{
    /// <summary>camelCase <c>{ "field", "old", "new" }</c>; Arabic kept readable in the column.</summary>
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    /// <summary>Cuts a value to the column's length instead of failing the business save.</summary>
    public static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
