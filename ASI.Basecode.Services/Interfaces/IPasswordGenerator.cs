using System;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IPasswordGenerator
    {
        string GenerateSecurePassword(int length = 12);
        string GenerateTemporaryPassword();
        bool ValidatePasswordStrength(string password);
    }
}
