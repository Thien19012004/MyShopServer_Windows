using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyShopServer.Application.GraphQL;
using MyShopServer.Application.GraphQL.Mutations;
using MyShopServer.Application.GraphQL.Queries;
using MyShopServer.Application.GraphQL.Types;
using MyShopServer.Application.Services.Implementations;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Infrastructure.Data;
using System.Text;

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
            // 2. Services (DI)
            // =========================
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReportService, ReportService>();

            // =========================
            // 3. JWT Authentication
            // =========================
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? "DEV_KEY_CHANGE_ME";

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                });
            builder.Services.AddAuthorization();
            // =========================
            // 4. GraphQL server
            // =========================
            builder.Services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddTypeExtension<AuthMutations>()
                .AddTypeExtension<ProductQueries>()
                .AddTypeExtension<ProductMutations>()
                .AddTypeExtension<CategoryQueries>()
                .AddTypeExtension<CategoryMutations>()
                .AddTypeExtension<ReportQueries>()
                .AddTypeExtension<OrderQueries>()
                .AddTypeExtension<OrderMutations>()

                .AddType<ProductListItemType>()
                .AddType<ProductDetailType>()
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .AddAuthorization();

            var app = builder.Build();

            // =========================
            // 5. Migrate + (optional) seed
            // =========================
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                await AppDbContextSeed.SeedAsync(db);
            }

            // =========================
            // 6. Middleware
            // =========================
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGraphQL("/graphql");

            await app.RunAsync();
        }
    }
}
