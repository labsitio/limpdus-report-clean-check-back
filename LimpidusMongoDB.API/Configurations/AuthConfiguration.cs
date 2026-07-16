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
                : configuration["Jwt__Key"] ?? Environment.GetEnvironmentVariable("Jwt__Key") ?? string.Empty;

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            key.Length >= 32 ? key : "TEMP_DEV_KEY_REPLACE_IN_APPSETTINGS_MIN_32")),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
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
