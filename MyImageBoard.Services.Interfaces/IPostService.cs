using System.Threading.Tasks;

namespace MyImageBoard.Services.Interfaces
{
    public interface IPostService
    {
        Task DeletePostAsync(int id, int userId);
    }
} 