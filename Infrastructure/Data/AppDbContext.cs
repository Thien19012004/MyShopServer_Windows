using Microsoft.EntityFrameworkCore;
using MyShopServer.Domain.Entities;

namespace MyShopServer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ======================
    // AUTH / SECURITY
    // ======================
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // ======================
    // MASTER DATA
    // ======================
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    // ======================
    // CUSTOMER
    // ======================
    public DbSet<Customer> Customers => Set<Customer>();

    // ======================
    // ORDER / TRANSACTION
    // ======================
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // ======================
    // PROMOTION / KPI
    // ======================
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<ProductPromotion> ProductPromotions => Set<ProductPromotion>();
    public DbSet<Commission> Commissions => Set<Commission>();

    // ======================
    // CONFIG / AUDIT
    // ======================
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ======================
    // MODEL CONFIGURATION
    // ======================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----------------------
        // USER ↔ ROLE (N-N)
        // ----------------------
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // ----------------------
        // PRODUCT ↔ PROMOTION (N-N)
        // ----------------------
        modelBuilder.Entity<ProductPromotion>()
            .HasKey(pp => new { pp.ProductId, pp.PromotionId });

        modelBuilder.Entity<ProductPromotion>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.ProductPromotions)
            .HasForeignKey(pp => pp.ProductId);

        modelBuilder.Entity<ProductPromotion>()
            .HasOne(pp => pp.Promotion)
            .WithMany(p => p.ProductPromotions)
            .HasForeignKey(pp => pp.PromotionId);

        // ----------------------
        // CATEGORY → PRODUCT (1-N)
        // ----------------------
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------
        // PRODUCT → PRODUCT_IMAGE (1-N)
        // ----------------------
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ----------------------
        // CUSTOMER → ORDER (1-N)
        // ----------------------
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // ----------------------
        // USER (SALE) → ORDER (1-N)
        // ----------------------
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Sale)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------
        // ORDER → ORDER_ITEM (1-N)
        // ----------------------
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        // ----------------------
        // PRODUCT → ORDER_ITEM (1-N)
        // ----------------------
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

        // ----------------------
        // ORDER → PAYMENT (1-N)
        // ----------------------
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId);

        // ----------------------
        // USER (SALE) → COMMISSION
        // ----------------------
        modelBuilder.Entity<Commission>()
            .HasOne(c => c.Sale)
            .WithMany(u => u.Commissions)
            .HasForeignKey(c => c.SaleId);

        modelBuilder.Entity<Commission>()
            .HasOne(c => c.Order)
            .WithOne()
            .HasForeignKey<Commission>(c => c.OrderId);

        // ----------------------
        // APP_SETTING (Key-Value)
        // ----------------------
        modelBuilder.Entity<AppSetting>()
            .HasKey(s => s.Key);

        // ----------------------
        // AUDIT_LOG
        // ----------------------
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId);

        // ----------------------
        // INDEXES (optional nhưng ăn điểm)
        // ----------------------
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}
