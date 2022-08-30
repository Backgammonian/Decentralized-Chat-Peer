using System.Collections.Immutable;

namespace UdpNatPunchClient
{
    public static class Constants
    {
        public static int MaxNicknameLength { get; } = 150;
        public static int MaxMessageLength { get; } = 3000;
        public static ImmutableHashSet<string> AllowedImageExtensions { get; }
            = ImmutableHashSet.Create(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" });
        public static string ImageFilter { get; } = "Pictures|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif";
        public static long MaxProfilePictureSize { get; } = 5242880; //5 MB
        public static long MaxPictureSize { get; } = 20971520; //20 MB
        public static (int, int) ProfilePictureThumbnailSize { get; } = (200, 200);
        public static (int, int) ImageMessageThumbnailSize { get; } = (300, 300);
        
    }
}
