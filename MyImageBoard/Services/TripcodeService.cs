using System.Security.Cryptography;
using System.Text;

namespace ForumProject.Services
{
    public interface ITripcodeService
    {
        string GenerateTripcode(string input);
    }

    public class TripcodeService : ITripcodeService
    {
        public string GenerateTripcode(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Contains("#"))
                return input;

            var parts = input.Split('#');
            if (parts.Length != 2)
                return input;

            var username = parts[0];
            var password = parts[1];

            // Классический алгоритм хеширования трипкодов с имиджборд
            // Используем DES в режиме ECB с солью "H."
            var salt = "H.";
            var truncatedPassword = password.Length > 8 ? password[..8] : password;
            
            // Добавляем соль к паролю
            var saltedPassword = truncatedPassword + salt;
            
            // Хешируем с помощью DES в режиме ECB
            var hashedBytes = MD5.HashData(Encoding.UTF8.GetBytes(saltedPassword));
            var tripcode = Convert.ToBase64String(hashedBytes)[..10]; // Берем первые 10 символов base64

            return $"{username}!{tripcode}";
        }
    }
} 