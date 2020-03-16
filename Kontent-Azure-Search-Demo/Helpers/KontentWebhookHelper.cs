using Kontent_Azure_Search_Demo.KontentWebhookModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kontent_Azure_Search_Demo.Helpers
{
    public class KontentWebhookHelper
    {
        private readonly string webhookSecret;

        public KontentWebhookHelper(IConfiguration configuration)
        {
            webhookSecret = configuration["KontentWebhookSecret"];
        }

        public ValidationResult ValidateAndProcessWebhook(string signature, string body)
        {
            var hash = GenerateHash(body, webhookSecret);
            if (signature != hash)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid signature"
                };
            }

            // Manually deserialize request body because we needed to use the raw value previously to validate the request
            var webhook = JsonConvert.DeserializeObject<Webhook>(body);

            var isContentItemVariantEvent = webhook.Message.Type == "content_item_variant";
            if (!isContentItemVariantEvent)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Not content item variant event"
                };
            }

            var isPublishevent = webhook.Message.Operation == "publish";
            var isUnpublishEvent = webhook.Message.Operation == "unpublish";

            if (!isPublishevent && !isUnpublishEvent)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Not publish/unpublish event"
                };
            }

            var articles = webhook.Data.Items.Where(i => i.Type == "article" && i.Language == "en-US");
            if (articles.Count() == 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No 'en-US' articles effected by this event"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Articles = articles
            };
        }

        private string GenerateHash(string message, string secret)
        {
            secret ??= "";
            UTF8Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            byte[] keyBytes = SafeUTF8.GetBytes(secret);
            byte[] messageBytes = SafeUTF8.GetBytes(message);

            using HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }

    public class ValidationResult
    {
        public IEnumerable<Item> Articles { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsValid { get; set; }
        public string Operation { get; set; }
    }
}
