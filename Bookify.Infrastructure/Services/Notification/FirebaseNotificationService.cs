using Bookify.Application.Interfaces.Notification;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Services.Notification
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ILogger<FirebaseNotificationService> _logger;

        public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendNotificationToTopicAsync(Guid userId, string title, string body, string? redirectUrl = null)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    _logger.LogWarning("FirebaseApp is not initialized. Push notification will not be sent.");
                    return false;
                }

                var topic = $"user_{userId:N}";
                
                var dataDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    dataDict.Add("redirectUrl", redirectUrl);
                }

                var message = new Message()
                {
                    Topic = topic,
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = dataDict
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent message to topic '{Topic}'. Response: {Response}", topic, response);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to topic 'user_{UserId}'", userId);
                return false;
            }
        }
    }
}
