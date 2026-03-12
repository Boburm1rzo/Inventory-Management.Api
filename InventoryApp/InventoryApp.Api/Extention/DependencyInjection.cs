using InventoryApp.Api.Services;
using InventoryApp.Application.Configurations;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace InventoryApp.Api.Extention;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services
            .AddOptions<AuthenticationSettings>()
            .Bind(configuration.GetSection(AuthenticationSettings.SectionName))
            .ValidateOnStart();

        var authSettings = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>();
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();


        var a = configuration["AuthenticationSettings:GoogleClientId"];
        var b = configuration["AuthenticationSettings:GoogleClientSecret"];

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" ################### google clientId: {a} ###################");
        Console.WriteLine($" ################### google Secret : {b} ###################");
        Console.WriteLine($" ################### facebook ApiId: {authSettings?.FacebookAppId} ###################");
        Console.WriteLine($" ################### facebook Secret: {authSettings?.FacebookAppSecret} ###################");
        Console.ResetColor();

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
