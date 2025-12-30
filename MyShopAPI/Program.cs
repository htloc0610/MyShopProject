using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.Services;

namespace MyShopAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );
            
            // Register DatabaseSeeder
            builder.Services.AddScoped<DatabaseSeeder>();
            
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS policy to allow WPF client to connect
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Seed database on startup (only in Development mode)
            // Will only seed if database is empty (no products exist)
            if (app.Environment.IsDevelopment())
            {
                await SeedDatabaseAsync(app);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use CORS - must be before UseAuthorization
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Add logging middleware to see all requests
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Incoming request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                await next();
                logger.LogInformation("Response status: {StatusCode}", context.Response.StatusCode);
            });

            app.MapControllers();

            app.Run();
        }

        private static async Task SeedDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var seeder = services.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
