using System;
using System.Linq;

namespace MyShopAPI.Services
{
    /// <summary>
    /// Helper service for generating random license activation keys.
    /// </summary>
    public static class LicenseKeyGenerator
    {
        private const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int KeyLength = 6;
        private static readonly Random Random = new Random();

        /// <summary>
        /// Generates a random 6-character license key using uppercase letters and digits.
        /// Example output: "A3X9K2", "ZY4B1C"
        /// </summary>
        /// <returns>A 6-character random string.</returns>
        public static string Generate()
        {
            lock (Random) // Thread-safe random generation
            {
                return new string(Enumerable.Range(0, KeyLength)
                    .Select(_ => ValidChars[Random.Next(ValidChars.Length)])
                    .ToArray());
            }
        }
    }
}
