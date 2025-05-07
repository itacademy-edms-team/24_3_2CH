using Microsoft.EntityFrameworkCore;
using NewImageBoard.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� Razor Pages
builder.Services.AddRazorPages();

// ����������� DbContext
builder.Services.AddDbContext<ImageBoardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ImageBoardContext")));

// ��������� ��������������
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Error";
    });

// ��������� �����������
builder.Services.AddAuthorization();

//// ���������� IWebHostEnvironment ��� ������ � �������
//builder.Services.AddSingleton<IWebHostEnvironment>(sp => sp.GetRequiredService<IWebHostEnvironment>());


var app = builder.Build();

// ��������� middleware
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