using System;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking;

namespace UdpNatPunchClient.Models
{
    public sealed class SharedFile : ObservableObject
    {
        private FileStream? _stream;
        private string _hash = CryptographyModule.DefaultFileHash;
        private string _name = string.Empty;
        private long _size;
        private long _numberOfSegments;
        private bool _isActive;

        public SharedFile(long index, string path)
        {
            Index = index;
            FilePath = path;
            IsActive = false;
        }

        public event EventHandler<EventArgs>? Closed;

        public long Index { get; }
        public string FilePath { get; }
        public bool IsHashCalculated => Hash != CryptographyModule.DefaultFileHash;

        public string Name
        {
            get => _name;
            private set => SetProperty(ref _name, value);
        }

        public long Size
        {
            get => _size;
            private set => SetProperty(ref _size, value);
        }

        public long NumberOfSegments
        {
            get => _numberOfSegments;
            private set => SetProperty(ref _numberOfSegments, value);
        }

        public string Hash
        {
            get => _hash;
            private set
            {
                SetProperty(ref _hash, value);
                OnPropertyChanged(nameof(IsHashCalculated));
            }
        }

        public bool IsActive
        {
            get => _isActive;
            private set => SetProperty(ref _isActive, value);
        }

        public bool TryOpenStream()
        {
            try
            {
                _stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.FileSegmentSize);
                Name = Path.GetFileName(FilePath);
                Size = _stream.Length;
                NumberOfSegments = Size / Constants.FileSegmentSize + (Size % Constants.FileSegmentSize != 0 ? 1 : 0);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CloseStream()
        {
            if (_stream != null)
            {
                _stream.Close();
            }

            IsActive = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public bool TryComputeFileHash()
        {
            if (IsHashCalculated)
            {
                return true;
            }

            var fileHash = CryptographyModule.ComputeFileHash(FilePath);
            if (fileHash == CryptographyModule.DefaultFileHash)
            {
                return false;
            }

            Hash = fileHash;
            IsActive = true;

            return true;
        }

        public byte[] TryReadSegment(long numberOfSegment)
        {
            if (_stream == null)
            {
                return Array.Empty<byte>();
            }

            try
            {
                var buffer = new byte[Constants.FileSegmentSize];
                _stream.Seek(numberOfSegment * Constants.FileSegmentSize, SeekOrigin.Begin);
                var readBytes = _stream.Read(buffer);

                if (readBytes == buffer.Length)
                {
                    return buffer;
                }
                else
                {
                    var specialBuffer = new byte[readBytes];
                    Buffer.BlockCopy(buffer, 0, specialBuffer, 0, readBytes);

                    return specialBuffer;
                }
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }
    }
}