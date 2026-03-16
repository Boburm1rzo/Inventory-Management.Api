using InventoryApp.Api.Extension;
using InventoryApp.Api.Hubs;
using InventoryApp.Api.Middleware;
using InventoryApp.Infrastructure.Extensions;
using InventoryApp.Infrastructure.Seeders;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext());

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApi(builder.Configuration);
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryApp API V1");
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseHttpsRedirection();
    app.UseForwardedHeaders();
    app.UseRouting();
    app.UseCors("Frontend");
    app.UseCookiePolicy();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<DiscussionHub>("/hubs/discussion");
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}