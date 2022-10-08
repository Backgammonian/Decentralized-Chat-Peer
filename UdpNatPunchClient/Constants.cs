using System;
using System.Collections.Immutable;

namespace UdpNatPunchClient
{
    public static class Constants
    {
        public static int MaxNicknameLength { get; } = 150;
        public static int MaxMessageLength { get; } = 3000;
        public static string ImageFilter { get; } = "Pictures|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif";
        public static string AllFilesFilter { get; } = "All files (*.*)|*.*";
        public static int MaxProfilePictureSize { get; } = 10_485_760; //10 MB
        public static int MaxPictureSize { get; } = 31_457_280; //30 MB
        public static int FileSegmentSize { get; } = Convert.ToInt32(Math.Pow(2, 17)); //128 KB
        public static (int, int) ProfilePictureThumbnailSize { get; } = (200, 200);
        public static (int, int) ImageMessageThumbnailSize { get; } = (250, 250);
        public static ImmutableHashSet<string> AllowedImageExtensions { get; } =
            ImmutableHashSet.Create(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" });
    }
}
