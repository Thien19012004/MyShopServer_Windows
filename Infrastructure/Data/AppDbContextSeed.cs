using Microsoft.EntityFrameworkCore;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;
using MyShopServer.Domain.Common;

namespace MyShopServer.Infrastructure.Data;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Nếu đã có user rồi thì coi như DB đã seed, bỏ qua
        if (await db.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ======================
        // ROLES
        // ======================
        var roles = new[]
        {
            new Role { RoleId = 1, RoleName = RoleName.Admin },
            new Role { RoleId = 2, RoleName = RoleName.Moderator },
            new Role { RoleId = 3, RoleName = RoleName.Sale },
        };
        await db.Roles.AddRangeAsync(roles);

        // ======================
        // USERS
        // ======================
        // PasswordHash ở đây chỉ là placeholder, sau bạn đổi thành hash thật cũng ok
        var users = new[]
         {
            new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("admin123"),
                FullName = "System Administrator",
                IsActive = true,
                CreatedAt = now.AddDays(-30),
            },
            new User
            {
                UserId = 2,
                Username = "mod",
                PasswordHash = PasswordHasher.Hash("mod123"),
                FullName = "Store Moderator",
                IsActive = true,
                CreatedAt = now.AddDays(-20),
            },
            new User
            {
                UserId = 3,
                Username = "sale_a",
                PasswordHash = PasswordHasher.Hash("sale123"),
                FullName = "Sale A",
                IsActive = true,
                CreatedAt = now.AddDays(-10),
            },
            new User
            {
                UserId = 4,
                Username = "sale_b",
                PasswordHash = PasswordHasher.Hash("sale123"),
                FullName = "Sale B",
                IsActive = true,
                CreatedAt = now.AddDays(-5),
            },
        };
        await db.Users.AddRangeAsync(users);

        var userRoles = new[]
        {
            new UserRole { UserId = 1, RoleId = 1 }, // admin
            new UserRole { UserId = 2, RoleId = 2 }, // moderator
            new UserRole { UserId = 3, RoleId = 3 }, // sale A
            new UserRole { UserId = 4, RoleId = 3 }, // sale B
        };
        await db.UserRoles.AddRangeAsync(userRoles);

        // ======================
        // CATEGORIES
        // ======================
        var categories = new[]
        {
            new Category { CategoryId = 1, Name = "Beverages", Description = "Đồ uống" },
            new Category { CategoryId = 2, Name = "Snacks",    Description = "Đồ ăn vặt" },
            new Category { CategoryId = 3, Name = "Household", Description = "Đồ gia dụng" },
        };
        await db.Categories.AddRangeAsync(categories);

        // ======================
        // PRODUCTS
        // ======================
        var products = new[]
        {
            new Product
            {
                ProductId = 1,
                Sku = "CF001",
                Name = "Cà phê sữa đá",
                ImportPrice = 15000,
                SalePrice = 22000,
                StockQuantity = 100,
                Description = "Cà phê sữa đá truyền thống",
                CategoryId = 1,
                CreatedAt = now.AddDays(-20),
            },
            new Product
            {
                ProductId = 2,
                Sku = "TEA001",
                Name = "Trà tắc",
                ImportPrice = 8000,
                SalePrice = 15000,
                StockQuantity = 80,
                Description = "Trà tắc mát lạnh",
                CategoryId = 1,
                CreatedAt = now.AddDays(-18),
            },
            new Product
            {
                ProductId = 3,
                Sku = "SN001",
                Name = "Bánh quy bơ",
                ImportPrice = 7000,
                SalePrice = 12000,
                StockQuantity = 60,
                Description = "Bánh quy bơ giòn tan",
                CategoryId = 2,
                CreatedAt = now.AddDays(-15),
            },
            new Product
            {
                ProductId = 4,
                Sku = "SN002",
                Name = "Khoai tây chiên",
                ImportPrice = 10000,
                SalePrice = 18000,
                StockQuantity = 50,
                Description = "Khoai tây chiên giòn",
                CategoryId = 2,
                CreatedAt = now.AddDays(-12),
            },
            new Product
            {
                ProductId = 5,
                Sku = "HS001",
                Name = "Nước rửa chén",
                ImportPrice = 15000,
                SalePrice = 28000,
                StockQuantity = 40,
                Description = "Nước rửa chén hương chanh",
                CategoryId = 3,
                CreatedAt = now.AddDays(-10),
            },
            new Product
            {
                ProductId = 6,
                Sku = "HS002",
                Name = "Nước lau nhà",
                ImportPrice = 20000,
                SalePrice = 35000,
                StockQuantity = 30,
                Description = "Nước lau nhà hương hoa",
                CategoryId = 3,
                CreatedAt = now.AddDays(-8),
            },
        };
        await db.Products.AddRangeAsync(products);

        var productImages = new[]
        {
            new ProductImage { ProductImageId = 1, ProductId = 1, ImagePath = "images/products/cf001-1.jpg" },
            new ProductImage { ProductImageId = 2, ProductId = 2, ImagePath = "images/products/tea001-1.jpg" },
            new ProductImage { ProductImageId = 3, ProductId = 3, ImagePath = "images/products/sn001-1.jpg" },
            new ProductImage { ProductImageId = 4, ProductId = 4, ImagePath = "images/products/sn002-1.jpg" },
            new ProductImage { ProductImageId = 5, ProductId = 5, ImagePath = "images/products/hs001-1.jpg" },
            new ProductImage { ProductImageId = 6, ProductId = 6, ImagePath = "images/products/hs002-1.jpg" },
        };
        await db.ProductImages.AddRangeAsync(productImages);

        // ======================
        // CUSTOMERS
        // ======================
        var customers = new[]
        {
            new Customer { CustomerId = 1, Name = "Nguyễn Văn A", Phone = "0901000001", Email = "a@example.com", Address = "Q1, HCM" },
            new Customer { CustomerId = 2, Name = "Trần Thị B",   Phone = "0902000002", Email = "b@example.com", Address = "Q3, HCM" },
            new Customer { CustomerId = 3, Name = "Lê Văn C",      Phone = "0903000003", Email = "c@example.com", Address = "Q7, HCM" },
        };
        await db.Customers.AddRangeAsync(customers);

        // ======================
        // PROMOTIONS
        // ======================
        var promotions = new[]
        {
            new Promotion
            {
                PromotionId = 1,
                Name = "Giảm 10% đồ uống",
                DiscountPercent = 10,
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(7),
            },
            new Promotion
            {
                PromotionId = 2,
                Name = "Snack cuối tuần -15%",
                DiscountPercent = 15,
                StartDate = now.AddDays(-3),
                EndDate = now.AddDays(3),
            },
        };
        await db.Promotions.AddRangeAsync(promotions);

        var productPromotions = new[]
        {
            new ProductPromotion { ProductId = 1, PromotionId = 1 }, // CF001
            new ProductPromotion { ProductId = 2, PromotionId = 1 }, // TEA001
            new ProductPromotion { ProductId = 3, PromotionId = 2 }, // SN001
            new ProductPromotion { ProductId = 4, PromotionId = 2 }, // SN002
        };
        await db.ProductPromotions.AddRangeAsync(productPromotions);

        // ======================
        // ORDERS + ITEMS + PAYMENTS + COMMISSIONS
        // ======================

        // Order 1: Paid, Sale A
        var order1 = new Order
        {
            OrderId = 1,
            CustomerId = 1,
            SaleId = 3,                               // sale_a
            Status = OrderStatus.Paid,
            TotalPrice = 0,                           // sẽ cập nhật sau
            CreatedAt = now.AddDays(-2),
        };

        var order1Items = new[]
        {
            new OrderItem
            {
                OrderItemId = 1,
                OrderId = order1.OrderId,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 22000,
                TotalPrice = 2 * 22000
            },
            new OrderItem
            {
                OrderItemId = 2,
                OrderId = order1.OrderId,
                ProductId = 3,
                Quantity = 1,
                UnitPrice = 12000,
                TotalPrice = 12000
            }
        };
        order1.TotalPrice = order1Items.Sum(i => i.TotalPrice);

        var payment1 = new Payment
        {
            PaymentId = 1,
            OrderId = order1.OrderId,
            Method = PaymentMethod.Cash,
            Amount = order1.TotalPrice,
            PaidAt = now.AddDays(-2).AddHours(1),
        };

        // Hoa hồng 10% cho Sale A
        var commission1 = new Commission
        {
            CommissionId = 1,
            SaleId = 3,
            OrderId = order1.OrderId,
            Amount = (int)(order1.TotalPrice * 0.10),
            CalculatedAt = now.AddDays(-2).AddHours(2),
        };

        // Order 2: Created, Sale B (chưa thanh toán)
        var order2 = new Order
        {
            OrderId = 2,
            CustomerId = 2,
            SaleId = 4,
            Status = OrderStatus.Created,
            TotalPrice = 0,
            CreatedAt = now.AddDays(-1),
        };

        var order2Items = new[]
        {
            new OrderItem
            {
                OrderItemId = 3,
                OrderId = order2.OrderId,
                ProductId = 5,
                Quantity = 1,
                UnitPrice = 28000,
                TotalPrice = 28000
            },
            new OrderItem
            {
                OrderItemId = 4,
                OrderId = order2.OrderId,
                ProductId = 6,
                Quantity = 1,
                UnitPrice = 35000,
                TotalPrice = 35000
            }
        };
        order2.TotalPrice = order2Items.Sum(i => i.TotalPrice);

        // Order 3: Cancelled
        var order3 = new Order
        {
            OrderId = 3,
            CustomerId = 3,
            SaleId = 3,
            Status = OrderStatus.Cancelled,
            TotalPrice = 18000,
            CreatedAt = now.AddDays(-1),
        };

        var order3Items = new[]
        {
            new OrderItem
            {
                OrderItemId = 5,
                OrderId = order3.OrderId,
                ProductId = 4,
                Quantity = 1,
                UnitPrice = 18000,
                TotalPrice = 18000
            }
        };

        await db.Orders.AddRangeAsync(order1, order2, order3);
        await db.OrderItems.AddRangeAsync(order1Items);
        await db.OrderItems.AddRangeAsync(order2Items);
        await db.OrderItems.AddRangeAsync(order3Items);
        await db.Payments.AddAsync(payment1);
        await db.Commissions.AddAsync(commission1);

        // ======================
        // APP SETTINGS
        // ======================
        var settings = new[]
        {
            new AppSetting { Key = "PageSize",   Value = "10" },
            new AppSetting { Key = "LastScreen", Value = "Dashboard" },
            new AppSetting { Key = "IsFirstRun", Value = "false" },
        };
        await db.AppSettings.AddRangeAsync(settings);

        // ======================
        // AUDIT LOGS
        // ======================
        var logs = new[]
        {
            new AuditLog
            {
                AuditLogId = 1,
                UserId = 1,
                Action = "Seed: initial admin account created",
                CreatedAt = now.AddMinutes(-10)
            },
            new AuditLog
            {
                AuditLogId = 2,
                UserId = 3,
                Action = "Seed: sample order #1 created",
                CreatedAt = now.AddMinutes(-9)
            }
        };
        await db.AuditLogs.AddRangeAsync(logs);

        await db.SaveChangesAsync();
    }
}
