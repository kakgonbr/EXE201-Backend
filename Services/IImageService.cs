
namespace EXE201_Backend.Services
{
    public interface IImageService
    {
        void CheckImagePresent(string imageName, int userId);
        void CleanupPending();
        bool ConsumeImage(int userId);
        void DeleteImageFile(string imageName);
        Task<string> Upload(IFormFile image, CancellationToken cancellationToken = default);
    }
}