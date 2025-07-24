using System;
using System.Security.Cryptography;
using System.Text;

namespace PeerTutoringSystem.Application.Helpers
{
    public static class PayosSignatureHelper
    {
        public static string GenerateSignature(string data, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}