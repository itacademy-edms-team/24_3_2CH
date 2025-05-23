using System.Threading.Tasks;

namespace MyImageBoard.Services.Interfaces
{
    public interface IThreadService
    {
        Task DeleteThreadAsync(int id, int userId);
    }
} 