using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.ProductsCount = await _context.Products.CountAsync();
            ViewBag.OrdersCount = await _context.Orders.CountAsync();
            ViewBag.UsersCount = await _context.Users.CountAsync();
            ViewBag.TodayOrders = await _context.Orders.Where(o => o.DeliveryDate.Date == DateTime.Today).CountAsync();
            ViewBag.TodaySum = await _context.Orders.Where(o => o.DeliveryDate.Date == DateTime.Today).SumAsync(o => o.TotalAmount);
            ViewBag.TotalStock = await _context.Products.SumAsync(p => p.Stock);

            return View();
        }

        // ========= ТОВАРЫ =========
        public async Task<IActionResult> Products()
        {
            return View(await _context.Products.ToListAsync());
        }

        public async Task<IActionResult> ProductEdit(int? id)
        {
            if (id == null) return View(new Product());
            var product = await _context.Products.FindAsync(id);
            return View(product ?? new Product());
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(Product product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "Недопустимый формат файла. Разрешены: JPG, PNG, GIF, WEBP");
                    return View(product);
                }

                var fileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/" + fileName;
            }

            if (ModelState.IsValid)
            {
                if (product.Id == 0) _context.Products.Add(product);
                else _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Products");
            }
            return View(product);
        }

        public async Task<IActionResult> ProductDelete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }

        // ========= ЗАКАЗЫ =========
        public async Task<IActionResult> Orders()
        {
            return View(await _context.Orders.OrderByDescending(o => o.DeliveryDate).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Orders");
        }

        // ========= ПОЛЬЗОВАТЕЛИ =========
        public async Task<IActionResult> Users()
        {
            return View(await _context.Users.ToListAsync());
        }
    }
}