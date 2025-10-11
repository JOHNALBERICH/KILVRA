using KILVRA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;

namespace KILVRA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly OnlineClothesShopContext _Context; 

        public HomeController(ILogger<HomeController> logger, OnlineClothesShopContext context)
        {
            _logger = logger;
            _Context = context;
        }
        
        public IActionResult Index()
        {
            // Load latest (just arrived) and trendy products
            var products = _Context.Products
                .OrderByDescending(p => p.CreatedAt) // assuming you have a CreatedDate column
                .Take(8) // show only latest 8 items
                .ToList();

            return View(products);
        }
        public IActionResult Shop()
        {
            return RedirectToAction("Index", "Shop");
        }
        
        
            [HttpGet("/Home/Details/{id:int}")]
        public IActionResult Details(int?id)

        {
            var product = _Context.Products.FirstOrDefault(p => p.ProductId == id);
            if(id==null)
            {
                return NotFound();
            }
            return View("~/Views/Shop/Details.cshtml", product);
        }
        public IActionResult ShopLocation()
        {
            return RedirectToAction("Create", "ShopsLocation");
        }
        

        
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
