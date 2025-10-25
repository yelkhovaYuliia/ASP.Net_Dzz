using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebATB.Data;
using WebATB.Data.Entities.Idenity;
using WebATB.Extensions;
using WebATB.Interfaces;
using WebATB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppATBDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyConnectionATB")));

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddEntityFrameworkStores<AppATBDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();




//системв маршрутизації по замовчуванню


app.MapAreaControllerRoute(
    name: "MyAreaAdmin",
    areaName: "Admin",
    pattern: "admin/{controller=Home}/{action=Index}/{id?}");
//.RequireAuthorization("AdminOnly");

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
    //Дозволяємо доступ до файлів в папці images по шляху /images
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(dir),
        RequestPath = $"/{value}"
    });
}

await app.SeedDataAsync();

app.Run();