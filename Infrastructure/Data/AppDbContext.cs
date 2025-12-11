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

        // Tự động tìm và apply tất cả IEntityTypeConfiguration<> trong assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
