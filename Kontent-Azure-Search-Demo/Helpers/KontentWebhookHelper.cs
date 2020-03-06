using System;
using System.Security.Cryptography;
using System.Text;

namespace Kontent_Azure_Search_Demo.Helpers
{
    public class KontentWebhookHelper
    {
        public static bool ValidateWebhook(string signature, string body, string webhookSecret)
        {
            var hash = GenerateHash(body, webhookSecret);
            return signature == hash;
        }

        private static string GenerateHash(string message, string secret)
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
}
