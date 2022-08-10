using System;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using LiteNetLib;
using LiteNetLib.Utils;
using Networking.Utils;
using Networking.Messages;

namespace Networking
{
    public class EncryptedPeer : ObservableObject
    {
        private readonly NetPeer _peer;
        private readonly CryptographyModule _cryptography;
        private ulong _incomingSegmentNumber;
        private ulong _outcomingSegmentNumber;

        private readonly DispatcherTimer _durationTimer;
        private TimeSpan _connectionDuration;
        private readonly DispatcherTimer _disconnectTimer;

        private const int _speedTimerInterval = 100;

        private long _oldAmountOfDownloadedBytes, _newAmountOfDownloadedBytes;
        private DateTime _oldDownloadTimeStamp, _newDownloadTimeStamp;
        private long _bytesDownloaded;
        private readonly DispatcherTimer _downloadSpeedCounter;
        private double _downloadSpeed;
        private readonly Queue<double> _downloadSpeedValues;

        private long _oldAmountOfUploadedBytes, _newAmountOfUploadedBytes;
        private DateTime _oldUploadTimeStamp, _newUploadTimeStamp;
        private long _bytesUploaded;
        private readonly DispatcherTimer _uploadSpeedCounter;
        private double _uploadSpeed;
        private readonly Queue<double> _uploadSpeedValues;

        private readonly DispatcherTimer _pingUpdateTimer;

        public EncryptedPeer(NetPeer peer)
        {
            _peer = peer;
            _cryptography = new CryptographyModule();
            _incomingSegmentNumber = RandomGenerator.GetRandomULong();
            _outcomingSegmentNumber = RandomGenerator.GetRandomULong();

            StartTime = DateTime.Now;

            _durationTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _durationTimer.Interval = new TimeSpan(0, 0, 1);
            _durationTimer.Tick += OnDurationTimerTick;
            _durationTimer.Start();

            _disconnectTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _disconnectTimer.Interval = new TimeSpan(0, 0, NetworkingConstants.DisconnectionTimeoutMilliseconds / 1000);
            _disconnectTimer.Tick += OnDisconnectTimerTick;
            _disconnectTimer.Start();

            _downloadSpeedValues = new Queue<double>();
            _downloadSpeedCounter = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _downloadSpeedCounter.Interval = new TimeSpan(0, 0, 0, 0, _speedTimerInterval);
            _downloadSpeedCounter.Tick += OnDownloadSpeedCounterTick;
            _downloadSpeedCounter.Start();

            _uploadSpeedValues = new Queue<double>();
            _uploadSpeedCounter = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _uploadSpeedCounter.Interval = new TimeSpan(0, 0, 0, 0, _speedTimerInterval);
            _uploadSpeedCounter.Tick += OnUploadSpeedCounterTick;
            _uploadSpeedCounter.Start();

            _pingUpdateTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _pingUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _pingUpdateTimer.Tick += OnPingUdpateTimerTick;
            _pingUpdateTimer.Start();
        }

        public event EventHandler<EncryptedPeerEventArgs>? PeerDisconnected;

        public bool IsSecurityEnabled => _cryptography.IsEnabled;
        public int Id => _peer.Id;
        public IPEndPoint EndPoint => _peer.EndPoint;
        public int Ping => _peer.Ping;
        public ConnectionState ConnectionState => _peer.ConnectionState;
        public DateTime StartTime { get; }

        public TimeSpan ConnectionDuration
        {
            get => _connectionDuration;
            private set => SetProperty(ref _connectionDuration, value);
        }

        public long BytesUploaded
        {
            get => _bytesUploaded;
            private set => SetProperty(ref _bytesUploaded, value);
        }

        public long BytesDownloaded
        {
            get => _bytesDownloaded;
            private set => SetProperty(ref _bytesDownloaded, value);
        }

        public double DownloadSpeed
        {
            get => _downloadSpeed;
            private set => SetProperty(ref _downloadSpeed, value);
        }

        public double UploadSpeed
        {
            get => _uploadSpeed;
            private set => SetProperty(ref _uploadSpeed, value);
        }

        private void OnDurationTimerTick(object? sender, EventArgs e)
        {
            ConnectionDuration = DateTime.Now - StartTime;
        }

        private void OnDisconnectTimerTick(object? sender, EventArgs e)
        {
            _disconnectTimer.Stop();

            if (!IsSecurityEnabled)
            {
                Disconnect();
            }
        }

        private void OnDownloadSpeedCounterTick(object? sender, EventArgs e)
        {
            _oldAmountOfDownloadedBytes = _newAmountOfDownloadedBytes;
            _newAmountOfDownloadedBytes = BytesDownloaded;

            _oldDownloadTimeStamp = _newDownloadTimeStamp;
            _newDownloadTimeStamp = DateTime.Now;

            var value = (_newAmountOfDownloadedBytes - _oldAmountOfDownloadedBytes) / (_newDownloadTimeStamp - _oldDownloadTimeStamp).TotalSeconds;
            _downloadSpeedValues.Enqueue(value);

            if (_downloadSpeedValues.Count > 20)
            {
                _downloadSpeedValues.Dequeue();
            }

            DownloadSpeed = _downloadSpeedValues.CalculateAverageValue();
        }

        private void OnUploadSpeedCounterTick(object? sender, EventArgs e)
        {
            _oldAmountOfUploadedBytes = _newAmountOfUploadedBytes;
            _newAmountOfUploadedBytes = BytesUploaded;

            _oldUploadTimeStamp = _newUploadTimeStamp;
            _newUploadTimeStamp = DateTime.Now;

            var value = (_newAmountOfUploadedBytes - _oldAmountOfUploadedBytes) / (_newUploadTimeStamp - _oldUploadTimeStamp).TotalSeconds;
            _uploadSpeedValues.Enqueue(value);

            if (_uploadSpeedValues.Count > 20)
            {
                _uploadSpeedValues.Dequeue();
            }

            UploadSpeed = _uploadSpeedValues.CalculateAverageValue();
        }

        private void OnPingUdpateTimerTick(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Ping));
        }

        public void SendPublicKeys()
        {
            var data = new NetDataWriter();

            var publicKey = _cryptography.PublicKey;
            data.Put(publicKey.Length);
            data.Put(publicKey);

            var signaturePublicKey = _cryptography.SignaturePublicKey;
            data.Put(signaturePublicKey.Length);
            data.Put(signaturePublicKey);

            data.Put(_incomingSegmentNumber);

            _peer.Send(data, 0, DeliveryMethod.ReliableOrdered);
        }

        public void ApplyKeys(byte[] publicKey, byte[] signaturePublicKey, ulong recepientsIncomingSegmentNumber)
        {
            if (_cryptography.TrySetKeys(publicKey, signaturePublicKey))
            {
                _outcomingSegmentNumber = recepientsIncomingSegmentNumber;

                OnPropertyChanged(nameof(IsSecurityEnabled));
            }
            else
            {
                Disconnect();
            }
        }

        public void SendEncrypted(BaseMessage message)
        {
            SendEncrypted(message, RandomGenerator.GetPseudoRandomByte(0, NetworkingConstants.ChannelsCount));
        }

        public void SendEncrypted(BaseMessage message, byte channelNumber)
        {
            var messageContent = message.GetContent();
            messageContent.Put(_outcomingSegmentNumber);

            SendEncrypted(messageContent.Data, channelNumber);
        }

        private void SendEncrypted(byte[] message, byte channelNumber)
        {
            if (!IsSecurityEnabled)
            {
                return;
            }

            if (_cryptography.TryCreateSignature(message, out byte[] signature) &&
                message.TryCompressByteArray(out byte[] compressedMessage) &&
                _cryptography.TryEncrypt(compressedMessage, out byte[] encryptedMessage, out byte[] iv))
            {
                var outcomingMessage = new NetDataWriter();
                outcomingMessage.PutBytesWithLength(encryptedMessage);
                outcomingMessage.PutBytesWithLength(signature);
                outcomingMessage.PutBytesWithLength(iv);
                outcomingMessage.Put(CRC32.Compute(encryptedMessage));

                _peer.Send(outcomingMessage.Data, channelNumber, DeliveryMethod.ReliableOrdered);

                BytesUploaded += outcomingMessage.Data.Length;

                _outcomingSegmentNumber += 1;
                _outcomingSegmentNumber = _outcomingSegmentNumber == ulong.MaxValue ? 0 : _outcomingSegmentNumber;
            }
        }

        public bool TryDecryptReceivedData(NetPacketReader incomingDataReader, out NetworkMessageType messageType, out string outputJson)
        {
            messageType = NetworkMessageType.Empty;
            outputJson = string.Empty;

            if (!IsSecurityEnabled)
            {
                return false;
            }

            if (incomingDataReader.TryGetBytesWithLength(out byte[] encryptedMessage) &&
                incomingDataReader.TryGetBytesWithLength(out byte[] signature) &&
                incomingDataReader.TryGetBytesWithLength(out byte[] iv) &&
                incomingDataReader.TryGetUInt(out uint receivedCrc32) &&
                CRC32.Compute(encryptedMessage) == receivedCrc32 &&
                _cryptography.TryDecrypt(encryptedMessage, iv, out byte[] decryptedMessage) &&
                decryptedMessage.TryDecompressByteArray(out byte[] decompressedMessage) &&
                _cryptography.TryVerifySignature(decompressedMessage, signature))
            {
                var messageReader = new NetDataReader(decompressedMessage);

                if (messageReader.TryGetByte(out byte type) &&
                    Enum.TryParse(typeof(NetworkMessageType), type + "", out object? networkMessageType) &&
                    networkMessageType != null &&
                    messageReader.TryGetString(out string json) &&
                    messageReader.TryGetULong(out ulong recepientsOutcomingSegmentNumber) &&
                    recepientsOutcomingSegmentNumber == _incomingSegmentNumber)
                {
                    _incomingSegmentNumber += 1;

                    _incomingSegmentNumber = _incomingSegmentNumber == ulong.MaxValue ? 0 : _incomingSegmentNumber;

                    BytesDownloaded += incomingDataReader.RawDataSize;

                    messageType = (NetworkMessageType)networkMessageType;
                    outputJson = json;

                    return true;
                }
            }

            return false;
        }

        public void Disconnect()
        {
            var id = _peer.Id;
            _peer.Disconnect();

            _durationTimer.Stop();
            _disconnectTimer.Stop();
            _downloadSpeedCounter.Stop();
            _uploadSpeedCounter.Stop();
            _pingUpdateTimer.Stop();

            PeerDisconnected?.Invoke(this, new EncryptedPeerEventArgs(this));
        }
    }
}
