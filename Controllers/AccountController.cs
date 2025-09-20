using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using SimpleCRM.Models;

namespace SimpleCRM.Controllers
{
    public class AccountController : Controller
    {
        public static List<User> users = new List<User>();
        private static int nextUserId = 1;

        static AccountController()
        {
            // Add default admin and user
            users.Add(new User 
            { 
                Id = nextUserId++, 
                Username = "admin", 
                Email = "admin@crm.com", 
                Password = "admin123", 
                Role = "Admin" 
            });
            users.Add(new User 
            { 
                Id = nextUserId++, 
                Username = "user", 
                Email = "user@crm.com", 
                Password = "user123", 
                Role = "User" 
            });
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
            
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = nextUserId++;
                user.Role = "User"; // Default role
                users.Add(user);
                return RedirectToAction("Login");
            }
            return View(user);
        }
    }
}