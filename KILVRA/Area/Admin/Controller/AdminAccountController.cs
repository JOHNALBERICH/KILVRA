using KILVRA.Area.Admin.AdminServices;
using KILVRA.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace KILVRA.Area.Admin
{
  [Area("Admin")]
  public class AdminAccountController : Microsoft.AspNetCore.Mvc.Controller
  {
    public readonly OnlineClothesShopContext _context;

    // Add constructor to receive DbContext via DI
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
      var admin = _context.Admins.Include(a => a.User).FirstOrDefault(a => a.User.Email == email && a.User.PasswordHash == hashedPassword);
      if (admin == null)
      {
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View();
      }
      HttpContext.Session.SetInt32("AdminId", admin.AdminId);

      // Redirect to Dashboard index in Admin area (controller name: Dashboard)
      return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public IActionResult Register(string email, string password, string position, string fullName, string address)
    {
      // ensure non-nullable columns are set
      if (string.IsNullOrWhiteSpace(fullName)) fullName = "Admin";
      if (string.IsNullOrWhiteSpace(address)) address = ""; // empty string allowed, not NULL

      if (_context.Users.Any(u => u.Email == email))
      {
        ViewBag.Error = "Email already exists.";
        return View();
      }

      var newUser = new User
      {
        FullName = fullName,
        Email = email,
        PasswordHash = HashPassword(password),
        Address = address
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

    // GET: Admin/Account/ResetPassword
        [HttpGet]
    public IActionResult ResetPassword()
    {
      return View();
    }

    // POST: Admin/Account/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(string email, string newPassword)
    {
      if (string.IsNullOrEmpty(email))
      {
        ModelState.AddModelError("Email", "Email is required.");
      }
      if (string.IsNullOrEmpty(newPassword))
      {
        ModelState.AddModelError("NewPassword", "New password is required.");
      }
      if (!ModelState.IsValid)
      {
        return View();
      }

      // Find admin by email (including related User)
      var admin = _context.Admins.Include(a => a.User).FirstOrDefault(a => a.User.Email == email);
      if (admin == null)
      {
        // For security, do not reveal whether the email exists
        ModelState.AddModelError(string.Empty, "If the email exists, a reset has been performed.");
        return View();
      }

      // Update the user's password hash
      admin.User.PasswordHash = HashPassword(newPassword);
      _context.SaveChanges();

      // Optionally clear any admin sessions or set flags
      ViewBag.Message = "Password has been reset successfully.";

      return RedirectToAction("Login");
    }
  }
}
