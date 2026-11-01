using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;
using System.Text.Json;

namespace Marketplace.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        public async Task<IActionResult> Add(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == id.Value);
            int currentQty = existingItem?.Quantity ?? 0;

            if (currentQty >= product.Stock)
                return RedirectToAction("Index");

            if (existingItem != null) existingItem.Quantity++;
            else cart.Add(new CartItem { ProductId = product.Id, Name = product.Name, Price = product.Price, Quantity = 1 });

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int? id)
        {
            if (id == null) return NotFound();
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id.Value);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return RedirectToAction("Index");

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else if (quantity > product.Stock) item.Quantity = product.Stock;
                else item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("ProfileGuest", "Account");

            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            // Остатки списываются и заказ создаётся в одной транзакции:
            // либо всё выполнилось, либо ничего не изменилось
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var ids = cart.Select(c => c.ProductId).ToList();
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Сначала проверяем наличие всех позиций, и только потом списываем
            foreach (var item in cart)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || product.Stock < item.Quantity)
                {
                    TempData["CartError"] = $"Товара «{item.Name}» недостаточно на складе. Измените количество и повторите заказ.";
                    return RedirectToAction("Index");
                }
            }

            foreach (var item in cart)
                products[item.ProductId].Stock -= item.Quantity;

            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                DeliveryDate = DateTime.Now.AddDays(3),
                DeliveryTime = DateTime.Now.ToString("HH:mm"),
                PaymentMethod = "Карта",
                TotalAmount = cart.Sum(i => i.Total),
                ItemCount = cart.Sum(i => i.Quantity),
                Status = "В обработке"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index", "Orders");
        }

        // Номер заказа проверяется на уникальность
        private async Task<string> GenerateOrderNumberAsync()
        {
            string number;
            do
            {
                number = "CD-" + Random.Shared.Next(10000, 100000);
            }
            while (await _context.Orders.AnyAsync(o => o.OrderNumber == number));
            return number;
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}