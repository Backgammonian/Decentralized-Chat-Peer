using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking.Utils;
using Extensions;

namespace UdpNatPunchClient.Models
{
    public sealed class ImageItem : ObservableObject
    {
        private const string _imagesFolderName = "LocalImages";
        private const int _defaultPreviewPictureWidth = 100;
        private const int _defaultPreviewPictureHeight = 100;

        static ImageItem()
        {
            CreateFolders();
        }

        private static void CreateFolders()
        {
            try
            {
                if (!Directory.Exists(_imagesFolderName))
                {
                    Directory.CreateDirectory(_imagesFolderName);
                }
            }
            catch (Exception) 
            {
            }
        }

        private string _picturePath = string.Empty;
        private string _previewPicturePath = string.Empty;
        private Uri? _pictureUri;
        private Uri? _previewPictureUri;
        private bool _isLoaded;

        public ImageItem(string path, int previewWidth, int previewHeight)
        {
            OriginalFilePath = path;
            FileName = RandomGenerator.GetRandomString(25);
            FileExtension = Path.GetExtension(OriginalFilePath).ToLower();
            PreviewPictureWidth = previewWidth <= 0 ? _defaultPreviewPictureWidth : previewWidth;
            PreviewPictureHeight = previewHeight <= 0 ? _defaultPreviewPictureHeight : previewHeight;
            IsLoaded = false;
        }

        public string OriginalFilePath { get; }
        public string FileName { get; }
        public string FileExtension { get; }
        public int PreviewPictureWidth { get; }
        public int PreviewPictureHeight { get; }
        public bool IsAnimation => FileExtension == ".gif";

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

        public Uri? PictureUri
        {
            get => _pictureUri;
            private set => SetProperty(ref _pictureUri, value);
        }

        public Uri? PreviewPictureUri
        {
            get => _previewPictureUri;
            private set => SetProperty(ref _previewPictureUri, value);
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }

        public static async Task<ImageItem?> SaveByteArrayAsImage(byte[] pictureBytes, string extension, int width, int height)
        {
            CreateFolders();

            if (width <= 0 ||
                height <= 0)
            {
                return null;
            }

            try
            {
                var newName = RandomGenerator.GetRandomString(25);
                var newFilePath = Path.GetFullPath(_imagesFolderName + "\\loaded_" + newName + extension);
                await File.WriteAllBytesAsync(newFilePath, pictureBytes);

                var imageItem = new ImageItem(newFilePath,
                    width,
                    height);

                return imageItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<byte[]?> GetPictureBytes()
        {
            if (IsLoaded &&
                PictureUri != null)
            {
                try
                {
                    return await File.ReadAllBytesAsync(PictureUri.OriginalString);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private bool TryCopyImage()
        {
            try
            {
                PicturePath = Path.GetFullPath(_imagesFolderName + "\\" + FileName + FileExtension);
                PreviewPicturePath = Path.GetFullPath(_imagesFolderName + "\\" + FileName + "_preview" + FileExtension);
                File.Copy(OriginalFilePath, PicturePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TryLoadImage()
        {
            CreateFolders();
            if (!TryCopyImage())
            {
                return false;
            }

            if (Constants.AllowedImageExtensions.Contains(FileExtension))
            {
                if (IsAnimation)
                {
                    return await TryLoadAsAnimation();
                }
                else
                {
                    return TryLoadAsPicture();
                }
            }
            else
            {
                return false;
            }
        }

        private bool TryLoadAsPicture()
        {
            try
            {
                using var pictureBitmap = new Bitmap(PicturePath);
                using var previewPictureBitmap = pictureBitmap.ResizeImageWithPreservedAspectRatio(PreviewPictureWidth, PreviewPictureHeight);

                switch (FileExtension)
                {
                    case ".jpg":
                    case ".jpeg":
                        previewPictureBitmap.Save(PreviewPicturePath, ImageFormat.Jpeg);
                        break;

                    case ".png":
                        previewPictureBitmap.Save(PreviewPicturePath, ImageFormat.Png);
                        break;

                    case ".bmp":
                        previewPictureBitmap.Save(PreviewPicturePath, ImageFormat.Bmp);
                        break;

                    case ".tiff":
                        previewPictureBitmap.Save(PreviewPicturePath, ImageFormat.Tiff);
                        break;
                }

                PreviewPictureUri = new Uri(PreviewPicturePath, UriKind.Absolute);
                PictureUri = new Uri(PicturePath, UriKind.Absolute);
                IsLoaded = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> TryLoadAsAnimation()
        {
            var frames = await GIFExtensions.TryGetResizedFramesFromGIFAsync(PicturePath,  PreviewPictureWidth, PreviewPictureHeight);
            if (frames == null)
            {
                return false;
            }

            if (await GIFExtensions.TryCreateGIFAsync(PreviewPicturePath, frames))
            {
                foreach (var frame in frames)
                {
                    frame.Image.Dispose();
                }

                PreviewPictureUri = new Uri(PreviewPicturePath, UriKind.Absolute);
                PictureUri = new Uri(PicturePath, UriKind.Absolute);
                IsLoaded = true;

                return true;
            }

            return false;
        }
    }
}