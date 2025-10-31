using KILVRA.Area.Admin.AdminServices;
using Microsoft.AspNetCore.Mvc;
using KILVRA.Models;
using Microsoft.EntityFrameworkCore;

namespace KILVRA.Area.Admin.Controller
{
    [Area("Admin")]
    public class DashboardController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly GoogleAnalyticsService _gaService;
        private readonly OnlineClothesShopContext _context;

        public DashboardController(GoogleAnalyticsService gaService, OnlineClothesShopContext context)
        {
            _gaService = gaService;
            _context = context;
        }
        [AdminAuthorize]
        public async Task<IActionResult> Index()
        {
            var analytics = await _gaService.GetAnalyticsSummaryAsync();
            // Keep analytics numbers in separate property
            ViewBag.UserCount = analytics.Users;
            ViewBag.Sessions = analytics.Sessions;
            ViewBag.Revenue = analytics.Revenue;

            // Provide a simple 7-day placeholder chart data (so the view's JS has something to render)
            var chartData = Enumerable.Range(0, 7)
                .Select(i => new
                {
                    Day = DateTime.Now.AddDays(-(6 - i)).ToString("MM-dd"),
                    Users = analytics.Users / 7, // rough distribution
                    Revenue = analytics.Revenue / 7.0
                })
                .ToList();
            ViewBag.ChartData = chartData;

            // Provide actual user list for the UsersList partial
            var users = _context.Users
                .Select(u => new { u.UserId, u.FullName, u.Email })
                .ToList();
            ViewBag.Users = users;

            // Provide recent orders for the OrderHistory partial (most recent10)
            var recentOrders = _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();
            ViewBag.Orders = recentOrders;

            return View();
        }

        public IActionResult Login()
        {
            return RedirectToAction("Login","AdminAccount");
        }
        public IActionResult Register()
        {
            return RedirectToAction("Register", "AdminAccount");
        }
        public IActionResult Reset()
        {
            return View();
        }
        public IActionResult UsersManage()
        {
            return RedirectToAction("Index","Users");
        }
        public IActionResult ProductsManage()
        {
            return RedirectToAction("Index", "Products");
        }
        public IActionResult CouponsManage()
        {
            return RedirectToAction("Index", "Coupons");
        }

        // GET: Admin/Dashboard/OrderHistory
        // If userId is not provided, show a list of users to choose from.
        // If userId is provided, show that user's orders with details.
        [AdminAuthorize]
        public IActionResult OrderHistory(int? userId)
        {
            if (userId == null)
            {
                // Return a simple users list view (create view Area/Admin/Views/Dashboard/UsersList.cshtml)
                var users = _context.Users
                    .Select(u => new { u.UserId, u.FullName, u.Email })
                    .ToList();
                // Passing as ViewBag for a quick implementation
                ViewBag.Users = users;
                return View("UsersList");
            }

            // Load orders for a specific user including order details and products and payment
            var orders = _context.Orders
                .Where(o => o.UserId == userId.Value)
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.User = _context.Users.Find(userId.Value);
            return View("OrderHistory", orders);
        }
    }
}
