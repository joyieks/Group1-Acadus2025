using ASI.Basecode.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ASI.Basecode.Services.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DigitChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        private const string AllChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

        public string GenerateSecurePassword(int length = 12)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

            using (var rng = RandomNumberGenerator.Create())
            {
                var password = new StringBuilder(length);
                
                // Ensure at least one character from each required category
                password.Append(GetRandomChar(LowercaseChars, rng));
                password.Append(GetRandomChar(UppercaseChars, rng));
                password.Append(GetRandomChar(DigitChars, rng));
                password.Append(GetRandomChar(SpecialChars, rng));

                // Fill the rest with random characters
                for (int i = 4; i < length; i++)
                {
                    password.Append(GetRandomChar(AllChars, rng));
                }

                // Shuffle the password to avoid predictable patterns
                return ShuffleString(password.ToString());
            }
        }

        public string GenerateTemporaryPassword()
        {
            // Generate a temporary password that's easy to type but still secure
            using (var rng = RandomNumberGenerator.Create())
            {
                var password = new StringBuilder(10);
                
                // Start with "Temp" for identification
                password.Append("Temp");
                
                // Add 4 random digits
                for (int i = 0; i < 4; i++)
                {
                    password.Append(GetRandomChar(DigitChars, rng));
                }
                
                // Add a special character
                password.Append("!");
                
                return password.ToString();
            }
        }

        public bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasLower = password.Any(c => char.IsLower(c));
            bool hasUpper = password.Any(c => char.IsUpper(c));
            bool hasDigit = password.Any(c => char.IsDigit(c));
            bool hasSpecial = password.Any(c => SpecialChars.Contains(c));

            return hasLower && hasUpper && hasDigit && hasSpecial;
        }

        private char GetRandomChar(string chars, RandomNumberGenerator rng)
        {
            var bytes = new byte[1];
            rng.GetBytes(bytes);
            return chars[bytes[0] % chars.Length];
        }

        private string ShuffleString(string input)
        {
            var array = input.ToCharArray();
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = array.Length - 1; i > 0; i--)
                {
                    var bytes = new byte[1];
                    rng.GetBytes(bytes);
                    int j = bytes[0] % (i + 1);
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }
            return new string(array);
        }
    }
}
