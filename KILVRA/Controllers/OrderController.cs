using KILVRA.Models;
using KILVRA.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace KILVRA.Controllers
{
    public class OrderController : Controller
    {
        private readonly OnlineClothesShopContext _context;
        private readonly CartService _cartService;

        public OrderController(OnlineClothesShopContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }
        public IActionResult Index()
        {
            var cart = _cartService.GetOrderDetails();
            ViewBag.Subtotal = _cartService.GetSubtotal();
            ViewBag.Shipping = _cartService.GetShippingFee();
            ViewBag.Total = _cartService.GetTotal();
            return View(cart); // Pass the cart (List<Product>) as the model
        }

        public IActionResult CheckOut()
        {
            var cart = _cartService.GetOrderDetails();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var vm = new CheckoutViewModel
            {
                CartItems = cart,
                Subtotal = _cartService.GetSubtotal(),
                Shipping = _cartService.GetShippingFee(),
                Total = _cartService.GetTotal()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return with same cart details if validation fails
                model.CartItems = _cartService.GetOrderDetails();
                model.Subtotal = _cartService.GetSubtotal();
                model.Shipping = _cartService.GetShippingFee();
                model.Total = _cartService.GetTotal();
                return View(model);
            }

            // Create Order
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.Total,
                Status = "Pending",
                PaymentMethod = model.PaymentMethod
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Add order details
            foreach (var item in _cartService.GetOrderDetails())
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = (item.Quantity ?? 0),
                    Size = item.Size
                };
                _context.OrderDetails.Add(detail);
            }
            _context.SaveChanges();

            _cartService.ClearCart();
            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Success");
        }
        [HttpPost]
        public IActionResult CheckoutConfirm()
        {
            var cart = _cartService.GetOrderDetails();
            if (cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => (i.Quantity ?? 0) * i.Price),
                Status = "Pending"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity ?? 0,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(detail);
            }

            _context.SaveChanges();

            _cartService.ClearCart();

            return RedirectToAction("Success", new { id = order.OrderId });
        }
        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Where(o => o.OrderId == id)
                .FirstOrDefault();

            return View(order);
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, string size)
        {
            _cartService.RemoveFromCart(productId, size);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1, string size = null)
        {


            // Size check
            if (string.IsNullOrEmpty(size))
            {
                TempData["ErrorMessage"] = "Please select a size before adding to cart.";
                return RedirectToAction("Index", "Shop"); // go back to shop, don't add yet
            }

            // Add to cart
            if (quantity <= 0) quantity = 1;
            _cartService.AddToCart(id, quantity, size);
            TempData["SuccessMessage"] = "Added to cart successfully!";

            return RedirectToAction("Index", "Order"); // ✅ this is your cart page
        }
        
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }
        

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailId == id);
        }
    }
}
