using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebATB.Data;
using WebATB.Data.Entities.Identity;
using WebATB.Extensions;
using WebATB.Interfaces;
using WebATB.Services;
using WebATB.Data.Entities.Idenity;

var builder = WebApplication.CreateBuilder(args);

// Google Auth
builder.Services.AddAuthentication()
    .AddCookie()
    .AddGoogle(opt =>
    {
        opt.ClientId = builder.Configuration["GoogleKeys:ClientId"];
        opt.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
        opt.SignInScheme = IdentityConstants.ExternalScheme; // explicit
        opt.SaveTokens = true;
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppATBDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MyConnectionATB")));

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppATBDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(o =>
{
    o.ValidationInterval = TimeSpan.Zero;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "MyAreaAdmin",
    areaName: "Admin",
    pattern: "Admin/{controller=Users}/{action=Index}/{id?}")
    .RequireAuthorization("AdminOnly");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}")
    .WithStaticAssets();

Dictionary<string, string> imageSizes = new()
{
    { "ImagesDir", "images" },
    { "AvatarDir", "avatars" },
};

foreach (var (key, value) in imageSizes)
{
    var dirName = builder.Configuration.GetValue<string>(key) ?? value;

    var dir = Path.Combine(Directory.GetCurrentDirectory(), dirName);
    Directory.CreateDirectory(dir);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(dir),
        RequestPath = $"/{value}"
    });
}

await app.SeedDataAsync();

app.Run();