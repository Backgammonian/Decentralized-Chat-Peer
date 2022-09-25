using System;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking;
using Networking.Utils;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public sealed class Upload : ObservableObject
    {
        private readonly SharedFile _sharedFile;
        private readonly SpeedCounter _uploadSpeedCounter;
        private long _numberOfAckedSegments;
        private bool _isFinished;
        private bool _isCancelled;
        private bool _isStarted;
        private DateTime _finishTime;

        public Upload(string id, SharedFile sharedFile, EncryptedPeer destination)
        {
            _sharedFile = sharedFile;
            _sharedFile.Closed += OnSharedFileClosed;
            _uploadSpeedCounter = new SpeedCounter();
            _uploadSpeedCounter.Updated += OnUploadSpeedCounterUpdated;
            ID = id;
            Destination = destination;
            NumberOfAckedSegments = 0;
            IsFinished = false;
            IsCancelled = false;
            IsStarted = false;
            StartTime = DateTime.Now;
        }

        public string ID { get; }
        public EncryptedPeer Destination { get; }
        public DateTime StartTime { get; }
        public string FileName => _sharedFile.Name;
        public long FileSize => _sharedFile.Size;
        public string FileHash => _sharedFile.Hash;
        public long NumberOfSegments => _sharedFile.NumberOfSegments;
        public bool IsActive => !IsCancelled && !IsFinished && IsStarted;
        public decimal Progress => NumberOfAckedSegments / Convert.ToDecimal(NumberOfSegments);
        public double UploadSpeed => _uploadSpeedCounter.Speed;
        public double AverageSpeed => _uploadSpeedCounter.AverageSpeed;
        public long BytesUploaded => _uploadSpeedCounter.Bytes;

        public long NumberOfAckedSegments
        {
            get => _numberOfAckedSegments;
            private set
            {
                SetProperty(ref _numberOfAckedSegments, value);
                OnPropertyChanged(nameof(Progress));

                if (NumberOfSegments == NumberOfAckedSegments)
                {
                    Finish();
                }
            }
        }

        public bool IsFinished
        {
            get => _isFinished;
            private set => SetProperty(ref _isFinished, value);
        }

        public bool IsCancelled
        {
            get => _isCancelled;
            private set => SetProperty(ref _isCancelled, value);
        }

        public DateTime FinishTime
        {
            get => _finishTime;
            private set => SetProperty(ref _finishTime, value);
        }

        public bool IsStarted
        {
            get => _isStarted;
            private set => SetProperty(ref _isStarted, value);
        }

        private void OnUploadSpeedCounterUpdated(object? sender, EventArgs e)
        {
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            OnPropertyChanged(nameof(UploadSpeed));
            OnPropertyChanged(nameof(AverageSpeed));
            OnPropertyChanged(nameof(BytesUploaded));
        }

        private void OnSharedFileClosed(object? sender, EventArgs e)
        {
            Cancel();
            _sharedFile.Closed -= OnSharedFileClosed;
        }

        private void Finish()
        {
            IsFinished = true;
            FinishTime = DateTime.Now;
        }

        public bool AddAck()
        {
            if (!IsActive)
            {
                Debug.WriteLine($"(Upload_AddAck) Can't receive ACK for file {FileName}, upload {ID}");

                return false;
            }

            NumberOfAckedSegments += 1;

            return true;
        }

        public void Cancel()
        {
            if (IsFinished ||
                IsCancelled)
            {
                return;
            }

            IsCancelled = true;
            SendCancellationMessage();
        }

        private void SendCancellationMessage()
        {
            var cancellationMessage = new UploadCancellationMessage(ID);
            Destination.SendEncrypted(cancellationMessage, 0);
        }

        public void StartUpload()
        {
            IsStarted = true;
            SendSegmentInternal(0);
        }

        public void SendNextFileSegment()
        {
            SendSegmentInternal(NumberOfAckedSegments);
        }

        private void SendSegmentInternal(long numberOfSegment)
        {
            if (!_sharedFile.IsActive)
            {
                return;
            }

            var segment = _sharedFile.TryReadSegment(numberOfSegment);
            if (segment.Length == 0)
            {
                return;
            }

            _uploadSpeedCounter.AddBytes(segment.Length);
            var fileSegmentMessage = new FileSegmentMessage(ID, segment);
            Destination.SendEncrypted(fileSegmentMessage, 1);
        }
    }
}