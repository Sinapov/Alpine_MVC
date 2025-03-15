using AlpineNeeds.Data;
using AlpineNeeds.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AlpineNeeds.Models;
using AlpineNeeds.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add optional local configuration
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity with role support
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Bind AdminCredentials section to options
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));

var app = builder.Build();

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

app.Run();
