namespace MyShopAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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
    }
}
