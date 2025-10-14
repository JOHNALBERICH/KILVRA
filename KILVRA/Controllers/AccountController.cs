using Microsoft.AspNetCore.Mvc;
using KILVRA.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using System.Security.Claims;
namespace KILVRA.Controllers
{
    public class AccountController : Controller
    {
        private readonly OnlineClothesShopContext _context;
        public AccountController(OnlineClothesShopContext context)
        {
            _context = context;
        }
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user, string password)
        {
           ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                return View(user);
            }
            
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(user);
            }
            if(_context.Users.Any(u => u.FullName == user.FullName))
            {
                ModelState.AddModelError("", "User name already exist");
                return View(user);
            }
            if (ModelState.IsValid)
            {
                user.Role = "User";
                user.PasswordHash = HashPassword(password);
                _context.Users.Add(user);
                
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }
            TempData["ErrorMessage"] = "Registration failed. Please check your input.";
            return View(user);


        }
        // ========================= LOGIN =========================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || user.PasswordHash != HashPassword(password))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Phone",user.Phone.ToString()),
                new Claim("Address",user.Address.ToString())

            };
            if (user != null)
            {
                TempData["ErrorMessage"] = "Invalid email or password.";

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                TempData["SuccessMessage"] = "Login successful! Welcome back 👋";
                
            }
            return RedirectToAction("Index", "Home");


        }

        // ========================= LOGOUT =========================
       public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Index", "Home");
        }

        // ========================= SOCIAL LOGIN =========================
        /*        public IActionResult GoogleLogin()
               {
                   var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
                   return Challenge(properties, GoogleDefaults.AuthenticationScheme);
               }

               public async Task<IActionResult> GoogleResponse()
               {
                   var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                   var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                   var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

                   var user = _context.Users.FirstOrDefault(u => u.Email == email);
                   if (user == null)
                   {
                       user = new User
                       {
                           FullName = name,
                           Email = email,
                           Provider = "Google"
                       };
                       _context.Users.Add(user);
                       _context.SaveChanges();
                   }

                   return RedirectToAction("Index", "Home");
               }
       */
        // ========================= UTIL =========================
       
    }
}

