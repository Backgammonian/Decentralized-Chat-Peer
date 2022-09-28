﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking;
using Networking.Utils;

namespace UdpNatPunchClient.Models
{
    public sealed class Download : ObservableObject
    {
        private readonly SpeedCounter _downloadSpeedCounter;
        private FileStream? _stream;
        private bool _isDownloaded;
        private bool _isCancelled;
        private HashVerificationStatus _hashVerificationStatus;
        private string _calculatedHash = CryptographyModule.DefaultFileHash;
        private long _numberOfReceivedSegments;

        public Download(AvailableFile availableFile, string path)
        {
            _downloadSpeedCounter = new SpeedCounter();
            _downloadSpeedCounter.Updated += OnDownloadSpeedCounterUpdated;
            DownloadID = RandomGenerator.GetRandomString(30);
            SourceInfo = availableFile;
            Name = Path.GetFileName(path);
            FilePath = path;
            IsDownloaded = false;
            IsCancelled = false;
            HashVerificationStatus = HashVerificationStatus.None;
            StartTime = DateTime.Now;
            CalculatedHash = CryptographyModule.DefaultFileHash;
            NumberOfReceivedSegments = 0;
        }

        public event EventHandler<EventArgs>? FileRemoved;
        public event EventHandler<DownloadFinishedEventArgs>? Finished;

        public string DownloadID { get; }
        public AvailableFile SourceInfo { get; }
        public string Name { get; }
        public string FilePath { get; }
        public DateTime StartTime { get; }
        public string SharedFileID => SourceInfo.FileIDFromServer;
        public UserModel Server => SourceInfo.Server;
        public string OriginalName => SourceInfo.Name;
        public long Size => SourceInfo.Size;
        public long NumberOfSegments => SourceInfo.NumberOfSegments;
        public string Hash => SourceInfo.Hash;
        public bool IsActive => !IsCancelled && !IsDownloaded;
        public double DownloadSpeed => _downloadSpeedCounter.Speed;
        public double AverageSpeed => _downloadSpeedCounter.AverageSpeed;
        public long BytesDownloaded => _downloadSpeedCounter.Bytes;
        public decimal Progress => NumberOfReceivedSegments / Convert.ToDecimal(NumberOfSegments);

        public long NumberOfReceivedSegments
        {
            get => _numberOfReceivedSegments;
            private set
            {
                SetProperty(ref _numberOfReceivedSegments, value);
                UpdateParameters();

                if (NumberOfReceivedSegments == NumberOfSegments)
                {
                    FinishDownload();
                }
            }
        }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            private set => SetProperty(ref _isDownloaded, value);
        }

        public bool IsCancelled
        {
            get => _isCancelled;
            private set => SetProperty(ref _isCancelled, value);
        }

        public HashVerificationStatus HashVerificationStatus
        {
            get => _hashVerificationStatus;
            private set => SetProperty(ref _hashVerificationStatus, value);
        }

        public string CalculatedHash
        {
            get => _calculatedHash;
            private set => SetProperty(ref _calculatedHash, value);
        }

        private void OnDownloadSpeedCounterUpdated(object? sender, EventArgs e)
        {
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(AverageSpeed));
            OnPropertyChanged(nameof(BytesDownloaded));
            OnPropertyChanged(nameof(Progress));
        }

        private bool AddReceivedBytes(byte[] segment)
        {
            if (_stream == null)
            {
                return false;
            }

            try
            {
                _ = _stream.Seek(NumberOfReceivedSegments * Constants.FileSegmentSize, SeekOrigin.Begin);
                _stream.Write(segment);
                _downloadSpeedCounter.AddBytes(segment.Length);
                NumberOfReceivedSegments += 1;

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private void FinishDownload()
        {
            Finished?.Invoke(this, new DownloadFinishedEventArgs(Name));
            IsDownloaded = true;
            ShutdownFile();

            Debug.WriteLine($"(FinishDownload) File '{Name}' - bytes downloaded: {BytesDownloaded} of {Size}, " +
                $"segments received: {NumberOfReceivedSegments} of {NumberOfSegments}");

            var verifyHashTask = new Task(() => VerifyHash());
            verifyHashTask.Start();
        }

        private void VerifyHash()
        {
            if (HashVerificationStatus != HashVerificationStatus.None ||
                !IsDownloaded)
            {
                return;
            }

            HashVerificationStatus = HashVerificationStatus.Started;
            CalculatedHash = CryptographyModule.ComputeFileHash(FilePath);

            if (CalculatedHash == CryptographyModule.DefaultFileHash)
            {
                HashVerificationStatus = HashVerificationStatus.Failed;
            }
            else
            if (CalculatedHash == Hash)
            {
                HashVerificationStatus = HashVerificationStatus.Positive;
            }
            else
            {
                HashVerificationStatus = HashVerificationStatus.Negative;
            }
        }

        public bool TryOpenFile()
        {
            try
            {
                _stream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, Constants.FileSegmentSize);

                return true;
            }
            catch (Exception)
            {
                _downloadSpeedCounter.Stop();

                return false;
            }
        }

        public void ShutdownFile()
        {
            _downloadSpeedCounter.Stop();

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }

            UpdateParameters();
        }

        public void Write(byte[] segment)
        {
            if (!IsActive)
            {
                Debug.WriteLine($"(DownloadFile_TryWrite) Download of file {Name} is not active!");

                return;
            }

            if (AddReceivedBytes(segment))
            {
                Server.SendFileSegmentAckMessage(DownloadID);
            }
            else
            {
                Cancel();
            }
        }

        public void CancelWithDeletion()
        {
            Cancel();

            try
            {
                File.Delete(FilePath);
                FileRemoved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
            }
        }

        public void Cancel()
        {
            if (!IsActive)
            {
                Debug.WriteLine($"(DownloadFile_SendFileDenial) File {Name} is already downloaded/cancelled!");

                return;
            }

            IsCancelled = true;
            ShutdownFile();
            Server.SendDownloadCancellationMessage(DownloadID);
        }

        public void SendFileRequest()
        {
            Server.SendFileRequest(this);
        }
    }
}