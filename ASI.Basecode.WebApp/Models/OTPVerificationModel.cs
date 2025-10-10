namespace Acadus___Alliance_Project_2025.Models
{
    /// <summary>
    /// Model representing the four-digit OTP input pieces for verification.
    /// </summary>
    public class OTPVerificationModel
    {
        /// <summary>First OTP digit.</summary>
        public string Otp1 { get; set; } = string.Empty;
        /// <summary>Second OTP digit.</summary>
        public string Otp2 { get; set; } = string.Empty;
        /// <summary>Third OTP digit.</summary>
        public string Otp3 { get; set; } = string.Empty;
        /// <summary>Fourth OTP digit.</summary>
        public string Otp4 { get; set; } = string.Empty;
    }
}








