using EXE201_Backend.Data;
using EXE201_Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<Utils.DtoMapper>());

            builder.Services.AddDbContext<ExeContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("DATABASE_CONNECTION")));

            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>()
                .AddSingleton<ITimeProvider, TimeMachine>();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseRouting();

            app.UseCors("FrontendPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
