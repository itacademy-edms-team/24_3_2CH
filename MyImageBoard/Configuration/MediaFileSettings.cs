namespace ForumProject.Configuration
{
    public static class MediaFileSettings
    {
        public static readonly string[] AllowedFileTypes = new[] { ".jpeg", ".jpg", ".png", ".gif", ".mp4", ".webm" };
        public static readonly string[] AllowedMimeTypes = new[] 
        { 
            "image/jpeg", 
            "image/png", 
            "image/gif", 
            "video/mp4", 
            "video/webm" 
        };
        
        public const int MaxFilesPerThread = 4;
        public const long MaxTotalSizeBytes = 52428800; // 50 MB in bytes
        
        public const string UploadDirectory = "uploads/threads";
        
        // Путь для сохранения файлов конкретного треда
        public static string GetThreadUploadPath(int threadId) => $"{UploadDirectory}/{threadId}".Replace("\\", "/");
    }
} 