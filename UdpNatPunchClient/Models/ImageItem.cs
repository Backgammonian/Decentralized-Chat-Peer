using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking.Utils;
using Extensions;

namespace UdpNatPunchClient.Models
{
    public class ImageItem : ObservableObject
    {
        public const string ImagesFolderName = "LocalImages";
        public const string LoadedImagesFolderName = "LoadedImages";

        private string _picturePath = string.Empty;
        private string _previewPicturePath = string.Empty;
        private FileStream? _pictureStream;
        private FileStream? _previewPictureStream;
        private bool _isLoaded;

        static ImageItem()
        {
            CreateFolders();
        }

        public ImageItem(string path, int previewWidth, int previewHeight)
        {
            OriginalFilePath = path;
            FileName = RandomGenerator.GetRandomString(25);
            FileExtension = Path.GetExtension(OriginalFilePath).ToLower();
            PreviewPictureWidth = previewWidth <= 0 ? Constants.ProfilePictureThumbnailSize.Item1 : previewWidth;
            PreviewPictureHeight = previewHeight <= 0 ? Constants.ProfilePictureThumbnailSize.Item2 : previewHeight;
            IsLoaded = false;
        }

        public string OriginalFilePath { get; }
        public string FileName { get; }
        public string FileExtension { get; }
        public int PreviewPictureWidth { get; }
        public int PreviewPictureHeight { get; }
        public bool IsAnimation => FileExtension == ".gif";
        public long Length => _pictureStream != null ? _pictureStream.Length : 0;

        public string PicturePath
        {
            get => _picturePath;
            private set => SetProperty(ref _picturePath, value);
        }

        public string PreviewPicturePath
        {
            get => _previewPicturePath;
            private set => SetProperty(ref _previewPicturePath, value);
        }

        public FileStream? PictureStream
        {
            get => _pictureStream;
            private set => SetProperty(ref _pictureStream, value);
        }

        public FileStream? PreviewPictureStream
        {
            get => _previewPictureStream;
            private set => SetProperty(ref _previewPictureStream, value);
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }

        private static void CreateFolders()
        {
            try
            {
                if (!Directory.Exists(ImagesFolderName))
                {
                    Directory.CreateDirectory(ImagesFolderName);
                }

                if (!Directory.Exists(LoadedImagesFolderName))
                {
                    Directory.CreateDirectory(LoadedImagesFolderName);
                }
            }
            catch (Exception)
            {
            }
        }

        public static ImageItem? TrySaveByteArrayAsImage(byte[] pictureBytes, string extension)
        {
            CreateFolders();

            try
            {
                var newName = RandomGenerator.GetRandomString(25);
                var newFilePath = Path.GetFullPath(LoadedImagesFolderName + "\\" + newName + extension);
                File.WriteAllBytes(newFilePath, pictureBytes);

                var imageItem = new ImageItem(
                    newFilePath,
                    Constants.ProfilePictureThumbnailSize.Item1,
                    Constants.ProfilePictureThumbnailSize.Item2);

                return imageItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<ImageItem?> TrySaveByteArrayAsImageAsync(byte[] pictureBytes, string extension)
        {
            CreateFolders();

            try
            {
                var newName = RandomGenerator.GetRandomString(25);
                var newFilePath = Path.GetFullPath(LoadedImagesFolderName + "\\" + newName + extension);
                await File.WriteAllBytesAsync(newFilePath, pictureBytes);

                var imageItem = new ImageItem(
                    newFilePath,
                    Constants.ProfilePictureThumbnailSize.Item1,
                    Constants.ProfilePictureThumbnailSize.Item2);

                return imageItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool, byte[])> TryGetPictureBytes()
        {
            try
            {
                if (_pictureStream != null)
                {
                    var bytes = await File.ReadAllBytesAsync(_pictureStream.Name);

                    return (true, bytes);
                }
                else
                {
                    return (false, Array.Empty<byte>());
                }
                
            }
            catch (Exception)
            {
                return (false, Array.Empty<byte>());
            }
        }

        private bool TryCopyImage()
        {
            try
            {
                PicturePath = Path.GetFullPath(ImagesFolderName + "\\" + FileName + FileExtension);
                PreviewPicturePath = Path.GetFullPath(ImagesFolderName + "\\" + FileName + "_preview" + FileExtension);
                File.Copy(OriginalFilePath, PicturePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryLoadImage()
        {
            CreateFolders();
            if (!TryCopyImage())
            {
                return false;
            }

            switch (FileExtension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".tiff":
                case ".gif":
                    break;

                default:
                    return false;
            }

            if (IsAnimation)
            {
                return TryLoadAsAnimation();
            }
            else
            {
                return TryLoadAsPicture();
            }
        }

        public async Task<bool> TryLoadImageAsync()
        {
            CreateFolders();
            if (!TryCopyImage())
            {
                return false;
            }

            switch (FileExtension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".tiff":
                case ".gif":
                    break;

                default:
                    return false;
            }

            if (IsAnimation)
            {
                return await TryLoadAsAnimationAsync();
            }
            else
            {
                return TryLoadAsPicture();
            }
        }

        private bool TryLoadAsPicture()
        {
            try
            {
                var stream = File.Open(PicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var previewBitmap = new Bitmap(stream).ResizeImageWithPreservedAspectRatio(PreviewPictureWidth, PreviewPictureHeight);

                switch (FileExtension)
                {
                    case ".jpg":
                    case ".jpeg":
                        previewBitmap.Save(PreviewPicturePath, ImageFormat.Jpeg);
                        break;

                    case ".png":
                        previewBitmap.Save(PreviewPicturePath, ImageFormat.Png);
                        break;

                    case ".bmp":
                        previewBitmap.Save(PreviewPicturePath, ImageFormat.Bmp);
                        break;

                    case ".tiff":
                        previewBitmap.Save(PreviewPicturePath, ImageFormat.Tiff);
                        break;
                }

                PreviewPictureStream = File.Open(PreviewPicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                PictureStream = stream;
                IsLoaded = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryLoadAsAnimation()
        {
            try
            {
                var stream = File.Open(PicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var result = GIFExtensions.TryGetResizedFramesFromGIF(stream, PreviewPictureWidth, PreviewPictureHeight);

                if (result.Item1 &&
                    GIFExtensions.TryCreateGIF(PreviewPicturePath, result.Item2))
                {
                    PreviewPictureStream = File.Open(PreviewPicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    PictureStream = stream;

                    foreach (var frame in result.Item2)
                    {
                        frame.Image.Dispose();
                    }

                    IsLoaded = true;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> TryLoadAsAnimationAsync()
        {
            try
            {
                var stream = File.Open(PicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var result = await GIFExtensions.TryGetResizedFramesFromGIFAsync(stream,  PreviewPictureWidth, PreviewPictureHeight);

                if (result.Item1 &&
                    await GIFExtensions.TryCreateGIFAsync(PreviewPicturePath, result.Item2))
                {
                    PreviewPictureStream = File.Open(PreviewPicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    PictureStream = stream;

                    foreach (var frame in result.Item2)
                    {
                        frame.Image.Dispose();
                    }

                    IsLoaded = true;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(bool, List<Frame>)> TryLoadAsAnimationAndGetFrames()
        {
            CreateFolders();
            if (!TryCopyImage())
            {
                return (false, new List<Frame>());
            }

            if (!IsAnimation)
            {
                return (false, new List<Frame>());
            }

            try
            {
                var stream = File.Open(PicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var result = await GIFExtensions.TryGetResizedFramesFromGIFAsync(stream, PreviewPictureWidth, PreviewPictureHeight);

                if (result.Item1 &&
                    await GIFExtensions.TryCreateGIFAsync(PreviewPicturePath, result.Item2))
                {
                    PreviewPictureStream = File.Open(PreviewPicturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    PictureStream = stream;
                    IsLoaded = true;

                    return (true, result.Item2);
                }
                else
                {
                    return (false, new List<Frame>());
                }
            }
            catch (Exception)
            {
                return (false, new List<Frame>());
            }
        }
    }
}