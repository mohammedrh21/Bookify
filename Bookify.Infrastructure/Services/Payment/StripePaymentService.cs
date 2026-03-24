using Bookify.Application.Common;
using Bookify.Application.Interfaces.Payment;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Services.Payment
{
    public class StripePaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public StripePaymentService(AppDbContext context, IOptions<StripeSettings> stripeSettings)
        {
            _context = context;
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(Guid serviceId, Guid clientId, Guid bookingId, string successUrl, string cancelUrl)
        {
            var service = await _context.Services.FindAsync(serviceId)
                ?? throw new Exception("Service not found.");

            long amountInCents = (long)(service.Price * 100);

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new System.Collections.Generic.List<string> { "card" },
                LineItems = new System.Collections.Generic.List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = amountInCents,
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = service.Name,
                                Description = service.Description ?? "Bookify Service Appointment"
                            }
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = clientId.ToString(),
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "BookingId", bookingId.ToString() },
                    { "ServiceId", serviceId.ToString() }
                }
            };

            var sessionService = new Stripe.Checkout.SessionService();
            var session = await sessionService.CreateAsync(options);

            var paymentRecord = new Domain.Entities.Payment
            {
                StripePaymentIntentId = session.Id,
                Amount = service.Price,
                Status = PaymentStatus.Pending,
                ServiceId = serviceId,
                ClientId = clientId,
                BookingId = bookingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            return session.Url;
        }

        public async Task<Domain.Entities.Payment> VerifyCheckoutSessionStatusAsync(string sessionId)
        {
            var paymentRecord = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == sessionId)
                ?? throw new Exception("Payment record not found locally.");

            if (paymentRecord.Status == PaymentStatus.Succeeded) return paymentRecord;

            var sessionService = new Stripe.Checkout.SessionService();
            var session = await sessionService.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                paymentRecord.Status = PaymentStatus.Succeeded;
            }
            else if (session.PaymentStatus == "unpaid" && session.Status == "expired")
            {
                paymentRecord.Status = PaymentStatus.Failed;
            }

            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return paymentRecord;
        }

        public async Task<(string ClientSecret, string PaymentIntentId)> CreatePaymentIntentAsync(Guid serviceId, Guid clientId)
        {
            var service = await _context.Services.FindAsync(serviceId)
                ?? throw new Exception("Service not found.");

            // Stripe requires Amount in cents (e.g., $10.00 = 1000)
            long amountInCents = (long)(service.Price * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "usd",
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "ServiceId", serviceId.ToString() },
                    { "ClientId", clientId.ToString() }
                }
            };

            var serviceStripe = new PaymentIntentService();
            var paymentIntent = await serviceStripe.CreateAsync(options);

            // Record Pending payment
            var paymentRecord = new Domain.Entities.Payment
            {
                StripePaymentIntentId = paymentIntent.Id,
                Amount = service.Price,
                Status = PaymentStatus.Pending,
                ServiceId = serviceId,
                ClientId = clientId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            return (paymentIntent.ClientSecret, paymentIntent.Id);
        }

        public async Task<Domain.Entities.Payment> VerifyPaymentIntentStatusAsync(string paymentIntentId)
        {
            var paymentRecord = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId)
                ?? throw new Exception("Payment record not found locally.");

            var serviceStripe = new PaymentIntentService();
            var paymentIntent = await serviceStripe.GetAsync(paymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                paymentRecord.Status = PaymentStatus.Succeeded;
            }
            else if (paymentIntent.Status == "requires_payment_method" || paymentIntent.Status == "canceled")
            {
                paymentRecord.Status = PaymentStatus.Failed;
            }

            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return paymentRecord;
        }

        public async Task RefundPaymentAsync(string stripeId)
        {
            var paymentRecord = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripeId)
                ?? throw new Exception("Payment record not found locally.");

            if (paymentRecord.Status != PaymentStatus.Succeeded) return;

            string paymentIntentIdToRefund = stripeId;

            if (stripeId.StartsWith("cs_")) // It's a Checkout session
            {
                var sessionService = new Stripe.Checkout.SessionService();
                var session = await sessionService.GetAsync(stripeId);
                paymentIntentIdToRefund = session.PaymentIntentId ?? throw new Exception("Checkout session does not have a mapped PaymentIntent.");
            }

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentIdToRefund,
                Reason = RefundReasons.RequestedByCustomer // "Race condition lost" mapping
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);

            if (refund.Status == "succeeded")
            {
                paymentRecord.Status = PaymentStatus.Refunded;
                paymentRecord.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Refund failed on Stripe: {refund.FailureReason}");
            }
        }

        public async Task LinkBookingToPaymentAsync(string paymentIntentId, Guid bookingId)
        {
            var paymentRecord = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId)
                ?? throw new Exception("Payment record not found locally.");

            paymentRecord.BookingId = bookingId;
            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
