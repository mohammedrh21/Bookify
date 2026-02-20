using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Bookify.Client.Auth;

/// <summary>
/// Lightweight JWT parser — no System.IdentityModel.Tokens.Jwt dependency.
/// Works in browser-wasm because it only uses BCL APIs.
/// </summary>
public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return Enumerable.Empty<Claim>();

        var payload = parts[1];

        // Fix base64url padding
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "=";  break;
        }
        payload = payload.Replace('-', '+').Replace('_', '/');

        var jsonBytes = Convert.FromBase64String(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        if (keyValuePairs is null) return Enumerable.Empty<Claim>();

        var claims = new List<Claim>();
        foreach (var (key, value) in keyValuePairs)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    claims.Add(new Claim(key, item.ToString()));
            }
            else
            {
                claims.Add(new Claim(key, value.ToString()));
            }
        }
        return claims;
    }

    public static DateTime GetExpiry(string jwt)
    {
        var claims = ParseClaims(jwt);
        var exp    = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (exp is null || !long.TryParse(exp, out var expSeconds))
            return DateTime.MinValue;

        return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
    }
}
