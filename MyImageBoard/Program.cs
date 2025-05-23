using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyImageBoard.Services;
using MyImageBoard.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы Razor Pages
builder.Services.AddRazorPages();

// Регистрируем DbContext
builder.Services.AddDbContext<ImageBoardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ImageBoardContext")));

// Регистрируем сервисы
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IThreadService, ThreadService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

// Настраиваем аутентификацию
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Error";
    });

// Настраиваем авторизацию
builder.Services.AddAuthorization();

var app = builder.Build();

// Настраиваем middleware
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