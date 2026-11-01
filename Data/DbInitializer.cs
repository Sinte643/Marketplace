using Marketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Marketplace.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            var hasher = new PasswordHasher<User>();
            // Демо-пользователь с заказами
            if (!context.Users.Any(u => u.Email == "demo@shop.ru"))
            {
                var demoUser = new User
                {
                    FirstName = "Дмитрий",
                    LastName = "Петров",
                    Email = "demo@shop.ru",
                    Phone = "+79998887766",
                    Role = "User"
                };
                demoUser.Password = hasher.HashPassword(demoUser, "demo123");
                context.Users.Add(demoUser);
                context.SaveChanges();

                var demoOrders = new Order[]
                {
        new Order { OrderNumber = "CD-10001", DeliveryDate = DateTime.Now.AddDays(-5), DeliveryTime = "10:30", PaymentMethod = "Карта", TotalAmount = 1999, ItemCount = 2, Status = "Доставлен" },
        new Order { OrderNumber = "CD-10002", DeliveryDate = DateTime.Now.AddDays(-2), DeliveryTime = "14:00", PaymentMethod = "Наличные", TotalAmount = 3499, ItemCount = 1, Status = "Отправлен" },
        new Order { OrderNumber = "CD-10003", DeliveryDate = DateTime.Now, DeliveryTime = "09:15", PaymentMethod = "Карта", TotalAmount = 999, ItemCount = 1, Status = "В обработке" },
        new Order { OrderNumber = "CD-10004", DeliveryDate = DateTime.Now.AddDays(2), DeliveryTime = "16:45", PaymentMethod = "Карта", TotalAmount = 5999, ItemCount = 3, Status = "Ожидает оплаты" }
                };
                context.Orders.AddRange(demoOrders);
                context.SaveChanges();
            }
            // Товары
            if (!context.Products.Any())
            {
                var products = new Product[]
 {
    new Product { Name = "Рубашка-поло", Category = "Одежда", SubCategory = "Мужская одежда", Price = 999, Description = "Хит сезона! Бежевая поло Premium Cotton.", Composition = "95% хлопок, 5% эластан", CareInstructions = "Машинная стирка при 30°С", AvailableSizes = "S,M,L,XL,XXL",Stock = 10, ImageUrl = null },
    new Product { Name = "Безрукавка", Category = "Одежда", SubCategory = "Мужская одежда", Price = 999, Description = "Утеплённая безрукавка.", AvailableSizes = "M,L,XL",Stock = 10, ImageUrl = null },
    new Product { Name = "Брюки классические", Category = "Одежда", SubCategory = "Мужская одежда", Price = 1499, Description = "Классические брюки прямого кроя.", AvailableSizes = "30,32,34,36",Stock = 10, ImageUrl = null },
    new Product { Name = "Футболка базовая", Category = "Одежда", SubCategory = "Мужская одежда", Price = 599, Description = "Базовая футболка из органического хлопка.", AvailableSizes = "S,M,L,XL",Stock = 10, ImageUrl = null },
    new Product { Name = "Джинсы скинни", Category = "Одежда", SubCategory = "Мужская одежда", Price = 1899, Description = "Узкие джинсы с эффектом растяжения.", AvailableSizes = "30,32,34",Stock = 10, ImageUrl = null },
    new Product { Name = "Свитер вязаный", Category = "Одежда", SubCategory = "Мужская одежда", Price = 1299, Description = "Тёплый вязаный свитер крупной вязки.", AvailableSizes = "M,L,XL",Stock = 10, ImageUrl = null },
    new Product { Name = "Кожаная куртка", Category = "Одежда", SubCategory = "Верхняя одежда", Price = 3999, Description = "Стильная кожаная куртка из натуральной кожи.", AvailableSizes = "S,M,L,XL",Stock = 10, ImageUrl = null },
    new Product { Name = "Пуховик зимний", Category = "Одежда", SubCategory = "Верхняя одежда", Price = 4999, Description = "Тёплый пуховик с капюшоном.", AvailableSizes = "M,L,XL,XXL",Stock = 10, ImageUrl = null },
    new Product { Name = "Пальто шерстяное", Category = "Одежда", SubCategory = "Верхняя одежда", Price = 5999, Description = "Классическое пальто из итальянской шерсти.", AvailableSizes = "M,L,XL",Stock = 10, ImageUrl = null },
    new Product { Name = "Кроссовки Casual", Category = "Обувь", SubCategory = "Мужская обувь", Price = 2499, Description = "Удобные кроссовки на каждый день.", AvailableSizes = "40,41,42,43,44",Stock = 10, ImageUrl = null },
    new Product { Name = "Туфли классические", Category = "Обувь", SubCategory = "Мужская обувь", Price = 3499, Description = "Классические туфли из натуральной кожи.", AvailableSizes = "40,41,42,43",Stock = 10, ImageUrl = null },
    new Product { Name = "Ботинки зимние", Category = "Обувь", SubCategory = "Мужская обувь", Price = 4299, Description = "Утеплённые ботинки с протекторной подошвой.", AvailableSizes = "41,42,43,44",Stock = 10, ImageUrl = null },
    new Product { Name = "Ремень кожаный", Category = "Аксессуары", SubCategory = "Ремни", Price = 799, Description = "Кожаный ремень с металлической пряжкой.",Stock = 10, ImageUrl = null },
    new Product { Name = "Шарф шерстяной", Category = "Аксессуары", SubCategory = "Шарфы", Price = 599, Description = "Тёплый шарф из мериносовой шерсти.",Stock = 10, ImageUrl = null },
    new Product { Name = "Перчатки кожаные", Category = "Аксессуары", SubCategory = "Перчатки", Price = 899, Description = "Кожаные перчатки с сенсорными пальцами.",Stock = 10, ImageUrl = null },
    new Product { Name = "Шапка вязаная", Category = "Аксессуары", SubCategory = "Головные уборы", Price = 499, Description = "Вязаная шапка с отворотом.",Stock = 10, ImageUrl = null }
 };
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // Заказы
            if (!context.Orders.Any())
            {
                var orders = new Order[]
                {
                    new Order { OrderNumber = "CD-01345", DeliveryDate = new DateTime(2024, 2, 22), DeliveryTime = "13:55", PaymentMethod = "Карта", TotalAmount = 1513, ItemCount = 2 },
                    new Order { OrderNumber = "PVZ-02645", DeliveryDate = new DateTime(2024, 2, 20), DeliveryTime = "18:30", PaymentMethod = "Наличные", TotalAmount = 1513, ItemCount = 3 },
                    new Order { OrderNumber = "CD-02231", DeliveryDate = new DateTime(2024, 2, 21), DeliveryTime = "17:55", PaymentMethod = "Карта", TotalAmount = 999, ItemCount = 1 },
                    new Order { OrderNumber = "PVZ-03245", DeliveryDate = new DateTime(2024, 2, 25), DeliveryTime = "18:25", PaymentMethod = "Наличные", TotalAmount = 2498, ItemCount = 5 }
                };
                context.Orders.AddRange(orders);
                context.SaveChanges();
            }
            // Администратор
            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                var admin = new User
                {
                    FirstName = "Админ",
                    LastName = "Магазина",
                    Email = "admin@shop.ru",
                    Phone = "+70000000000",
                    Role = "Admin"
                };
                admin.Password = hasher.HashPassword(admin, "admin123");
                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}