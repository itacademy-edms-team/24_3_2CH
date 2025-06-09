using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ForumProject.Services.Implementations
{
    public class UserFingerprintService : IUserFingerprintService
    {
        private readonly ApplicationDbContext _context;

        public UserFingerprintService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserFingerprint> GetOrCreateFingerprintAsync(HttpContext httpContext)
        {
            // Собираем данные для отпечатка
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // Если IP-адрес недоступен (например, в некоторых тестовых окружениях), используем заглушку
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "UNKNOWN_IP";
            }

            var rawFingerprint = $"{ipAddress}-{userAgent}";

            // Хешируем отпечаток, чтобы не хранить прямые данные
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawFingerprint));
                var fingerprintHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                // Ищем существующий отпечаток в БД
                var existingFingerprint = await _context.UserFingerprints
                    .FirstOrDefaultAsync(f => f.FingerprintHash == fingerprintHash);

                if (existingFingerprint != null)
                {
                    return existingFingerprint;
                }
                else
                {
                    // Если не найден, создаем новый
                    var newFingerprint = new UserFingerprint
                    {
                        FingerprintHash = fingerprintHash,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserFingerprints.Add(newFingerprint);
                    await _context.SaveChangesAsync();
                    return newFingerprint;
                }
            }
        }
    }
}