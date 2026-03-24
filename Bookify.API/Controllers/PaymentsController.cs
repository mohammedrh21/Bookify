using Bookify.Application.DTO.Payment;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Bookify.Application.Common;
using Microsoft.Extensions.Options;
using Stripe;
using System.IO;
using System;

namespace Bookify.API.Controllers
{
    [Route("api/payments")]
    [Authorize(Policy = "ClientOnly")] // Only clients should pay and book
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly StripeSettings _stripeSettings;

        public PaymentsController(
            IPaymentService paymentService, 
            IBookingService bookingService, 
            IOptions<StripeSettings> stripeSettings)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _stripeSettings = stripeSettings.Value;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateBookingRequest request)
        {
            // 1. Lock the time slot and create the Pending booking in the DB
            var bookingResult = await _bookingService.CreateAsync(request);
            if (!bookingResult.Success)
                return BadRequest(bookingResult);

            var bookingId = bookingResult.Data;
            
            // Build dynamically without hardcoded host
            // (Assumptions: running directly behind reverse proxy or natively)
            // Ideally we get the absolute base URL of the client, but since we serve API & Client from same host (locally/dev), Request.Host works.
            // Wait: A better approach is fetching it from AppSettings, but Request.Scheme works for now. 
            // BUT: If the client runs on a different port (e.g. blazor 7035, API 7031) Request.Host might be the API's port.
            // Let's rely on Request.Headers["Origin"] if available.
            var origin = Request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(origin)) 
                origin = $"{Request.Scheme}://{Request.Host}"; // Fallback

            var successUrl = $"{origin}/payment-success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{origin}/book/{request.ServiceId}";

            // 2. Generate the Stripe Checkout Session URL
            var url = await _paymentService.CreateCheckoutSessionAsync(
                request.ServiceId, 
                CurrentUserGuid, 
                bookingId, 
                successUrl, 
                cancelUrl);

            return Ok(Bookify.Application.Common.ServiceResponse<object>.Ok(new { CheckoutUrl = url }));
        }

        [HttpPost("confirm-checkout")]
        public async Task<IActionResult> ConfirmCheckoutSession([FromBody] ConfirmCheckoutRequest request)
        {
            var result = await _bookingService.ConfirmCheckoutAsync(request.SessionId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret
                );

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        {
                            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                            if (session != null)
                            {
                                await _bookingService.ConfirmCheckoutAsync(session.Id);
                            }
                            break;
                        }
                    case "checkout.session.expired":
                        {
                            // A session expired without payment. Here we theoretically release the pending booking.
                            // However, we just return Ok() for now as Pending hooks naturally drop.
                            break;
                        }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }

    public class ConfirmCheckoutRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
