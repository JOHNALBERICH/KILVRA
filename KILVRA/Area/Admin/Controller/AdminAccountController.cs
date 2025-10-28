using KILVRA.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace KILVRA.Area.Admin
{
  [Area("Admin")]
  public class AdminAccountController : Microsoft.AspNetCore.Mvc.Controller
  {
    public readonly OnlineClothesShopContext _context;
    public AdminAccountController(OnlineClothesShopContext context)
    {
      _context = context;
    }
    private string HashPassword(string password)
    {
      using var sha256 = SHA256.Create();
      var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
      return Convert.ToBase64String(bytes);
    }
    [HttpGet]
    public IActionResult Login() => View();
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
      if (string.IsNullOrEmpty(email))
      {
        ModelState.AddModelError("Email", "Email is required.");
      }
      if (string.IsNullOrEmpty(password))
      {
        ModelState.AddModelError("Password", "Password is required.");
      }
      if (!ModelState.IsValid)
      {
        return View();
      }
      var hashedPassword = HashPassword(password);
      var admin = _context.Admins.FirstOrDefault(a => a.User.Email == email && a.User.PasswordHash == hashedPassword);
      if (admin == null)
      {
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View();
      }
      HttpContext.Session.SetInt32("AdminId", admin.AdminId);

      return RedirectToAction("Dashboard", "AdminHome");
    }
    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public IActionResult Register(string email, string password, string position)
    {
      if (_context.Users.Any(u => u.Email == email))
      {
        ViewBag.Error = "Email already exists.";
        return View();
      }

      var newUser = new User
      {
        Email = email,
        PasswordHash = HashPassword(password)
      };
      _context.Users.Add(newUser);
      _context.SaveChanges();

      var newAdmin = new KILVRA.Models.Admin // Fully qualify to avoid namespace/type conflict
      {
        UserId = newUser.UserId,
        Position = position
      };
      _context.Admins.Add(newAdmin);
      _context.SaveChanges();

      return RedirectToAction("Login");
    }
  
   // Logout
        [HttpGet]
    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      Response.Cookies.Delete("AdminEmail");
      return RedirectToAction("Login");
    }
  }
}
