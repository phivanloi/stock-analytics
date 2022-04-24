using Pl.Sas.Core;
using Pl.Sas.Infrastructure.Loging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributeLogService(option =>
{
    option.BaseUrl = builder.Configuration["LoggingSettings:BaseUrl"];
    option.Secret = builder.Configuration["LoggingSettings:Secret"];
    option.ServerName = builder.Configuration["LoggingSettings:ServerName"] ?? Utilities.IdentityServer();
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
