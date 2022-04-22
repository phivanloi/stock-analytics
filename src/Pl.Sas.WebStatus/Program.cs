using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders()
    .AddConsole()
    .AddSeq(builder.Configuration.GetSection("Seq"));

builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature?.Error, "Error url: {url}", url);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("Internal server error.");
        });
    });
}

app.UseHealthChecksUI(config =>
{
    config.ResourcesPath = "/ui/resources";
    config.UIPath = "/hc-ui";
    config.AddCustomStylesheet("wwwroot/css/site.css");
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
    {
        Predicate = r => r.Name.Contains("self")
    });
});
app.MapDefaultControllerRoute();

app.Logger.LogDebug("Web status application {AppVersion} started at {Now}", app.Configuration["AppSettings:AppVersion"], DateTime.Now);
app.Run();
