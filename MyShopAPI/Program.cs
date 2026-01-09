using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyShopAPI.Data;
using MyShopAPI.Models;
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

            // ====================================================
            // ASP.NET Core Identity Configuration
            // ====================================================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ====================================================
            // JWT Authentication Configuration
            // ====================================================
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No tolerance for token expiry
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                        logger.LogInformation("Token validated. User: {User}. Claims: {Claims}", 
                            context.Principal?.Identity?.Name, string.Join(", ", claims ?? new List<string>()));
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader))
                        {
                            logger.LogInformation("Creating ticket with Auth header: {HeaderLength} chars", authHeader.Length);
                        }
                        else 
                        {
                            logger.LogWarning("No Authorization header received");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // ====================================================
            // Authorization Policies
            // ====================================================
            builder.Services.AddAuthorization(options =>
            {
                // Policy for Owner-only actions (e.g., delete operations)
                options.AddPolicy("OwnerOnly", policy =>
                    policy.RequireRole("Owner"));

                // Policy for Staff - can manage inventory but not delete
                options.AddPolicy("CanManageInventory", policy =>
                    policy.RequireRole("Owner", "Staff"));
            });

            // ====================================================
            // Register Application Services
            // ====================================================
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContextService, UserContextService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<DatabaseSeeder>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ====================================================
            // Swagger with JWT Support
            // ====================================================
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyShop API", Version = "v1" });

                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Add CORS policy to allow WinUI client to connect
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
            
            // app.UseHttpsRedirection(); // Disable for local dev to avoid Auth header stripping

            // Serve static files (for product images)
            app.UseStaticFiles();

            // IMPORTANT: UseAuthentication must come BEFORE UseAuthorization
            app.UseAuthentication();
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
