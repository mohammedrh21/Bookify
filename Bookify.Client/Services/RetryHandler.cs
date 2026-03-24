namespace Bookify.Client.Services;

/// <summary>
/// DelegatingHandler that retries outgoing HTTP requests when the API is
/// unreachable (e.g. the server hasn't finished starting yet).
///
/// This solves the startup race-condition where the Blazor WASM client loads
/// faster than the API and the first fetch attempt returns "Failed to fetch".
/// </summary>
public class RetryHandler : DelegatingHandler
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                // Connection refused / failed to fetch — the API is probably
                // still starting.  Wait briefly and try again.
                Console.WriteLine(
                    $"[RetryHandler] API unreachable (attempt {attempt + 1}/{MaxRetries + 1}). " +
                    $"Retrying in {Delay.TotalSeconds}s…");

                await Task.Delay(Delay, cancellationToken);
            }
        }
    }
}
