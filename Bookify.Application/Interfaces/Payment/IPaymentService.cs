using Bookify.Application.Common;
using Bookify.Domain.Entities;

namespace Bookify.Application.Interfaces.Payment
{
    public interface IPaymentService
    {
        /// <summary>Creates a Stripe Checkout Session URL for the specified Service/Client/Booking.</summary>
        /// <returns>The secure Stripe Checkout URL representing the Hosted Session</returns>
        Task<string> CreateCheckoutSessionAsync(Guid serviceId, Guid clientId, Guid bookingId, string successUrl, string cancelUrl);

        /// <summary>Verifies that a specific Checkout Session succeeded by contacting Stripe.</summary>
        /// <returns>The verified Payment tracking entity.</returns>
        Task<Domain.Entities.Payment> VerifyCheckoutSessionStatusAsync(string sessionId);

        /// <summary>Creates a Stripe PaymentIntent for the specified Service/Client and records it as Pending in the DB.</summary>
        /// <returns>A tuple containing the ClientSecret (for the frontend to authenticate) and the PaymentIntentId.</returns>
        Task<(string ClientSecret, string PaymentIntentId)> CreatePaymentIntentAsync(Guid serviceId, Guid clientId);

        /// <summary>Verifies that a specific PaymentIntent actually succeeded by contacting Stripe.</summary>
        /// <returns>The verified Payment tracking entity.</returns>
        Task<Domain.Entities.Payment> VerifyPaymentIntentStatusAsync(string paymentIntentId);

        /// <summary>Automatically issues a refund on Stripe and marks the DB record as Refunded.</summary>
        Task RefundPaymentAsync(string paymentIntentId);
        
        /// <summary>Links a successfully created Booking to its corresponding Payment record.</summary>
        Task LinkBookingToPaymentAsync(string paymentIntentId, Guid bookingId);
    }
}
