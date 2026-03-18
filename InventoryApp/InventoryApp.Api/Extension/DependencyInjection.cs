using InventoryApp.Api.Extension;
using InventoryApp.Api.Services;
using InventoryApp.Application.Configurations;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InventoryApp.Api.Extension;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigurations(configuration);
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy
                    = JsonNamingPolicy.CamelCase;
            });
        services.AddForwardedHeaders();
        services.AddAuthentication(configuration);
        services.AddSwagger();
        services.AddCors();
        services.AddRateLimiter();
        services.AddSignalR();
        services.AddServices();

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
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

        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins("https://inventoryapp-web.onrender.com", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("login", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
            });
        });

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>();
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = jwtSettings?.Key ?? throw new InvalidOperationException("Jwt:Key missing.");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };

        })
        .AddGoogle("Google", options =>
        {
            options.ClientId = authSettings.GoogleClientId!;
            options.ClientSecret = authSettings.GoogleClientSecret!;
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.CallbackPath = "/signin-google";
        })
        .AddFacebook("Facebook", options =>
        {
            options.AppId = authSettings.FacebookAppId!;
            options.AppSecret = authSettings.FacebookAppSecret!;
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

        return services;
    }

    private static IServiceCollection AddForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IDiscussionHubService, DiscussionHubService>();

        return services;
    }

    private static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services
          .AddOptions<AuthenticationSettings>()
          .Bind(configuration.GetSection(AuthenticationSettings.SectionName))
          .ValidateOnStart();

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
        });

        return services;
    }
}
