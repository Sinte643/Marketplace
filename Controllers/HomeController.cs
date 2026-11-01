using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var popularProducts = await _context.Products.Take(6).ToListAsync();
            return View(popularProducts);
        }
        public IActionResult Contacts()
        { 
            return View(); 
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Advertising()
        {
            return View();
        }
    }
}