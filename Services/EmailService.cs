using SimpleCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace SimpleCRM.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(User user, string verificationUrl);
        Task SendPasswordResetAsync(User user, string resetUrl);
        Task SendUserApprovalAsync(User user, bool isApproved, string? comments = null);
        Task SendWelcomeEmailAsync(User user);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(User user, string verificationUrl)
        {
            // For MVP, we'll just log the email content
            // In production, integrate with SMTP service or email provider
            var subject = "Verify Your Email Address - SimpleCRM";
            var body = $@"
                <h2>Welcome to SimpleCRM!</h2>
                <p>Hello {user.Username},</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email Address</a></p>
                <p>If you didn't create this account, please ignore this email.</p>
                <p>Best regards,<br>SimpleCRM Team</p>
            ";

            _logger.LogInformation("Email Verification sent to {Email}: {Subject}", user.Email, subject);
            _logger.LogInformation("Email Body: {Body}", body);

            // TODO: Implement actual email sending
            await Task.CompletedTask;
        }

        public async Task SendPasswordResetAsync(User user, string resetUrl)
        {
            var subject = "Password Reset Request - SimpleCRM";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.Username},</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetUrl}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>Best regards,<br>SimpleCRM Team</p>
            ";

            _logger.LogInformation("Password Reset sent to {Email}: {Subject}", user.Email, subject);
            _logger.LogInformation("Email Body: {Body}", body);

            await Task.CompletedTask;
        }

        public async Task SendUserApprovalAsync(User user, bool isApproved, string? comments = null)
        {
            var subject = isApproved ? "Account Approved - SimpleCRM" : "Account Application Update - SimpleCRM";
            var body = isApproved ? $@"
                <h2>Account Approved!</h2>
                <p>Hello {user.Username},</p>
                <p>Your SimpleCRM account has been approved. You can now log in and start using the system.</p>
                {(string.IsNullOrEmpty(comments) ? "" : $"<p>Admin Comments: {comments}</p>")}
                <p>Best regards,<br>SimpleCRM Team</p>
            " : $@"
                <h2>Account Application Update</h2>
                <p>Hello {user.Username},</p>
                <p>Thank you for your interest in SimpleCRM. Unfortunately, your account application was not approved at this time.</p>
                {(string.IsNullOrEmpty(comments) ? "" : $"<p>Reason: {comments}</p>")}
                <p>If you have questions, please contact our support team.</p>
                <p>Best regards,<br>SimpleCRM Team</p>
            ";

            _logger.LogInformation("User Approval Email sent to {Email}: {Subject}", user.Email, subject);
            _logger.LogInformation("Email Body: {Body}", body);

            await Task.CompletedTask;
        }

        public async Task SendWelcomeEmailAsync(User user)
        {
            var subject = "Welcome to SimpleCRM!";
            var body = $@"
                <h2>Welcome to SimpleCRM!</h2>
                <p>Hello {user.Username},</p>
                <p>Your account is now active and ready to use.</p>
                <p>You can log in and start managing your timesheets and customer data.</p>
                <p>If you need help getting started, don't hesitate to contact our support team.</p>
                <p>Best regards,<br>SimpleCRM Team</p>
            ";

            _logger.LogInformation("Welcome Email sent to {Email}: {Subject}", user.Email, subject);
            _logger.LogInformation("Email Body: {Body}", body);

            await Task.CompletedTask;
        }
    }
}
