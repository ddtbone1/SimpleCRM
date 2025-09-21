using System.Text.RegularExpressions;

namespace SimpleCRM.Services
{
    public interface IPasswordService
    {
        bool ValidatePassword(string password, out List<string> errors);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordService : IPasswordService
    {
        public bool ValidatePassword(string password, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password is required");
                return false;
            }

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("Password must contain at least one number");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]"))
                errors.Add("Password must contain at least one special character");

            return errors.Count == 0;
        }

        public string HashPassword(string password)
        {
            // In production, use BCrypt or similar
            // For MVP, we'll use a simple hash (NOT SECURE - REPLACE IN PRODUCTION)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Check if it's a BCrypt hash (starts with $2a$, $2b$, $2x$, or $2y$)
            if (hashedPassword.StartsWith("$2a$") || hashedPassword.StartsWith("$2b$") || 
                hashedPassword.StartsWith("$2x$") || hashedPassword.StartsWith("$2y$"))
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            
            // Legacy plain text comparison (for existing seed data)
            return password == hashedPassword;
        }
    }
}
