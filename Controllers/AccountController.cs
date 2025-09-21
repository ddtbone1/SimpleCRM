using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SimpleCRM.Models;
using SimpleCRM.Data;
using SimpleCRM.Services;

namespace SimpleCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEmailService _emailService;

        public AccountController(
            CrmDbContext context,
            IPasswordService passwordService,
            IAuditService auditService,
            IRateLimitService rateLimitService,
            IEmailService emailService)
        {
            _context = context;
            _passwordService = passwordService;
            _auditService = auditService;
            _rateLimitService = rateLimitService;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var rateLimitKey = $"login_{clientIp}_{username}";

            // Check rate limiting
            if (!_rateLimitService.IsRequestAllowed(rateLimitKey, 5, TimeSpan.FromMinutes(15)))
            {
                ViewBag.Error = "Too many login attempts. Please try again later.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user != null && _passwordService.VerifyPassword(password, user.Password))
            {
                // Check user status
                if (user.Status != "ACTIVE")
                {
                    var statusMessage = user.Status switch
                    {
                        "PENDING" => "Your account is pending approval. Please wait for admin approval.",
                        "DECLINED" => "Your account application was declined. Please contact support.",
                        "DEACTIVATED" => "Your account has been deactivated. Please contact support.",
                        _ => "Your account is not active. Please contact support."
                    };
                    
                    ViewBag.Error = statusMessage;
                    await _auditService.LogUserActionAsync(user.Id, "LOGIN_FAILED", $"Login failed - Status: {user.Status}", HttpContext);
                    return View();
                }

                // Reset rate limiting on successful login
                _rateLimitService.ResetAttempts(rateLimitKey);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Email", user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Log successful login
                await _auditService.LogUserActionAsync(user.Id, "LOGIN", "User logged in successfully", HttpContext);

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid username or password";
            
            // Log failed login attempt
            if (user != null)
            {
                await _auditService.LogUserActionAsync(user.Id, "LOGIN_FAILED", "Invalid password", HttpContext);
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                await _auditService.LogUserActionAsync(userId, "LOGOUT", "User logged out", HttpContext);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            // Username policy validation
            if (!IsValidUsername(user.Username))
            {
                ModelState.AddModelError("Username", "Username must be 8-20 characters long, contain only letters and numbers, and include at least one number");
            }

            // Validate required agent information
            if (string.IsNullOrEmpty(user.FirstName))
            {
                ModelState.AddModelError("FirstName", "First name is required");
            }
            
            if (string.IsNullOrEmpty(user.LastName))
            {
                ModelState.AddModelError("LastName", "Last name is required");
            }

            // Validate password
            if (!_passwordService.ValidatePassword(user.Password, out var passwordErrors))
            {
                foreach (var error in passwordErrors)
                {
                    ModelState.AddModelError("Password", error);
                }
            }

            // Check password confirmation
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            }

            if (ModelState.IsValid)
            {
                // Check if username or email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);
                
                if (existingUser != null)
                {
                    ViewBag.Error = "Username or email already exists";
                    return View(user);
                }

                // Hash password
                user.Password = _passwordService.HashPassword(user.Password);
                user.Role = "Agent"; // Role for agent registration
                user.Status = "PENDING"; // Requires admin approval
                user.EmailVerified = false;
                user.CreatedDate = DateTime.Now;
                user.AgentId = null; // Will be set when agent record is created upon approval

                // Generate email verification token
                user.EmailVerificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationSentAt = DateTime.Now;
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Log registration
                await _auditService.LogUserActionAsync(user.Id, "SIGNUP", "User registered and pending approval", HttpContext);

                // Send email verification (if email service is configured)
                var verificationUrl = Url.Action("VerifyEmail", "Account", new { token = user.EmailVerificationToken }, Request.Scheme);
                await _emailService.SendEmailVerificationAsync(user, verificationUrl!);

                ViewBag.Success = "Registration successful! Please check your email for verification instructions and wait for admin approval.";
                return View();
            }

            return View(user);
        }

        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 8 || username.Length > 20)
                return false;
            
            // Must contain only letters and numbers
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                return false;
                
            // Must contain at least one number
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"\d"))
                return false;
            
            return true;
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid verification token";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired verification token";
                return View();
            }

            // Check if token is expired (24 hours)
            if (user.EmailVerificationSentAt.HasValue && 
                DateTime.Now.Subtract(user.EmailVerificationSentAt.Value).TotalHours > 24)
            {
                ViewBag.Error = "Verification token has expired. Please request a new one.";
                return View();
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationSentAt = null;
            
            await _context.SaveChangesAsync();
            await _auditService.LogUserActionAsync(user.Id, "EMAIL_VERIFIED", "User verified email address", HttpContext);

            ViewBag.Success = "Email verified successfully! Please wait for admin approval.";
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email is required";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // Generate reset token
                user.PasswordResetToken = Guid.NewGuid().ToString();
                user.PasswordResetTokenExpiry = DateTime.Now.AddHours(1);
                
                await _context.SaveChangesAsync();

                // Send reset email
                var resetUrl = Url.Action("ResetPassword", "Account", new { token = user.PasswordResetToken }, Request.Scheme);
                await _emailService.SendPasswordResetAsync(user, resetUrl!);

                await _auditService.LogUserActionAsync(user.Id, "PASSWORD_RESET_REQUESTED", "User requested password reset", HttpContext);
            }

            // Always show success message to prevent email enumeration
            ViewBag.Success = "If an account with that email exists, we've sent password reset instructions.";
            return View();
        }

        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid reset token";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token && 
                                        u.PasswordResetTokenExpiry > DateTime.Now);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset token";
                return View();
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            ViewBag.Token = token;

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid reset token";
                return View();
            }

            // Validate password
            if (!_passwordService.ValidatePassword(newPassword, out var passwordErrors))
            {
                foreach (var error in passwordErrors)
                {
                    ModelState.AddModelError("NewPassword", error);
                }
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token && 
                                        u.PasswordResetTokenExpiry > DateTime.Now);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset token";
                return View();
            }

            // Update password and clear reset token
            user.Password = _passwordService.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            
            await _context.SaveChangesAsync();
            await _auditService.LogUserActionAsync(user.Id, "PASSWORD_RESET", "User reset password", HttpContext);

            ViewBag.Success = "Password reset successfully! You can now log in with your new password.";
            return View();
        }
    }
}