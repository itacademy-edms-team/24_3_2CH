using ForumProject.Data;
using ForumProject.Services;
using ForumProject.Services.Implementations;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавляем DbContext (используйте вашу строку подключения, которая уже настроена)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Добавляем сервисы
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ILikeService, LikeService>();
            builder.Services.AddScoped<ILikeTypeService, LikeTypeService>();
            builder.Services.AddScoped<IThreadService, ThreadService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IBoardService, BoardService>();
            builder.Services.AddScoped<IUserFingerprintService, UserFingerprintService>();
            builder.Services.AddScoped<IComplaintService, ComplaintService>(); // Добавляем сервис жалоб
            builder.Services.AddScoped<ITripcodeService, TripcodeService>();
            builder.Services.AddScoped<IMediaFileService, MediaFileService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<ISuperUserService, SuperUserService>(); // Добавляем наш новый сервис

            // Add services to the container. (Razor Pages будет использовать этот код)
            builder.Services.AddRazorPages();
            builder.Services.AddControllers(); // Добавляем поддержку контроллеров

            // Add session support
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // ������ ����� ����������� ���� middleware ��� ����������� ������

            app.UseRouting();

            app.UseAuthorization();

            // Enable session
            app.UseSession();

            // app.MapStaticAssets(); // ,    ,   UseStaticFiles()
            app.MapRazorPages(); //    
                                 // .WithStaticAssets(); // ,    ,   UseStaticFiles()
                                 //      /
            app.MapControllers(); // Добавляем маршрутизацию для контроллеров

            // Initialize the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    Task.Run(async () =>
                    {
                        await DbInitializer.Initialize(context);
                    }).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while initializing the database.");
                }
            }

            app.Run();
        }
    }
}