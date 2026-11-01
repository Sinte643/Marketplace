using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;

namespace Marketplace.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("ProfileGuest", "Account");

            var orders = await _context.Orders
                .OrderByDescending(o => o.DeliveryDate)
                .ToListAsync();
            return View(orders);
        }
    }
}