
namespace EXE201_Backend.Services
{
    public interface IImageService
    {
        void CheckImagePresent(string imageName, int userId);
        void CleanupPending();
        bool ConsumeImage(int userId);
        Task<string> Upload(IFormFile image);
    }
}