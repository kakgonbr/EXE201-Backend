using System.IO;

namespace EXE201_Backend.Services
{
    public class ImageService : IImageService
    {
        private static readonly HashSet<ImageUploadTracker> _imageTrackers = new();
        private readonly ILogger<ImageService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly ITimeProvider _timeProvider;

        public ImageService(ILogger<ImageService> logger, IConfigurationService configurationService, ITimeProvider timeProvider)
        {
            _logger = logger;
            _configurationService = configurationService;
            _timeProvider = timeProvider;

            ImageUploadTracker.Initialize(timeProvider, _configurationService.IMAGE_EXPIRE_SEC);
        }

        private class ImageUploadTracker
        {
            public DateTime UploadTime { get; init; }
            public bool IsExpired => UploadTime.AddSeconds(_expireSec) < (_timeProvider?.Now ?? DateTime.Now);
            public int CustomerId { get; init; }
            public string ImageName { get; init; } = null!;

            private static ITimeProvider? _timeProvider;
            private static int _expireSec;

            public ImageUploadTracker()
            {
                UploadTime = _timeProvider?.Now ?? DateTime.Now;
            }

            public static void Initialize(ITimeProvider timeProvider, int expireInSecs)
            {
                _timeProvider = timeProvider;
                _expireSec = expireInSecs;
            }

            public override bool Equals(object? obj)
            {
                if (obj is ImageUploadTracker other)
                    return CustomerId == other.CustomerId;

                return false;
            }

            public override int GetHashCode()
            {
                return CustomerId;
            }

            public override string ToString()
            {
                return $"Tracker: CID : {CustomerId}, IMG: {ImageName}";
            }
        }

        public void CleanupPending()
        {
            foreach (var tracker in _imageTrackers)
            {
                if (tracker.IsExpired)
                {
                    _logger.LogInformation("Clearing tracker for {CustomerId}", tracker.CustomerId);

                    DeleteImage(tracker.ImageName);

                    _imageTrackers.Remove(tracker);
                }
            }
        }

        public void DeleteImageFile(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return;
            }

            var fileName = Path.GetFileName(imageName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            DeleteImage(fileName);
        }

        private void DeleteImage(string name)
        {
            _logger.LogInformation("Deleting {Name}", name);

            string fullPath = Path.Combine(_configurationService.IMAGE_DIR, name);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);

                    _logger.LogInformation("Deleted {Path}.", fullPath);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Image deletion failed.");
                }
            }
            else
            {
                _logger.LogInformation("{Name} not found.", name);
            }
        }

        public void CheckImagePresent(string imageName, int userId)
        {
            CleanupPending();

            if (_imageTrackers.Contains(new() { CustomerId = userId }))
            {
                ImageUploadTracker? tracker = _imageTrackers.FirstOrDefault(t => t.CustomerId == userId);

                if (tracker is not null)
                {
                    DeleteImage(tracker.ImageName);
                }
            }

            ImageUploadTracker newTracker = new() { CustomerId = userId, ImageName = imageName };

            _imageTrackers.Remove(newTracker);
            _imageTrackers.Add(newTracker);
        }

        public async Task<string> Upload(IFormFile image, CancellationToken cancellationToken = default)
        {
            if (image == null || image.Length == 0)
            {
                return "Failed: No file uploaded.";
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
            {
                return "Failed: Only JPG/PNG files are allowed.";
            }

            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string randomPart = new Random().Next(1000, 9999).ToString();
            string uniqueName = $"{timeStamp}_{randomPart}{extension}";
            string directory = _configurationService.IMAGE_DIR;

            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream, cancellationToken);
            }

            return uniqueName;
        }

        /// <summary>
        /// MUST BE CALLED TO PERSISTENTLY SAVE CUSTOMER UPLOADED IMAGE.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>true if the image was consumed, false if the image was not present</returns>
        public bool ConsumeImage(int userId)
        {
            return _imageTrackers.Remove(new() { CustomerId = userId });
        }
    }
}
