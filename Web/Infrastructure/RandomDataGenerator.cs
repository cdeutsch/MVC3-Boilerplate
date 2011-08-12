using System;
using System.Text;

namespace Web.Infrastructure
{
    /// <summary>
    /// Most of this comes from DotNetOpenAuth.
    /// </summary>
    public static class RandomDataGenerator
    {
        /// <summary>
        /// The uppercase alphabet.
        /// </summary>
        public const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// The lowercase alphabet.
        /// </summary>
        public const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The set of base 10 digits.
        /// </summary>
        public const string Digits = "0123456789";

        /// <summary>
        /// The set of digits, and alphabetic letters (upper and lowercase) that are clearly
        /// visually distinguishable.
        /// </summary>
        public const string AlphaNumericNoLookAlikes = "23456789abcdefghjkmnpqrstwxyzABCDEFGHJKMNPQRSTWXYZ";

        private static readonly Random NonCryptoRandomDataGenerator = new Random();

        /// <summary>
        /// Returns a random string of "AlphaNumeric No LookAlike" characters with the length defaulted to 10.
        /// </summary>
        /// <returns></returns>
        public static string GetRandomString()
        {
            return GetRandomString(10);
        }

        /// <summary>
        /// Returns a random string of "AlphaNumeric No LookAlike" characters with the length specified.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static string GetRandomString(int Length)
        {
            return GetRandomString(Length, AlphaNumericNoLookAlikes);
        }

        /// <summary>
        /// Returns a random string of characters using the Allowable characters specified with the length specified.
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="AllowableCharacters"></param>
        /// <returns></returns>
        public static string GetRandomString(int Length, string AllowableCharacters)
        {
            char[] randomString = new char[Length];
            for (int i = 0; i < Length; i++)
            {
                randomString[i] = AllowableCharacters[NonCryptoRandomDataGenerator.Next(AllowableCharacters.Length)];
            }

            return new string(randomString);
        }
    }
}
