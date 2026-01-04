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
         FullName = "Nguyễn Văn An",
         IsActive = true,
   CreatedAt = now.AddDays(-10),
         },
   new User
{
             UserId = 4,
    Username = "sale_b",
       PasswordHash = PasswordHasher.Hash("sale123"),
                FullName = "Trần Thị Bình",
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
     new Category
            {
      CategoryId = 1,
       Name = "Điện Thoại & Phụ Kiện",
      Description = "Điện thoại thông minh, tai nghe, ốp lưng, sạc dự phòng"
  },
            new Category
     {
       CategoryId = 2,
         Name = "Laptop & Máy Tính Bảng",
                Description = "Laptop, máy tính bảng, bàn phím, chuột, balo laptop"
       },
 new Category
  {
       CategoryId = 3,
     Name = "Thiết Bị Âm Thanh",
       Description = "Tai nghe, loa bluetooth, amply, micro"
            },
            new Category
  {
                CategoryId = 4,
    Name = "Đồng Hồ Thông Minh",
     Description = "Smartwatch, vòng đeo tay thông minh"
     },
        };
        await db.Categories.AddRangeAsync(categories);

        // ======================
        // PRODUCTS - ĐIỆN THOẠI & PHỤ KIỆN (22 sản phẩm)
        // ======================
        var phoneProducts = new List<Product>();
        var phoneData = new[]
     {
        ("IP14PM-BLK", "iPhone 14 Pro Max 256GB Đen", 28000000, 33990000, "Chip A16 Bionic, Camera 48MP, Dynamic Island"),
    ("IP14PM-WHT", "iPhone 14 Pro Max 256GB Trắng", 28000000, 33990000, "Chip A16 Bionic, Camera 48MP, Dynamic Island"),
  ("IP14P-BLK", "iPhone 14 Pro 128GB Đen", 24000000, 28990000, "Chip A16 Bionic, Camera 48MP, màn hình 6.1 inch"),
          ("IP13PM-BLU", "iPhone 13 Pro Max 256GB Xanh", 22000000, 26990000, "Chip A15 Bionic, Camera 12MP, pin khủng"),
            ("IP13-BLK", "iPhone 13 128GB Đen", 16000000, 19990000, "Chip A15 Bionic, màn hình 6.1 inch OLED"),
            ("SS-S23U-BLK", "Samsung Galaxy S23 Ultra 256GB Đen", 25000000, 30990000, "Snapdragon 8 Gen 2, Camera 200MP, S Pen"),
 ("SS-S23U-GRN", "Samsung Galaxy S23 Ultra 512GB Xanh Lá", 27000000, 33990000, "Snapdragon 8 Gen 2, Camera 200MP"),
 ("SS-S23P-WHT", "Samsung Galaxy S23+ 256GB Trắng", 20000000, 24990000, "Snapdragon 8 Gen 2, màn hình 6.6 inch"),
            ("SS-ZF4-BLK", "Samsung Galaxy Z Fold4 512GB Đen", 32000000, 39990000, "Màn hình gập, Snapdragon 8+ Gen 1"),
            ("SS-ZF4-GLD", "Samsung Galaxy Z Fold4 256GB Vàng", 30000000, 37990000, "Màn hình gập, S Pen support"),
        ("SS-A54-BLK", "Samsung Galaxy A54 5G 128GB Đen", 8000000, 10490000, "Exynos 1380, Camera 50MP, pin 5000mAh"),
          ("SS-A54-VIO", "Samsung Galaxy A54 5G 256GB Tím", 8500000, 11490000, "Exynos 1380, màn hình Super AMOLED"),
    ("XM-13P-BLK", "Xiaomi 13 Pro 256GB Đen", 18000000, 22990000, "Snapdragon 8 Gen 2, Camera Leica 50MP"),
            ("XM-13P-WHT", "Xiaomi 13 Pro 512GB Trắng", 20000000, 24990000, "Camera Leica, sạc nhanh 120W"),
  ("XM-12T-BLU", "Xiaomi 12T Pro 256GB Xanh", 12000000, 15990000, "Snapdragon 8+ Gen 1, Camera 200MP"),
     ("OP-11-GRN", "OPPO Find N2 Flip 256GB Xanh", 16000000, 19990000, "Màn hình gập dọc, Dimensity 9000+"),
   ("OP-R11-BLK", "OPPO Reno11 5G 256GB Đen", 9000000, 11990000, "Dimensity 7050, Camera 50MP, pin 5000mAh"),
  ("VV-X90P-BLK", "Vivo X90 Pro 256GB Đen", 20000000, 24990000, "Dimensity 9200, Camera Zeiss 50MP"),
            ("AW-PP-BLK", "Tai Nghe AirPods Pro 2 (USB-C)", 5000000, 6490000, "Chống ồn chủ động, chip H2, hộp sạc USB-C"),
  ("AW-3-WHT", "Tai Nghe AirPods 3 Trắng", 3500000, 4490000, "Spatial Audio, chống nước IPX4"),
            ("SS-BUD-WHT", "Tai Nghe Samsung Galaxy Buds2 Pro Trắng", 3000000, 3990000, "Chống ồn chủ động, âm thanh 360"),
            ("CH-30W-WHT", "Sạc Nhanh 30W Type-C Trắng", 250000, 399000, "Sạc nhanh PD 30W, tương thích đa thiết bị"),
     };

        for (int i = 0; i < phoneData.Length; i++)
        {
            var (sku, name, importPrice, salePrice, desc) = phoneData[i];
            phoneProducts.Add(new Product
            {
                ProductId = i + 1,
                Sku = sku,
                Name = name,
                ImportPrice = importPrice,
                SalePrice = salePrice,
                StockQuantity = 50 + (i * 10),
                Description = desc,
                CategoryId = 1,
                CreatedAt = now.AddDays(-20 + i),
            });
        }

        // ======================
        // PRODUCTS - LAPTOP & MÁY TÍNH BẢNG (22 sản phẩm)
        // ======================
        var laptopProducts = new List<Product>();
        var laptopData = new[]
    {
   ("MBP-M2P-14", "MacBook Pro 14 M2 Pro 512GB Xám", 45000000, 52990000, "Chip M2 Pro 10 core, RAM 16GB, màn hình Liquid Retina XDR"),
          ("MBP-M2P-16", "MacBook Pro 16 M2 Pro 512GB Bạc", 55000000, 62990000, "Chip M2 Pro 12 core, RAM 16GB, pin 22 giờ"),
            ("MBA-M2-13", "MacBook Air M2 13 256GB Xanh", 25000000, 29990000, "Chip M2 8 core, màn hình 13.6 inch, siêu mỏng nhẹ"),
            ("MBA-M2-15", "MacBook Air M2 15 512GB Bạc", 30000000, 34990000, "Chip M2, màn hình 15.3 inch, pin 18 giờ"),
            ("MBA-M1-13", "MacBook Air M1 13 256GB Vàng", 20000000, 23990000, "Chip M1 8 core, RAM 8GB, fanless"),
    ("DL-XPS13-BLK", "Dell XPS 13 Plus i7 512GB Đen", 35000000, 41990000, "Intel Core i7-1360P, RAM 16GB, màn hình 13.4 inch OLED"),
        ("DL-XPS15-SLV", "Dell XPS 15 i7 1TB Bạc", 45000000, 52990000, "Intel Core i7-13700H, RTX 4050, RAM 32GB"),
         ("HP-ENV-14", "HP Envy 14 i7 512GB Bạc", 28000000, 33990000, "Intel Core i7-1355U, RAM 16GB, màn hình 2.8K"),
       ("HP-SPE-16", "HP Spectre x360 16 i7 1TB Xanh", 42000000, 49990000, "Intel Core i7-13700H, màn hình cảm ứng 16 inch 3K"),
  ("AS-ZB-14", "Asus Zenbook 14 OLED i5 512GB Xanh", 18000000, 22990000, "Intel Core i5-1340P, RAM 16GB, màn hình OLED 2.8K"),
            ("AS-ZB-DUO", "Asus Zenbook Pro 14 Duo OLED i9 1TB", 55000000, 64990000, "Intel Core i9-13900H, RTX 4060, màn hình phụ ScreenPad Plus"),
   ("AS-ROG-G15", "Asus ROG Strix G15 R7 RTX4060 1TB", 35000000, 42990000, "AMD Ryzen 7 7735HS, RTX 4060, màn hình 165Hz"),
         ("LG-GRAM-17", "LG Gram 17 i7 512GB Trắng", 35000000, 41990000, "Intel Core i7-1360P, RAM 16GB, siêu nhẹ 1.35kg"),
      ("LG-GRAM-16", "LG Gram 16 i5 512GB Xám", 28000000, 33990000, "Intel Core i5-1340P, màn hình 16 inch 2.5K"),
   ("MS-SF-13", "Microsoft Surface Laptop 5 13 i7 512GB Đen", 32000000, 37990000, "Intel Core i7-1255U, màn hình PixelSense 13.5 inch"),
            ("MS-SP-9", "Microsoft Surface Pro 9 i7 256GB Bạc", 28000000, 33990000, "Intel Core i7-1255U, màn hình 13 inch, có thể tháo rời"),
      ("AC-SWIFT-14", "Acer Swift 14 i5 512GB Bạc", 16000000, 19990000, "Intel Core i5-1340P, RAM 16GB, pin 12 giờ"),
            ("AC-PRED-15", "Acer Predator Helios 16 i7 RTX4070 1TB", 42000000, 49990000, "Intel Core i7-13700HX, RTX 4070, màn hình 165Hz"),
     ("IPD-PR-M2-11", "iPad Pro M2 11 inch 256GB Xám", 18000000, 21990000, "Chip M2 8 core, màn hình Liquid Retina"),
     ("IPD-PR-M2-13", "iPad Pro M2 12.9 inch 512GB Bạc", 28000000, 33990000, "Chip M2, màn hình Liquid Retina XDR"),
 ("IPD-AIR-M1-10", "iPad Air M1 10.9 inch 256GB Xanh", 13000000, 15990000, "Chip M1, hỗ trợ Apple Pencil 2"),
            ("SS-TAB-S9", "Samsung Galaxy Tab S9 11 inch 256GB Xám", 16000000, 19990000, "Snapdragon 8 Gen 2, màn hình Dynamic AMOLED 2X"),
        };

        for (int i = 0; i < laptopData.Length; i++)
        {
            var (sku, name, importPrice, salePrice, desc) = laptopData[i];
            laptopProducts.Add(new Product
            {
                ProductId = phoneProducts.Count + i + 1,
                Sku = sku,
                Name = name,
                ImportPrice = importPrice,
                SalePrice = salePrice,
                StockQuantity = 30 + (i * 5),
                Description = desc,
                CategoryId = 2,
                CreatedAt = now.AddDays(-20 + i),
            });
        }

        // ======================
        // PRODUCTS - THIẾT BỊ ÂM THANH (22 sản phẩm)
        // ======================
        var audioProducts = new List<Product>();
        var audioData = new[]
     {
            ("SO-WH1000-BLK", "Tai Nghe Sony WH-1000XM5 Đen", 7000000, 8990000, "Chống ồn hàng đầu, 30 giờ pin, LDAC Hi-Res"),
            ("SO-WH1000-SLV", "Tai Nghe Sony WH-1000XM5 Bạc", 7000000, 8990000, "Chống ồn AI, 8 micro HD, Multipoint"),
            ("SO-WH1000M4", "Tai Nghe Sony WH-1000XM4 Đen", 5500000, 6990000, "Chống ồn thế hệ 4, Speak-to-Chat"),
        ("BO-QC45-BLK", "Tai Nghe Bose QuietComfort 45 Đen", 6000000, 7490000, "Chống ồn tuyệt vời, âm thanh Bose signature"),
      ("BO-QC45-WHT", "Tai Nghe Bose QuietComfort 45 Trắng", 6000000, 7490000, "24 giờ pin, Aware Mode"),
      ("BO-700-BLK", "Tai Nghe Bose 700 Đen", 7000000, 8490000, "11 mức chống ồn, thiết kế cao cấp"),
         ("SH-MX50-BLK", "Tai Nghe Sennheiser Momentum 4 Đen", 6500000, 7990000, "60 giờ pin, âm thanh audiophile"),
            ("JBL-T770-BLU", "Tai Nghe JBL Tune 770NC Xanh", 1800000, 2490000, "Chống ồn chủ động, 70 giờ pin, JBL Pure Bass"),
 ("JBL-T770-BLK", "Tai Nghe JBL Tune 770NC Đen", 1800000, 2490000, "Bluetooth 5.3, sạc nhanh 5 phút dùng 3 giờ"),
            ("JBL-FLIP6-RED", "Loa JBL Flip 6 Đỏ", 2200000, 2990000, "Chống nước IP67, 12 giờ pin, PartyBoost"),
     ("JBL-FLIP6-BLU", "Loa JBL Flip 6 Xanh", 2200000, 2990000, "Bass mạnh mẽ, kết nối 2 loa"),
("JBL-CHG5-BLK", "Loa JBL Charge 5 Đen", 3500000, 4490000, "Pin 20 giờ, có thể sạc cho điện thoại"),
        ("JBL-XTRE3-BLK", "Loa JBL Xtreme 3 Đen", 5000000, 6490000, "Bass cực mạnh, chống nước IP67, pin 15 giờ"),
("SO-SRS-XB43", "Loa Sony SRS-XB43 Đen", 3500000, 4490000, "Extra Bass, đèn LED, chống nước IP67"),
 ("SO-SRS-XG300", "Loa Sony SRS-XG300 Xám", 5500000, 6990000, "X-Balanced Speaker, 25 giờ pin"),
     ("BO-SE2-BLK", "Loa Bose SoundLink Flex Đen", 3000000, 3790000, "Chống nước IP67, PositionIQ technology"),
            ("BO-RE-300", "Loa Bose SoundLink Revolve+ II Xám", 6000000, 7490000, "Âm thanh 360 độ, pin 17 giờ"),
            ("HM-K-MINI", "Loa Harman Kardon Onyx Studio 7 Đen", 4500000, 5490000, "Thiết kế đẹp, âm thanh HiFi, pin 8 giờ"),
      ("HM-K-AUR", "Loa Harman Kardon Aura Studio 3 Đen", 6000000, 7490000, "Đèn LED ambient, âm thanh 360 độ"),
     ("MA-HP-BLK", "Tai Nghe Marshall Major IV Đen", 2500000, 3290000, "Thiết kế iconic, 80 giờ pin"),
            ("MA-MB-BLK", "Loa Marshall Emberton II Đen", 4000000, 4990000, "Âm thanh Marshall đặc trưng, chống nước IPX7"),
          ("MA-SF-BLK", "Loa Marshall Stanmore III Đen", 8000000, 9990000, "Loa để bàn, Bluetooth 5.2, âm thanh Hi-Fi"),
  };

        for (int i = 0; i < audioData.Length; i++)
        {
            var (sku, name, importPrice, salePrice, desc) = audioData[i];
            audioProducts.Add(new Product
            {
                ProductId = phoneProducts.Count + laptopProducts.Count + i + 1,
                Sku = sku,
                Name = name,
                ImportPrice = importPrice,
                SalePrice = salePrice,
                StockQuantity = 40 + (i * 5),
                Description = desc,
                CategoryId = 3,
                CreatedAt = now.AddDays(-20 + i),
            });
        }

        // ======================
        // PRODUCTS - ĐỒNG HỒ THÔNG MINH (22 sản phẩm)
        // ======================
        var watchProducts = new List<Product>();
        var watchData = new[]
        {
    ("AW-S9-45-BLK", "Apple Watch Series 9 GPS 45mm Đen", 9000000, 10990000, "Chip S9, màn hình sáng 2000 nits, cảm biến nhiệt độ"),
        ("AW-S9-45-RED", "Apple Watch Series 9 GPS 45mm Đỏ", 9000000, 10990000, "Always-On Retina, Double Tap gesture"),
 ("AW-S9-41-WHT", "Apple Watch Series 9 GPS 41mm Trắng", 8000000, 9990000, "WatchOS 10, theo dõi sức khỏe toàn diện"),
   ("AW-SE-44-BLK", "Apple Watch SE GPS 44mm Đen", 5500000, 6990000, "Chip S8, phát hiện va chạm, chống nước 50m"),
         ("AW-SE-40-SLV", "Apple Watch SE GPS 40mm Bạc", 5000000, 6490000, "Family Setup, Fitness+, giá trị tốt nhất"),
       ("AW-U2-49-TIT", "Apple Watch Ultra 2 GPS 49mm Titan", 18000000, 21990000, "Màn hình 3000 nits, pin 36 giờ, GPS kép"),
         ("SS-W6-CL-44", "Samsung Galaxy Watch6 Classic 44mm Đen", 7000000, 8490000, "Wear OS 4, One UI 5 Watch, vòng xoay"),
   ("SS-W6-40-SLV", "Samsung Galaxy Watch6 40mm Bạc", 5500000, 6990000, "Đo BIA, ECG, huyết áp, giấc ngủ"),
         ("SS-W6-44-GRN", "Samsung Galaxy Watch6 44mm Xanh", 6000000, 7490000, "Pin 2 ngày, sạc nhanh, chống nước 5ATM"),
    ("SS-W5P-45", "Samsung Galaxy Watch5 Pro 45mm Xám", 8000000, 9490000, "Pin 80 giờ, mặt sapphire, GPS tuyến đường"),
   ("GA-PW2-BLK", "Google Pixel Watch 2 41mm Đen", 7500000, 8990000, "Wear OS 4, Fitbit integration, ECG"),
 ("GA-PW2-SLV", "Google Pixel Watch 2 41mm Bạc", 7500000, 8990000, "Stress management, Safety Check"),
            ("XM-W2P-BLK", "Xiaomi Watch 2 Pro Đen", 5000000, 6490000, "Wear OS, Snapdragon W5+, AMOLED 1.43 inch"),
        ("XM-W2-BLU", "Xiaomi Watch 2 Xanh", 3500000, 4490000, "Wear OS, 150+ chế độ thể thao, pin 65 giờ"),
            ("XM-B8P-BLK", "Xiaomi Smart Band 8 Pro Đen", 1200000, 1590000, "Màn hình AMOLED 1.74 inch, GPS tích hợp"),
            ("XM-B8-BLK", "Xiaomi Smart Band 8 Đen", 800000, 1090000, "Màn hình 1.62 inch, pin 16 ngày, 150+ mode"),
            ("HW-W4P-BLK", "Huawei Watch 4 Pro Đen", 9000000, 10990000, "HarmonyOS 4, ECG, đo nhiệt độ da"),
    ("HW-GT4-GRN", "Huawei Watch GT 4 46mm Xanh", 5500000, 6990000, "Pin 14 ngày, TruSeen 5.5+, GPS kép"),
            ("HW-GT4-41-WHT", "Huawei Watch GT 4 41mm Trắng", 5000000, 6490000, "Thiết kế sang trọng, 100+ mode thể thao"),
            ("HW-B8-BLK", "Huawei Band 8 Đen", 800000, 1090000, "Siêu mỏng 8.99mm, pin 14 ngày, màn hình AMOLED"),
 ("AM-GTR4-BLK", "Amazfit GTR 4 46mm Đen", 4000000, 4990000, "GPS kép, pin 12 ngày, 150+ mode thể thao"),
      ("AM-BL5-BLU", "Amazfit Bip 5 Xanh", 1500000, 1990000, "Màn hình 1.91 inch, pin 11 ngày, Alexa built-in"),
        };

        for (int i = 0; i < watchData.Length; i++)
        {
            var (sku, name, importPrice, salePrice, desc) = watchData[i];
            watchProducts.Add(new Product
            {
                ProductId = phoneProducts.Count + laptopProducts.Count + audioProducts.Count + i + 1,
                Sku = sku,
                Name = name,
                ImportPrice = importPrice,
                SalePrice = salePrice,
                StockQuantity = 35 + (i * 5),
                Description = desc,
                CategoryId = 4,
                CreatedAt = now.AddDays(-20 + i),
            });
        }

        // Thêm tất cả products vào DB
        var allProducts = phoneProducts.Concat(laptopProducts).Concat(audioProducts).Concat(watchProducts).ToList();
        await db.Products.AddRangeAsync(allProducts);

        // ======================
        // PRODUCT IMAGES (3+ images per product)
        // ======================
        var productImages = new List<ProductImage>();
        int imageId = 1;

        foreach (var product in allProducts)
        {
            // Mỗi sản phẩm có 3 hình (main, detail1, detail2)
            productImages.Add(new ProductImage
            {
                ProductImageId = imageId++,
                ProductId = product.ProductId,
                ImagePath = $"images/products/{product.Sku.ToLower()}-main.jpg"
            });
            productImages.Add(new ProductImage
            {
                ProductImageId = imageId++,
                ProductId = product.ProductId,
                ImagePath = $"images/products/{product.Sku.ToLower()}-detail1.jpg"
            });
            productImages.Add(new ProductImage
            {
                ProductImageId = imageId++,
                ProductId = product.ProductId,
                ImagePath = $"images/products/{product.Sku.ToLower()}-detail2.jpg"
            });
        }

        await db.ProductImages.AddRangeAsync(productImages);

        // ======================
        // CUSTOMERS
        // ======================
        var customers = new[]
        {
        new Customer { CustomerId = 1, Name = "Nguyễn Văn Anh", Phone = "0901234567", Email = "anh.nv@gmail.com", Address = "123 Lê Lợi, Quận 1, TP.HCM" },
         new Customer { CustomerId = 2, Name = "Trần Thị Bích", Phone = "0912345678", Email = "bich.tt@gmail.com", Address = "456 Nguyễn Huệ, Quận 1, TP.HCM" },
        new Customer { CustomerId = 3, Name = "Lê Văn Cường", Phone = "0923456789", Email = "cuong.lv@gmail.com", Address = "789 Hai Bà Trưng, Quận 3, TP.HCM" },
  new Customer { CustomerId = 4, Name = "Phạm Thị Dung", Phone = "0934567890", Email = "dung.pt@gmail.com", Address = "321 Cách Mạng Tháng 8, Quận 10, TP.HCM" },
      new Customer { CustomerId = 5, Name = "Hoàng Văn Em", Phone = "0945678901", Email = "em.hv@gmail.com", Address = "654 Võ Văn Tần, Quận 3, TP.HCM" },
        };
        await db.Customers.AddRangeAsync(customers);

        // ======================
        // PROMOTIONS (3 scope: Product, Category, Order)
        // IMPORTANT: Using relative dates to ensure promotions are always active
        // ======================
        var promotions = new[]
        {
            // Product Promotions - Always active from 30 days ago to 30 days from now
            new Promotion
      {
     PromotionId = 1,
    Name = "Flash Sale iPhone - Giảm 15%",
   DiscountPercent = 15,
       StartDate = now.AddDays(-30),  // 30 days ago
          EndDate = now.AddDays(30),     // 30 days from now
                Scope = PromotionScope.Product
          },
     new Promotion
            {
                PromotionId = 2,
  Name = "Sale Tai Nghe Cao Cấp - Giảm 20%",
   DiscountPercent = 20,
    StartDate = now.AddDays(-30),
      EndDate = now.AddDays(30),
        Scope = PromotionScope.Product
            },
 new Promotion
       {
      PromotionId = 3,
         Name = "Giảm 10% Apple Watch Series 9",
 DiscountPercent = 10,
       StartDate = now.AddDays(-30),
    EndDate = now.AddDays(30),
      Scope = PromotionScope.Product
            },
 
            // Category Promotions
            new Promotion
     {
           PromotionId = 4,
     Name = "Sale Toàn Bộ Laptop - Giảm 12%",
              DiscountPercent = 12,
                StartDate = now.AddDays(-30),
      EndDate = now.AddDays(30),
    Scope = PromotionScope.Category
    },
     new Promotion
         {
    PromotionId = 5,
       Name = "Khuyến Mãi Thiết Bị Âm Thanh - Giảm 18%",
        DiscountPercent = 18,
          StartDate = now.AddDays(-30),
            EndDate = now.AddDays(30),
                Scope = PromotionScope.Category
      },
            new Promotion
            {
      PromotionId = 6,
           Name = "Sale Đồng Hồ Thông Minh - Giảm 8%",
    DiscountPercent = 8,
    StartDate = now.AddDays(-30),
       EndDate = now.AddDays(30),
   Scope = PromotionScope.Category
},
            
 // Order Promotions
          new Promotion
   {
       PromotionId = 7,
             Name = "Giảm 5% Tổng Đơn Hàng",
       DiscountPercent = 5,
          StartDate = now.AddDays(-30),
            EndDate = now.AddDays(30),
                Scope = PromotionScope.Order
    },
     new Promotion
     {
     PromotionId = 8,
        Name = "Khách Hàng Thân Thiết - Giảm 10% Tổng Đơn",
    DiscountPercent = 10,
                StartDate = now.AddDays(-30),
     EndDate = now.AddDays(30),
        Scope = PromotionScope.Order
         },
   };
        await db.Promotions.AddRangeAsync(promotions);

        // Product Promotions (áp dụng cho sản phẩm cụ thể)
        var productPromotions = new[]
      {
   // Flash Sale iPhone (promotion 1)
     new ProductPromotion { ProductId = 1, PromotionId = 1 },  // iPhone 14 Pro Max Đen
            new ProductPromotion { ProductId = 2, PromotionId = 1 },  // iPhone 14 Pro Max Trắng
     new ProductPromotion { ProductId = 3, PromotionId = 1 },  // iPhone 14 Pro
 new ProductPromotion { ProductId = 4, PromotionId = 1 },  // iPhone 13 Pro Max
   new ProductPromotion { ProductId = 5, PromotionId = 1 },  // iPhone 13
            
 // Sale Tai Nghe Cao Cấp (promotion 2)
new ProductPromotion { ProductId = 45, PromotionId = 2 }, // Sony WH-1000XM5 Đen
       new ProductPromotion { ProductId = 46, PromotionId = 2 }, // Sony WH-1000XM5 Bạc
     new ProductPromotion { ProductId = 47, PromotionId = 2 }, // Sony WH-1000XM4
      new ProductPromotion { ProductId = 48, PromotionId = 2 }, // Bose QC45 Đen
        new ProductPromotion { ProductId = 49, PromotionId = 2 }, // Bose QC45 Trắng
     
         // Apple Watch Series 9 (promotion 3)
            new ProductPromotion { ProductId = 67, PromotionId = 3 }, // Apple Watch S9 45mm Đen
   new ProductPromotion { ProductId = 68, PromotionId = 3 }, // Apple Watch S9 45mm Đỏ
   new ProductPromotion { ProductId = 69, PromotionId = 3 }, // Apple Watch S9 41mm Trắng
        };
        await db.ProductPromotions.AddRangeAsync(productPromotions);

        // Category Promotions (áp dụng cho toàn bộ category)
        var categoryPromotions = new[]
         {
            new CategoryPromotion { CategoryId = 2, PromotionId = 4 }, // Laptop & Máy Tính Bảng
   new CategoryPromotion { CategoryId = 3, PromotionId = 5 }, // Thiết Bị Âm Thanh
 new CategoryPromotion { CategoryId = 4, PromotionId = 6 }, // Đồng Hồ Thông Minh
        };
        await db.CategoryPromotions.AddRangeAsync(categoryPromotions);

        // ======================
        // ORDERS + ITEMS + PAYMENTS + COMMISSIONS
        // ======================

        // Order 1: Đã thanh toán - có áp dụng product promotion và order promotion
        var order1 = new Order
        {
            OrderId = 1,
            CustomerId = 1,
            SaleId = 3,
            Status = OrderStatus.Paid,
            TotalPrice = 0, // sẽ tính sau
            CreatedAt = now.AddDays(-3),
        };

        // iPhone 14 Pro Max: 33,990,000 - 15% = 28,891,500
        // Tai nghe AirPods Pro 2: 6,490,000 (không có promotion)
        var order1Items = new[]
        {
            new OrderItem
   {
   OrderItemId = 1,
      OrderId = 1,
    ProductId = 1, // iPhone 14 Pro Max
    Quantity = 1,
      UnitPrice = 28891500, // đã giảm 15%
     TotalPrice = 28891500
         },
         new OrderItem
            {
      OrderItemId = 2,
       OrderId = 1,
              ProductId = 19, // AirPods Pro 2
          Quantity = 1,
   UnitPrice = 6490000,
      TotalPrice = 6490000
   }
        };

        // Subtotal = 35,381,500
        // Order discount 5% = 1,769,075
        // Total = 33,612,425
        order1.TotalPrice = 33612425;

        var order1Promotions = new[]
        {
    new OrderPromotion { OrderId = 1, PromotionId = 7 }, // Order promotion 5%
        };

        var payment1 = new Payment
        {
            PaymentId = 1,
            OrderId = 1,
            Method = PaymentMethod.BankTransfer,
            Amount = order1.TotalPrice,
            PaidAt = now.AddDays(-3).AddHours(1),
        };

        // Order 2: Đã thanh toán - áp dụng category promotion
        var order2 = new Order
        {
            OrderId = 2,
            CustomerId = 2,
            SaleId = 4,
            Status = OrderStatus.Paid,
            TotalPrice = 0,
            CreatedAt = now.AddDays(-2),
        };

        // MacBook Air M2: 29,990,000 - 12% = 26,391,200
        var order2Items = new[]
        {
         new OrderItem
   {
      OrderItemId = 3,
   OrderId = 2,
            ProductId = 25, // MacBook Air M2 13
      Quantity = 1,
     UnitPrice = 26391200, // đã giảm 12% (category promotion)
                TotalPrice = 26391200
      }
        };

        order2.TotalPrice = 26391200;

        OrderPromotion[] order2Promotions = Array.Empty<OrderPromotion>();

        var payment2 = new Payment
        {
            PaymentId = 2,
            OrderId = 2,
            Method = PaymentMethod.Cash,
            Amount = order2.TotalPrice,
            PaidAt = now.AddDays(-2).AddHours(2),
        };

        // Order 3: Chưa thanh toán - có nhiều promotions
        var order3 = new Order
        {
            OrderId = 3,
            CustomerId = 3,
            SaleId = 3,
            Status = OrderStatus.Created,
            TotalPrice = 0,
            CreatedAt = now.AddDays(-1),
        };

        // Sony WH-1000XM5: 8,990,000 - 20% = 7,192,000 (product promotion)
        // JBL Flip 6: 2,990,000 - 18% = 2,451,800 (category promotion)
        var order3Items = new[]
              {
     new OrderItem
            {
   OrderItemId = 4,
   OrderId = 3,
       ProductId = 45, // Sony WH-1000XM5
         Quantity = 1,
        UnitPrice = 7192000, // product promotion 20%
    TotalPrice = 7192000
      },
  new OrderItem
    {
       OrderItemId = 5,
     OrderId = 3,
              ProductId = 54, // JBL Flip 6
    Quantity = 2,
     UnitPrice = 2451800, // category promotion 18%
  TotalPrice = 4903600
  }
        };

        // Subtotal = 12,095,600
        // Order discount 10% = 1,209,560
        // Total = 10,886,040
        order3.TotalPrice = 10886040;

        var order3Promotions = new[]
     {
      new OrderPromotion { OrderId = 3, PromotionId = 8 }, // Order promotion 10%
  };

        // Order 4: Đã hủy
        var order4 = new Order
        {
            OrderId = 4,
            CustomerId = 4,
            SaleId = 3,
            Status = OrderStatus.Cancelled,
            TotalPrice = 10990000,
            CreatedAt = now.AddDays(-4),
        };

        var order4Items = new[]
          {
       new OrderItem
         {
          OrderItemId = 6,
          OrderId = 4,
                ProductId = 67, // Apple Watch S9
    Quantity = 1,
                UnitPrice = 10990000,
    TotalPrice = 10990000
 }
   };

        await db.Orders.AddRangeAsync(order1, order2, order3, order4);
        await db.OrderItems.AddRangeAsync(order1Items);
        await db.OrderItems.AddRangeAsync(order2Items);
        await db.OrderItems.AddRangeAsync(order3Items);
        await db.OrderItems.AddRangeAsync(order4Items);
        await db.OrderPromotions.AddRangeAsync(order1Promotions);
        await db.OrderPromotions.AddRangeAsync(order2Promotions);
        await db.OrderPromotions.AddRangeAsync(order3Promotions);
        await db.Payments.AddRangeAsync(payment1, payment2);

        // ======================
        // APP SETTINGS
        // ======================
        var settings = new[]
    {
          new AppSetting { Key = "PageSize", Value = "10" },
            new AppSetting { Key = "DefaultCurrency", Value = "VND" },
            new AppSetting { Key = "CommissionRate", Value = "10" },
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
        Action = "System initialized with seed data",
                CreatedAt = now.AddMinutes(-30)
      },
    new AuditLog
            {
       AuditLogId = 2,
          UserId = 1,
     Action = "Created 4 categories with 88 products",
      CreatedAt = now.AddMinutes(-25)
          },
            new AuditLog
            {
    AuditLogId = 3,
 UserId = 1,
    Action = "Created 8 promotions (3 scopes: Product, Category, Order)",
       CreatedAt = now.AddMinutes(-20)
         },
        new AuditLog
 {
             AuditLogId = 4,
        UserId = 3,
                Action = "Order #1 created and paid - Customer: Nguyễn Văn Anh",
         CreatedAt = now.AddDays(-3)
         },
         new AuditLog
            {
    AuditLogId = 5,
                UserId = 4,
          Action = "Order #2 created and paid - Customer: Trần Thị Bích",
     CreatedAt = now.AddDays(-2)
 },
        };
        await db.AuditLogs.AddRangeAsync(logs);

        // ======================
        // KPI TIERS
        // ======================
        var kpiTiers = new[]
        {
 // NOTE: Simplified KPI: MinRevenue is interpreted as MinAchievedPercent (progress vs target)
 new KpiTier
 {
 KpiTierId =1,
 Name = "Bronze",
 MinRevenue =100, // >=100% target
 BonusPercent =2,
 Description = "Đạt >=100% target tháng - Thưởng thêm2%",
 DisplayOrder =1
 },
 new KpiTier
 {
 KpiTierId =2,
 Name = "Silver",
 MinRevenue =120, // >=120% target
 BonusPercent =5,
 Description = "Đạt >=120% target tháng - Thưởng thêm5%",
 DisplayOrder =2
 },
 new KpiTier
 {
 KpiTierId =3,
 Name = "Gold",
 MinRevenue =150, // >=150% target
 BonusPercent =8,
 Description = "Đạt >=150% target tháng - Thưởng thêm8%",
 DisplayOrder =3
 },
 new KpiTier
 {
 KpiTierId =4,
 Name = "Platinum",
 MinRevenue =200, // >=200% target
 BonusPercent =12,
 Description = "Đạt >=200% target tháng - Thưởng thêm12%",
 DisplayOrder =4
 }
 };
        await db.KpiTiers.AddRangeAsync(kpiTiers);

        // ======================
        // SALE KPI TARGETS (Tháng hiện tại cho 2 sales)
        // ======================
        var currentYear = now.Year;
        var currentMonth = now.Month;

        var saleKpiTargets = new[]
        {
      new SaleKpiTarget
     {
           SaleKpiTargetId = 1,
             SaleId = 3, // Nguyễn Văn An
            Year = currentYear,
       Month = currentMonth,
      TargetRevenue = 100_000_000, // Target 100 triệu
ActualRevenue = 0,
            CreatedAt = now.AddDays(-5)
        },
        new SaleKpiTarget
       {
     SaleKpiTargetId = 2,
         SaleId = 4, // Trần Thị Bình
  Year = currentYear,
    Month = currentMonth,
          TargetRevenue = 80_000_000, // Target 80 triệu
     ActualRevenue = 0,
         CreatedAt = now.AddDays(-5)
         }
   };
        await db.SaleKpiTargets.AddRangeAsync(saleKpiTargets);

        await db.SaveChangesAsync();
    }
}
