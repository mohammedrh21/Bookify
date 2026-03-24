using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Bookify.Client.Services;

/// <summary>
/// DelegatingHandler that automatically reads the JWT from local-storage and
/// attaches it as a Bearer token to every outgoing HTTP request.
///
/// This replaces the per-service SetAuthHeaderAsync() pattern, which was
/// (a) copy-pasted across every service file and
/// (b) unsafe because it mutated the shared HttpClient.DefaultRequestHeaders
///     — a race condition when multiple requests fired concurrently.
/// </summary>
public class AuthBearerHandler(ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await localStorage.GetItemAsync<string>("access_token", cancellationToken);

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Set on the request message (not on DefaultRequestHeaders), so
            // each concurrent request carries its own header — thread-safe.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
