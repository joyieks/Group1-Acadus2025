using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendStudentWelcomeEmailAsync(string email, string firstName, string lastName, string temporaryPassword)
        {
            try
            {
                var subject = "Welcome to Acadus - Your Student Account is Ready!";
                var body = CreateWelcomeEmailBody(firstName, lastName, temporaryPassword, email);
                
                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetLink)
        {
            try
            {
                var subject = "Password Reset Request - Acadus";
                var body = CreatePasswordResetEmailBody(firstName, resetLink);
                
                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendStudentCredentialsEmailAsync(string email, string firstName, string lastName, string temporaryPassword)
        {
            try
            {
                var subject = "Your Student Account Credentials - Acadus";
                var body = CreateCredentialsEmailBody(firstName, lastName, temporaryPassword);
                
                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending credentials email to {Email}", email);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // For now, we'll use a simple SMTP configuration
                // In production, you'd want to use a service like SendGrid, AWS SES, etc.
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@acadus.edu";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration is incomplete. Email not sent to {Email}", toEmail);
                    return false;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(fromEmail, "Acadus Student Management System");
                        message.To.Add(toEmail);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        private string CreateWelcomeEmailBody(string firstName, string lastName, string temporaryPassword, string email)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #059669; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .credentials {{ background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .button {{ background: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Acadus!</h1>
            <p>Your Student Account is Ready</p>
        </div>
        <div class='content'>
            <h2>Hello {firstName} {lastName}!</h2>
            
            <p>Welcome to the Acadus Student Management System! Your student account has been successfully created by your administrator.</p>
            
            <div class='credentials'>
                <h3>Your Login Credentials:</h3>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Temporary Password:</strong> <code style='background: #f0f0f0; padding: 2px 4px; border-radius: 3px;'>{temporaryPassword}</code></p>
            </div>
            
            <div class='warning'>
                <h4>⚠️ Important Security Notice:</h4>
                <ul>
                    <li>This is a <strong>temporary password</strong> that you must change on your first login</li>
                    <li>Please log in immediately and update your password</li>
                    <li>Keep your login credentials secure and do not share them</li>
                </ul>
            </div>
            
            <p>To get started, please visit our student portal and log in with the credentials provided above.</p>
            
            <a href='https://localhost:63125/Auth/Login' class='button'>Login to Student Portal</a>
            
            <p>If you have any questions or need assistance, please contact your administrator or the IT support team.</p>
            
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
            <p style='font-size: 12px; color: #666;'>
                This is an automated message from the Acadus Student Management System.<br>
                Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreatePasswordResetEmailBody(string firstName, string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ background: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName}!</h2>
            
            <p>We received a request to reset your password for your Acadus student account.</p>
            
            <p>Click the button below to reset your password:</p>
            
            <a href='{resetLink}' class='button'>Reset My Password</a>
            
            <p>If you didn't request this password reset, please ignore this email or contact support if you have concerns.</p>
            
            <p><strong>Note:</strong> This link will expire in 24 hours for security reasons.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateCredentialsEmailBody(string firstName, string lastName, string temporaryPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #059669; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .credentials {{ background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Student Account Credentials</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName} {lastName}!</h2>
            
            <p>Your student account credentials have been updated. Here are your new login details:</p>
            
            <div class='credentials'>
                <h3>Login Credentials:</h3>
                <p><strong>Temporary Password:</strong> <code style='background: #f0f0f0; padding: 2px 4px; border-radius: 3px;'>{temporaryPassword}</code></p>
            </div>
            
            <p>Please log in and change your password immediately for security reasons.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
