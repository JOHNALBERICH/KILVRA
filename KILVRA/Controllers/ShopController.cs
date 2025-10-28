using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KILVRA.Models;
using KILVRA.Services;

namespace KILVRA.Controllers
{
    public class ShopController : Controller
    {
        private readonly OnlineClothesShopContext _context;
       

        public ShopController(OnlineClothesShopContext context)
        {
            _context = context;
            
        }
        [HttpGet("Shop/ByCategory/{categoryId}")]
        public IActionResult ByCategory(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);

            if (category == null)
                return NotFound();

            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Where(p => p.CategoryId == categoryId)
                                   .ToList();

            if (products.Count == 0)
            {
                ViewBag.Message = $"No products found in category '{category.Name}'";
            }

            ViewBag.CategoryName = category.Name;
            return View("Index", products);
        }
        
         // Show all products
        [HttpGet]
        public IActionResult Index(string searchQuery, string priceRange, string size)
        {
            var products = _context.Products
         .Include(p => p.Category)
         .Include(p => p.Shop)
         .AsQueryable();


            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.Contains(searchQuery));
                ViewBag.SearchQuery = searchQuery;
            }
            else
            {
                ViewBag.SearchQuery = "";
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var range = priceRange.Split('-');
                if (range.Length == 2 && decimal.TryParse(range[0], out var min) && decimal.TryParse(range[1], out var max))
                {
                    products = products.Where(p => p.Price >= min && p.Price <= max);
                }
                ViewBag.SelectedPriceRange = priceRange;
            }
            else
            {
                ViewBag.SelectedPriceRange = "";
            }

            // Fix: Only filter by size if not null or empty AND not All Size (empty string)
            if (!string.IsNullOrEmpty(size))
            {
                products = products.Where(p => p.Size != null && p.Size.Contains(size));
                ViewBag.SelectedSize = size;
            }
            else
            {
                ViewBag.SelectedSize = "";
            }

            return View(products.Include(p => p.Shop).ToList());
        }

        // GET: Shop/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Shop/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ShopId"] = new SelectList(_context.Shops, "ShopId", "ShopName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = "/images/" + uniqueFileName;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShopId"] = new SelectList(_context.Shops, "ShopId", "ShopName", product.ShopId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(product);
        }
        // POST: Shop/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
 //       [ValidateAntiForgeryToken]
/*        public async Task<IActionResult> Create([Bind("ProductId,ProductId,Name,Description,Price,Quantity,Category,ImageUrl,CreatedAt")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Shops, "ProductId", "ShopName", product.ProductId);
            return View(product);
        }*/

        // GET: Shop/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ShopId"] = new SelectList(_context.Shops, "ShopId", "ShopName", product.ShopId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(product);
        }

        // POST: Shop/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        product.ImageUrl = "/images/" + uniqueFileName;
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShopId"] = new SelectList(_context.Shops, "ShopId", "ShopName", product.ShopId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(product);
        }
        
        // GET: Shop/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Shop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        public async Task<IActionResult> NewArrivals()
        {
            // Get products added in the last 7 days (or latest 6 products)
            var newProducts = _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToList();

            return View(newProducts);
        }
        public IActionResult Shop(string category = null)
        {
            // Get all products
            var products = _context.Products.AsQueryable();

            // Filter by category if one is selected
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name == category);
            }

            // Get distinct categories for the sidebar
            var categories = _context.Products
                .Select(p => p.Category)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            // Pass both to the view
            ViewBag.Categories = categories;
            return View(products.ToList());
        }
       

    }
}
