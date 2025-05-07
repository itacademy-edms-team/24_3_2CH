using Microsoft.EntityFrameworkCore;
using NewImageBoard.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов Razor Pages
builder.Services.AddRazorPages();

// Регистрация DbContext
builder.Services.AddDbContext<ImageBoardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ImageBoardContext")));

// Настройка аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Error";
    });

// Настройка авторизации
builder.Services.AddAuthorization();

//// Добавление IWebHostEnvironment для работы с файлами
//builder.Services.AddSingleton<IWebHostEnvironment>(sp => sp.GetRequiredService<IWebHostEnvironment>());


var app = builder.Build();

// Настройка middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Redirect root to BoardsView
app.MapGet("/", async context =>
{
    context.Response.Redirect("/BoardsView");
    await Task.CompletedTask;
});

app.Run();