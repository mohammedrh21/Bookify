using Bookify.Client.Models;
using Bookify.Client.Models.Booking;
using System.Net.Http.Json;

namespace Bookify.Client.Services
{
    public class CheckoutSessionResponse
    {
        public string CheckoutUrl { get; set; } = string.Empty;
    }

    public class ConfirmCheckoutRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public interface IPaymentApiService
    {
        Task<ApiResult<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateBookingRequest request);
        Task<ApiResult<bool>> ConfirmCheckoutSessionAsync(string sessionId);
    }

    public class PaymentApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), IPaymentApiService
    {
        public async Task<ApiResult<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateBookingRequest request)
        {
            return await PostAsync<CreateBookingRequest, CheckoutSessionResponse>(
                "api/payments/create-checkout-session", request, "Failed to initialize fast checkout.");
        }

        public async Task<ApiResult<bool>> ConfirmCheckoutSessionAsync(string sessionId)
        {
            var req = new ConfirmCheckoutRequest { SessionId = sessionId };
            return await PostAsync<ConfirmCheckoutRequest, bool>(
                "api/payments/confirm-checkout", req, "Failed to confirm payment checkout.");
        }
    }
}
