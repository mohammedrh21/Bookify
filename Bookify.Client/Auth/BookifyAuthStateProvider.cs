using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Bookify.Client.Auth;

public class BookifyAuthStateProvider(ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorage.GetItemAsync<string>("access_token");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            // Check expiry using our lightweight parser (no System.IdentityModel.Tokens.Jwt)
            if (JwtParser.GetExpiry(token) < DateTime.UtcNow)
            {
                await localStorage.RemoveItemAsync("access_token");
                return new AuthenticationState(_anonymous);
            }

            var claims  = JwtParser.ParseClaims(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user    = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthenticated(string token)
    {
        var claims   = JwtParser.ParseClaims(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user     = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
        => NotifyAuthenticationStateChanged(
               Task.FromResult(new AuthenticationState(_anonymous)));
}
