using BNC.Microservices.Auth;
using BNC.Microservices.Auth.Repositories;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using SecretsHandler;
using System.Globalization;

namespace BNC.Microservices.MsAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuración de Cultura Global (Estandarización)
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            #region Hardening y Seguridad (DLL Security)
            // Deshabilita cabecera Server en Kestrel
            builder.Services.AddSecurityHardening();

            // Configura la política CORS bancaria desde appsettings.json
            builder.Services.AddBankingCors(builder.Configuration, "BNCCorsPolicy");
            #endregion

            #region Secrets Handler
            // Soporte para Docker/Kubernetes Secrets
            if (Directory.Exists("/run/secrets"))
                builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

            // Registra e inicia el descifrado automático de secretos
            builder.Services.RegisterSecretsHandler(builder.Configuration);
            #endregion

            #region Base de Datos (DLL Infraestructura)
            builder.Services.AddDatabaseLayer(builder.Configuration);
            #endregion

            // 2. Inyección de Dependencias del Negocio
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.Configure<RepositorySettings>(
                builder.Configuration.GetSection("RepositorySettings")
            );

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt")
            );

            builder.Services.AddHealthChecks();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.UseSecurityHardening();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("BNCCorsPolicy");
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();

            app.Run();
        }
    }
}
