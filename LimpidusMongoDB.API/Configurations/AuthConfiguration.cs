using System.Text;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace LimpidusMongoDB.Api.Configurations
{
    public static class AuthConfiguration
    {
        public static void AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

            var key = !string.IsNullOrWhiteSpace(jwt.Key)
                ? jwt.Key
                : configuration["Jwt:Key"]
                  ?? configuration["Jwt__Key"]
                  ?? Environment.GetEnvironmentVariable("Jwt__Key")
                  ?? string.Empty;

            if (string.IsNullOrWhiteSpace(key) || key.Length < 32
                || key.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
                || key.StartsWith("TEMP_DEV_KEY", StringComparison.OrdinalIgnoreCase)
                || key.StartsWith("DEV_ONLY_", StringComparison.OrdinalIgnoreCase))
            {
                // Em produção o App Setting Jwt__Key (ou Jwt:Key) é obrigatório.
                // Em Development mantém fallback local para não bloquear o time.
                var env = configuration["ASPNETCORE_ENVIRONMENT"]
                    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                    ?? "Production";
                if (!string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Jwt:Key inválida/ausente. Defina o App Setting Jwt__Key (>=32 chars, sem placeholder CHANGE_ME).");
                }

                if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
                    key = "DEV_ONLY_CHANGE_ME_CLEANCHECK_JWT_KEY_32+";
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            // Garante que IOptions<JwtSettings> use a mesma key resolvida (login/token).
            services.PostConfigure<JwtSettings>(settings =>
            {
                settings.Key = key;
                if (string.IsNullOrWhiteSpace(settings.Issuer))
                    settings.Issuer = jwt.Issuer;
                if (string.IsNullOrWhiteSpace(settings.Audience))
                    settings.Audience = jwt.Audience;
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy(AuthPolicies.CanExportReports, policy =>
                    policy.RequireRole(AuthRoles.Franqueado, AuthRoles.Admin));

                options.AddPolicy(AuthPolicies.AdminOnly, policy =>
                    policy.RequireRole(AuthRoles.Admin));
            });
        }

        public static void AddSwaggerBearer(this Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions option)
        {
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o JWT: Bearer {token}"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        }
    }
}
