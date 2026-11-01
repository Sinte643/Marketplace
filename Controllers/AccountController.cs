using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ID текущего пользователя из подписанной cookie (null, если не вошёл)
        private int? CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Пользователь с таким email уже существует");

            if (!ModelState.IsValid)
                return View(user);

            // Роль нельзя задавать из формы: регистрируется только обычный пользователь
            user.Role = "User";
            user.Password = _passwordHasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && await VerifyPasswordAsync(user, password ?? string.Empty))
            {
                await SignInUserAsync(user);
                return RedirectToAction("Profile");
            }
            ViewBag.Error = "Неверный email или пароль";
            return View();
        }

        // GET: /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = CurrentUserId;
            if (userId == null)
            {
                return RedirectToAction("ProfileGuest");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("ProfileGuest");
            }

            var cards = await _context.Cards.Where(c => c.UserId == user.Id).ToListAsync();
            ViewBag.Cards = cards;

            return View(user);
        }

        // GET: /Account/ProfileGuest
        public IActionResult ProfileGuest()
        {
            return View();
        }

        // POST: /Account/AddCard
        [HttpPost]
        public async Task<IActionResult> AddCard(Card card)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login");

            card.UserId = userId.Value;
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            // Удаляем старые незащищённые cookie прежних версий
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("UserRole");
            Response.Cookies.Delete("UserName");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/DeleteCard/5
        public async Task<IActionResult> DeleteCard(int id)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login");

            var card = await _context.Cards.FindAsync(id);
            if (card != null && card.UserId == userId.Value)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile");
        }

        // GET: /Account/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(User updatedUser)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login");

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.MiddleName = updatedUser.MiddleName;
            user.Phone = updatedUser.Phone;

            await _context.SaveChangesAsync();

            // Обновляем имя в cookie входа
            await SignInUserAsync(user);

            return RedirectToAction("Profile");
        }

        // ===== Вспомогательные методы =====

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });
        }

        // Хэш у Identity V3 всегда начинается с "AQAAAA"
        private static bool LooksHashed(string value) =>
            value.Length >= 60 && value.StartsWith("AQAAAA", StringComparison.Ordinal);

        // Проверяет пароль. Если в базе остался старый пароль открытым текстом,
        // после успешного входа он автоматически заменяется на хэш.
        private async Task<bool> VerifyPasswordAsync(User user, string password)
        {
            if (LooksHashed(user.Password))
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Failed) return false;

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await _context.SaveChangesAsync();
                }
                return true;
            }

            var legacyMatches = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(user.Password),
                Encoding.UTF8.GetBytes(password));
            if (!legacyMatches) return false;

            user.Password = _passwordHasher.HashPassword(user, password);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
