using System;
using System.Windows;
using System.Windows.Threading;
using System.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using LiteNetLib;
using LiteNetLib.Utils;
using Networking.Messages;
using Networking.Utils;

namespace Networking
{
    public sealed class EncryptedPeer : ObservableObject
    {
        private readonly NetPeer _peer;
        private readonly CryptographyModule _cryptography;
        private readonly SpeedCounter _downloadSpeedCounter;
        private readonly SpeedCounter _uploadSpeedCounter;
        private readonly DispatcherTimer _disconnectTimer;
        private readonly DispatcherTimer _pingUpdateTimer;
        private readonly DispatcherTimer _durationTimer;
        private TimeSpan _connectionDuration;
        private ulong _incomingSegmentNumber;
        private ulong _outcomingSegmentNumber;

        public EncryptedPeer(NetPeer peer)
        {
            _peer = peer;
            _cryptography = new CryptographyModule();
            _incomingSegmentNumber = RandomGenerator.GetRandomULong();
            _incomingSegmentNumber = _incomingSegmentNumber != 0 ? _incomingSegmentNumber - 1 : _incomingSegmentNumber;
            _outcomingSegmentNumber = RandomGenerator.GetRandomULong();
            _outcomingSegmentNumber = _outcomingSegmentNumber != 0 ? _outcomingSegmentNumber - 1 : _outcomingSegmentNumber;

            _durationTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _durationTimer.Interval = TimeSpan.FromSeconds(1);
            _durationTimer.Tick += OnDurationTimerTick;
            _durationTimer.Start();

            _disconnectTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _disconnectTimer.Interval = TimeSpan.FromMilliseconds(NetworkingConstants.DisconnectionTimeoutMilliseconds);
            _disconnectTimer.Tick += OnDisconnectTimerTick;
            _disconnectTimer.Start();

            _pingUpdateTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _pingUpdateTimer.Interval = TimeSpan.FromMilliseconds(200);
            _pingUpdateTimer.Tick += OnPingUdpateTimerTick;
            _pingUpdateTimer.Start();

            _downloadSpeedCounter = new SpeedCounter();
            _downloadSpeedCounter.Updated += OnDownloadSpeedCounterUpdated;
            _uploadSpeedCounter = new SpeedCounter();
            _uploadSpeedCounter.Updated += OnUploadSpeedCounterUpdated;

            StartTime = DateTime.Now;
        }

        public event EventHandler<EncryptedPeerEventArgs>? PeerDisconnected;

        public DateTime StartTime { get; }
        public bool IsSecurityEnabled => _cryptography.IsEnabled;
        public int Id => _peer.Id;
        public IPEndPoint EndPoint => _peer.EndPoint;
        public int Ping => _peer.Ping;
        public ConnectionState ConnectionState => _peer.ConnectionState;
        public double DownloadSpeed => _downloadSpeedCounter.Speed;
        public double UploadSpeed => _uploadSpeedCounter.Speed;
        public long BytesDownloaded => _downloadSpeedCounter.Bytes;
        public long BytesUploaded => _uploadSpeedCounter.Bytes;

        public TimeSpan ConnectionDuration
        {
            get => _connectionDuration;
            private set => SetProperty(ref _connectionDuration, value);
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

        private void OnDownloadSpeedCounterUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(BytesDownloaded));
        }

        private void OnUploadSpeedCounterUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(UploadSpeed));
            OnPropertyChanged(nameof(BytesUploaded));
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

        public void SendEncrypted(BaseMessage message, byte channelNumber)
        {
            if (channelNumber < 0 ||
                channelNumber >= _peer.NetManager.ChannelsCount)
            {
                return;
            }

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

                _peer.Send(outcomingMessage, channelNumber, DeliveryMethod.ReliableOrdered);

                _uploadSpeedCounter.AddBytes(outcomingMessage.Data.Length);
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
                _cryptography.TryDecrypt(encryptedMessage, iv, out byte[] decryptedMessage) &&
                decryptedMessage.TryDecompressByteArray(out byte[] decompressedMessage) &&
                _cryptography.TryVerifySignature(decompressedMessage, signature))
            {
                var messageReader = new NetDataReader(decompressedMessage);

                if (messageReader.TryGetByte(out byte typeByte) &&
                    typeByte.TryParseType(out NetworkMessageType type) &&
                    messageReader.TryGetString(out string json) &&
                    messageReader.TryGetULong(out ulong recepientsOutcomingSegmentNumber) &&
                    recepientsOutcomingSegmentNumber == _incomingSegmentNumber)
                {
                    _incomingSegmentNumber += 1;
                    _incomingSegmentNumber = _incomingSegmentNumber == ulong.MaxValue ? 0 : _incomingSegmentNumber;

                    _downloadSpeedCounter.AddBytes(incomingDataReader.RawDataSize);

                    messageType = type;
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

        public override string ToString()
        {
            return _peer.EndPoint.ToString();
        }
    }
}
