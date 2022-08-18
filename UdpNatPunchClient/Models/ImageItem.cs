﻿using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Extensions;
using Networking.Utils;

namespace UdpNatPunchClient.Models
{
    public class ImageItem : ObservableObject
    {
        private const string _imageFolderName = "Images";

        private BitmapImage? _preview;

        public ImageItem(string path, int previewWidth, int previewHeight)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(Path);
            FileExtension = System.IO.Path.GetExtension(Path).ToUpper();

            if (BitmapImageExtensions.TryLoadBitmapImageFromPath(Path, previewWidth, previewHeight, out var image))
            {
                Preview = image;
            }
        }

        public ImageItem(byte[] array, string extension, int previewWidth, int previewHeight)
        {
            FileExtension = extension.ToUpper();
            FileName = RandomGenerator.GetRandomString(20) + extension;
            var newImagePath = _imageFolderName + "\\" + FileName;
            Path = System.IO.Path.GetFullPath(newImagePath);
            File.WriteAllBytes(Path, array);

            if (BitmapImageExtensions.TryLoadBitmapImageFromPath(Path, previewWidth, previewHeight, out var image))
            {
                Preview = image;
            }
        }

        public string Path { get; }
        public string FileName { get; }
        public string FileExtension { get; }
        public bool IsAnimation => FileExtension == ".GIF";

        public BitmapImage? Preview
        {
            get => _preview;
            private set => SetProperty(ref _preview, value);
        }

        public async Task<byte[]> GetBytes()
        {
            try
            {
                return await File.ReadAllBytesAsync(Path);
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }
    }
}
