using InventoryApp.Api.Services;
using InventoryApp.Application.Configurations;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace InventoryApp.Api.Extention;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            using var scope = services.BuildServiceProvider();
            var configuration = scope.GetRequiredService<IOptions<JwtSettings>>();
            var settings = configuration.Value;

            var key = settings.Key ?? throw new InvalidOperationException("Jwt:Key missing.");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = settings.Issuer,
                ValidAudience = settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            };
        })
        .AddGoogle("Google", options =>
        {
            using var scope = services.BuildServiceProvider();
            var configuration = scope.GetRequiredService<IOptions<AuthenticationSettings>>();
            var settings = configuration.Value;

            options.ClientId = settings.GoogleClientId!;
            options.ClientSecret = settings.GoogleClientSecret!;
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.CallbackPath = "/signin-google";
        })
        .AddFacebook("Facebook", options =>
        {

            using var scope = services.BuildServiceProvider();
            var configuration = scope.GetRequiredService<IOptions<AuthenticationSettings>>();
            var settings = configuration.Value;

            options.AppId = settings.FacebookAppId!;
            options.AppSecret = settings.FacebookAppSecret!;
            options.SignInScheme = IdentityConstants.ExternalScheme;

            options.CallbackPath = "/signin-facebook";
            options.Scope.Add("public_profile");
            options.Scope.Add("email");
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Name = "InventoryApp.External";
        });

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddAuthorization();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "InventoryApp", Version = "v1" });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins("https://inventoryb.netlify.app", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddSignalR();
        services.AddScoped<IDiscussionHubService, DiscussionHubService>();

        return services;
    }
}
