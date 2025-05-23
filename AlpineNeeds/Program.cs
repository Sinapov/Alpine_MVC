using AlpineNeeds.Data;
using AlpineNeeds.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AlpineNeeds.Models;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;


System.Console.WriteLine("Starting AlpineNeeds application...");
var builder = WebApplication.CreateBuilder(args);

// Add optional local configuration
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity with role support and custom SignInManager for cart merging
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddCartMergeOnLogin(); // Add custom SignInManager

// Configure Localization
builder.Services.AddPortableObjectLocalization(options =>
{
    options.ResourcesPath = "Localization";
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("bg")
    };
    options.DefaultRequestCulture = new RequestCulture("bg");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

// Add HttpContextAccessor, required for the CartService
builder.Services.AddHttpContextAccessor();

// Add Session services (required for guest cart functionality)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register CartService
builder.Services.AddScoped<ICartService, CartService>();
// Register Checkout session service
builder.Services.AddScoped<ICheckoutSessionService, CheckoutSessionService>();
// Register OrderService
builder.Services.AddScoped<IOrderService, OrderService>();

// Bind AdminCredentials section to options
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));

builder.Services
    .AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// Add localization middleware
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Seed roles and admin user using the method from ApplicationDbContext
await ApplicationDbContext.SeedRolesAndAdminAsync(app.Services);

// Seed products and their images
await ApplicationDbContext.SeedProductsAsync(app.Services);

app.Run();
