using KILVRA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;


namespace KILVRA.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OnlineClothesShopContext _context;
        private const string CartSessionKey = "OrderDetails";
        public CartService(IHttpContextAccessor httpContextAccessor,OnlineClothesShopContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public List<Product> GetOrderDetails()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(CartSessionKey);
            return cartJson == null ? new List<Product>() : JsonConvert.DeserializeObject<List<Product>>(cartJson);
        }
        public void AddToCart(int ProductId, int quantity = 1, string size = null)
        {

            if (quantity <= 0) return;

            var cart = GetOrderDetails();

            var existing = cart.FirstOrDefault(x => x.ProductId == ProductId && x.Size == size);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                // Fetch product from db to get name/price
                var product = _context.Products.Find(ProductId);
                if (product == null) return;

                cart.Add(new Product
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    Size = size // store chosen size
                });
            }

            SaveOrderDetails(cart);
        }
        public void RemoveFromCart(int productId, string size = null)
        {
            var cart = GetOrderDetails();
            var itemToRemove = cart.FirstOrDefault(ci => ci.ProductId == productId && ci.Size == size);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveOrderDetails(cart);
            }
        }
        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(CartSessionKey);
        }
        private void SaveOrderDetails(List<Product> products)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = JsonConvert.SerializeObject(products);
            session.SetString(CartSessionKey, cartJson);
        }
        public decimal GetSubtotal()
        {
            var cart = GetOrderDetails();
            return cart.Sum(item => item.Price * (item.Quantity ?? 0));
        }
        public decimal GetShippingFee()
        {
            var cart = GetOrderDetails();
            if (!cart.Any())
                return 0; // no items, no shipping fee

            return 30000; // flat fee (you can change this)
        }

        public decimal GetTotal()
        {
            var subtotal = GetSubtotal();
            var shipping = GetShippingFee();
            return subtotal + shipping;
        }
        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetOrderDetails();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null && quantity > 0)
                item.Quantity = quantity;
            else if (item != null && quantity <= 0)
                cart.Remove(item);
            SaveOrderDetails(cart);
        }
    }
}
