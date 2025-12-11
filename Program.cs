using Microsoft.EntityFrameworkCore;
using MyShopServer.Infrastructure.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyShopServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // 1. DbContext + SQLite
            // =========================
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? "Data Source=MyShop.db";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            // =========================
            // 2. GraphQL server
            // =========================
            builder.Services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddProjections()
                .AddFiltering()
                .AddSorting();

            var app = builder.Build();

            // =========================
            // 3. Migrate + Seed (nếu đã tạo AppDbContextSeed)
            // =========================
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();

                // Nếu bạn đã tạo AppDbContextSeed thì giữ dòng này,
                // chưa có thì comment lại.
                await AppDbContextSeed.SeedAsync(db);
            }

            // =========================
            // 4. Map GraphQL endpoint
            // =========================
            app.MapGraphQL("/graphql");

            await app.RunAsync();
        }
    }
}
