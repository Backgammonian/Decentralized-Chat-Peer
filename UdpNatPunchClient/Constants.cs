namespace UdpNatPunchClient
{
    public static class Constants
    {
        public const int MaxNicknameLength = 150;
        public const int MaxMessageLength = 3000;
        public const string ImageFilter = "Pictures|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif";
        public const long MaxProfilePictureSize = 5242880; //5 MB
        public const long MaxPictureSize = 52428800; //50 MB
        public static readonly (int, int) ProfilePictureThumbnailSize = (400, 400);
        public static readonly (int, int) ImageMessageThumbnailSize = (200, 200);
    }
}
