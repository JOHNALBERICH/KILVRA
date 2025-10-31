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
            // If coupon applied, show discount and adjusted total
            var appliedCode = HttpContext.Session.GetString("AppliedCouponCode");
            if (!string.IsNullOrEmpty(appliedCode))
            {
                if (decimal.TryParse(HttpContext.Session.GetString("AppliedCouponPercent"), out var pct))
                {
                    var total = _cartService.GetTotal();
                    var discount = total * (pct /100m);
                    ViewBag.Discount = discount;
                    ViewBag.Total = total - discount;
                    ViewBag.AppliedCoupon = appliedCode;
                }
            }
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

            // Calculate totals and apply coupon stored in session (if any)
            decimal subtotal = _cartService.GetSubtotal();
            decimal shipping = _cartService.GetShippingFee();
            decimal total = _cartService.GetTotal();
            decimal discountAmount =0m;
            var appliedCode = HttpContext.Session.GetString("AppliedCouponCode");
            if (!string.IsNullOrEmpty(appliedCode) && decimal.TryParse(HttpContext.Session.GetString("AppliedCouponPercent"), out var pct))
            {
                discountAmount = total * (pct /100m);
                total = total - discountAmount;
            }

            var vm = new CheckoutViewModel
            {
                CartItems = cart,
                Subtotal = subtotal,
                Shipping = shipping,
                Total = total
            };

            ViewBag.AppliedCoupon = appliedCode;
            ViewBag.Discount = discountAmount;

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

            // Ensure user is signed in and attach UserId
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Recalculate totals server-side and apply coupon from session
            decimal subtotal = _cartService.GetSubtotal();
            decimal shipping = _cartService.GetShippingFee();
            decimal total = _cartService.GetTotal();
            decimal discountAmount =0m;
            var appliedCode = HttpContext.Session.GetString("AppliedCouponCode");
            if (!string.IsNullOrEmpty(appliedCode) && decimal.TryParse(HttpContext.Session.GetString("AppliedCouponPercent"), out var pct))
            {
                discountAmount = total * (pct /100m);
                total = total - discountAmount;
            }

            // Create Order
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Pending",
                Payment = new Payment
                {
                    PaymentMethod = model.PaymentMethod,
                    AmountPaid = total,
                    PaymentDate = DateTime.Now
                }
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
                    Quantity = (item.Quantity ??0),
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(detail);

                // Optionally update product stock
                var product = _context.Products.Find(item.ProductId);
                if (product != null && product.Quantity.HasValue)
                {
                    product.Quantity = Math.Max(0, (product.Quantity ??0) - (item.Quantity ??0));
                    _context.Products.Update(product);
                }
            }
            _context.SaveChanges();

            // Clear coupon from session after successful order
            HttpContext.Session.Remove("AppliedCouponCode");
            HttpContext.Session.Remove("AppliedCouponPercent");

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
                TotalAmount = cart.Sum(i => (i.Quantity ??0) * i.Price),
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
                    Quantity = item.Quantity ??0,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(detail);

                var product = _context.Products.Find(item.ProductId);
                if (product != null && product.Quantity.HasValue)
                {
                    product.Quantity = Math.Max(0, (product.Quantity ??0) - (item.Quantity ??0));
                    _context.Products.Update(product);
                }
            }

            _context.SaveChanges();

            _cartService.ClearCart();
            try
            {
                // Example: save order to database or perform payment
                bool success = true; // change this based on your logic

                if (success)
                {
                    TempData["SuccessMessage"] = "🎉 Your order has been placed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ Something went wrong while placing your order.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "⚠️ An unexpected error occurred. Please try again.";
            }

//            return RedirectToAction("View"); // reloads the checkout page

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
        public IActionResult AddToCart(int id, int quantity =1, string size = null)
        {


            // Size check
            if (string.IsNullOrEmpty(size))
            {
                TempData["ErrorMessage"] = "Please select a size before adding to cart.";
                return RedirectToAction("Index", "Shop"); // go back to shop, don't add yet
            }

            // Add to cart
            if (quantity <=0) quantity =1;
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
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                TempData["Error"] = "Please enter a coupon code.";
                return RedirectToAction("Index");
            }

            var coupon = await _context.Coupon
                .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

            if (coupon == null || (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.Now))
            {
                TempData["Error"] = "Invalid or expired coupon.";
                return RedirectToAction("Index");
            }

            // calculate totals from cart service
            decimal subtotal = _cartService.GetSubtotal();
            decimal shipping = _cartService.GetShippingFee();
            decimal total = _cartService.GetTotal(); // subtotal + shipping normally
            decimal discountAmount = total * (coupon.DiscountPercent /100);
            decimal finalTotal = total - discountAmount;

            // Store the values in ViewBag for the Index view
            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = shipping;
            ViewBag.Discount = discountAmount;
            ViewBag.Total = finalTotal;
            ViewBag.AppliedCoupon = coupon.Code;

            // Persist coupon in session so it applies during checkout
            HttpContext.Session.SetString("AppliedCouponCode", coupon.Code);
            HttpContext.Session.SetString("AppliedCouponPercent", coupon.DiscountPercent.ToString());

            TempData["Success"] = $"Coupon '{coupon.Code}' applied successfully!";

            return View("Index", _cartService.GetOrderDetails());
        }
    }
}
