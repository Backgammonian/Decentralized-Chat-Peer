using System;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Win32;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Meziantou.Framework.WPF.Collections;
using SystemTrayApp.WPF;
using Networking;
using Networking.Messages;
using Networking.Utils;
using InputBox;
using ImageShowcase;
using DropFiles;
using Helpers;
using UdpNatPunchClient.Models;

namespace UdpNatPunchClient
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private const string _userMessagePlaceholder = "Write a message to user...";
        private const string _trackerMessagePlaceholder = "Write a message to tracker...";
        private const string _noMessagePlaceholder = "---";
        private const string _noTrackerAddressPlaceholder = "---";
        private const string _title = "Chat Client";
        private const int _profileTabIndex = 0;
        private const int _chatTabIndex = 1;
        private const string _defaultTrackerIPAddress = "192.168.0.14";
        private const int _defaultTrackerPort = 56000;

        private readonly Client _client;
        private readonly Users _connectedUsers;
        private readonly DispatcherTimer _nicknameUpdateTimer;
        private readonly DispatcherTimer _localAddressUpdater;
        private readonly DispatcherTimer _keepAliveTimer;
        private string _titleText = _title;
        private TrackerModel? _tracker;
        private string _nickname = "My nickname";
        private NicknameUpdateState _nicknameUpdateState;
        private ImageItem? _profilePicture;
        private ProfilePictureLoadingStatusType _profilePictureLoadingStatus;
        private bool _isConnectedToTracker;
        private string _currentMessage = string.Empty;
        private ConcurrentObservableCollection<BaseMessageModel>? _messages;
        private PeerModel? _selectedPeer;
        private Action? _scrollMessageBoxToEnd;
        private Action? _focusOnMessageBox;
        private bool _canSendMessage;
        private bool _canSendImage;
        private string? _currentPlaceholder;
        private IPEndPoint? _externalAddress;
        private IPEndPoint? _localAddress;
        private int _tabIndex;
        private byte[]? _profilePictureBytes;
        private BaseMessageModel? _selectedMessage;
        private string _systemTrayText = string.Empty;
        private TrackerConnectionStatusType _trackerConnectionStatus;
        private IPEndPoint? _expectedAddress;

        public MainWindowViewModel()
        {
            InitializeSystemTrayCommands();

            ConnectToTrackerCommand = new RelayCommand(ConnectToTracker);
            PutAsciiArtCommand = new RelayCommand<AsciiArtsType>(PutAsciiArt);
            SendMessageCommand = new RelayCommand(SendMessage);
            SelectTrackerDialogCommand = new RelayCommand(SelectTrackerDialog);
            ChangeProfilePictureCommand = new AsyncRelayCommand(ChangeProfilePicture);
            GetNewProfilePictureFromDropCommand = new AsyncRelayCommand<FilesDroppedEventArgs?>(GetNewProfilePictureFromDrop);
            SendImageCommand = new AsyncRelayCommand(SelectImageToSend);
            SendImagesFromDropCommand = new AsyncRelayCommand<FilesDroppedEventArgs?>(SendImagesFromFileDrop);
            ShowOwnProfilePictureCommand = new RelayCommand<MouseEventArgs?>(ShowOwnProfilePicture);
            ShowPeerProfilePictureCommand = new RelayCommand<MouseEventArgs?>(ShowPeerProfilePicture);
            ShowPictureFromMessageCommand = new RelayCommand<MouseEventArgs?>(ShowPictureFromMessage);

            _tracker = null;

            _client = new Client();
            _client.PeerAdded += OnPeerAdded;
            _client.PeerConnected += OnPeerConnected;
            _client.PeerRemoved += OnPeerRemoved;
            _client.MessageFromPeerReceived += OnMessageFromPeerReceived;
            _client.TrackerAdded += OnTrackerAdded;
            _client.TrackerConnected += OnTrackerConnected;
            _client.TrackerRemoved += OnTrackerRemoved;
            _client.MessageFromTrackerReceived += OnMessageFromTrackerReceived;
            _client.TrackerConnectionAttemptFailed += OnTrackerConnectionAttemptFailed;

            _connectedUsers = new Users();
            _connectedUsers.UserAdded += OnConnectedPeerAdded;
            _connectedUsers.UserRemoved += OnConnectedPeerRemoved;

            _localAddressUpdater = new DispatcherTimer();
            _localAddressUpdater.Interval = new TimeSpan(0, 0, 2);
            _localAddressUpdater.Tick += OnLocalAddressUpdaterTick;
            _nicknameUpdateTimer = new DispatcherTimer();
            _nicknameUpdateTimer.Interval = new TimeSpan(0, 0, 1);
            _nicknameUpdateTimer.Tick += OnNcknameUpdateTimerTick;
            _keepAliveTimer = new DispatcherTimer();
            _keepAliveTimer.Interval = new TimeSpan(0, 0, 5);
            _keepAliveTimer.Tick += OnKeepAliveTimerTick;

            TextArts = new ObservableCollection<AsciiArtsType>(Enum.GetValues(typeof(AsciiArtsType)).Cast<AsciiArtsType>());
            SystemTrayText = _title;
            ID = RandomGenerator.GetRandomString(30);
            CurrentMessage = string.Empty;
            IsConnectedToTracker = false;
            SelectedPeer = null;
            CanSendMessage = false;
            CanSendImage = false;
            ExternalEndPoint = null;
            LocalEndPoint = new IPEndPoint(new LocalAddressResolver().GetLocalAddress(), _client.LocalPort);
            NicknameUpdateState = NicknameUpdateState.None;
            ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.None;
            TrackerConnectionStatus = TrackerConnectionStatusType.None;
        }

        public ICommand ConnectToTrackerCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand PutAsciiArtCommand { get; }
        public ICommand SelectTrackerDialogCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand ShowOwnProfilePictureCommand { get; }
        public ICommand ShowPeerProfilePictureCommand { get; }
        public ICommand SendImageCommand { get; }
        public ICommand GetNewProfilePictureFromDropCommand { get; }
        public ICommand SendImagesFromDropCommand { get; }
        public ICommand ShowPictureFromMessageCommand { get; }

        public string ID { get; }
        public ObservableCollection<AsciiArtsType> TextArts { get; }
        public string TrackerAddress => _tracker == null ? "---" : _tracker.EndPoint;
        public ObservableCollection<UserModel> ConnectedUsers => new ObservableCollection<UserModel>(_connectedUsers.List.OrderBy(user => user.ConnectionTime));

        public IPEndPoint? ExpectedTracker
        {
            get => _expectedAddress;
            private set => SetProperty(ref _expectedAddress, value);
        }

        public string SystemTrayText
        {
            get => _systemTrayText;
            set => SetProperty(ref _systemTrayText, value);
        }

        public string TitleText
        {
            get => _titleText;
            private set => SetProperty(ref _titleText, value);
        }

        public string Nickname
        {
            get => _nickname;
            set
            {
                if (StringExtensions.IsNotEmpty(value))
                {
                    if (value.Length > Constants.MaxNicknameLength)
                    {
                        SetProperty(ref _nickname, value.Substring(0, Constants.MaxNicknameLength));
                    }
                    else
                    {
                        SetProperty(ref _nickname, value);
                    }

                    NicknameUpdateState = NicknameUpdateState.Changing;
                    RestartNicknameUpdateTimerTick();
                }
            }
        }

        public NicknameUpdateState NicknameUpdateState
        {
            get => _nicknameUpdateState;
            private set => SetProperty(ref _nicknameUpdateState, value);
        }

        public ImageItem? ProfilePicture
        {
            get => _profilePicture;
            private set => SetProperty(ref _profilePicture, value);
        }

        public ProfilePictureLoadingStatusType ProfilePictureLoadingStatus
        {
            get => _profilePictureLoadingStatus;
            private set => SetProperty(ref _profilePictureLoadingStatus, value);
        }

        public bool IsConnectedToTracker
        {
            get => _isConnectedToTracker;
            private set => SetProperty(ref _isConnectedToTracker, value);
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set
            {
                if (value.Length > Constants.MaxMessageLength)
                {
                    SetProperty(ref _currentMessage, value.Substring(0, Constants.MaxMessageLength));
                }
                else
                {
                    SetProperty(ref _currentMessage, value);
                }
            }
        }

        public string? CurrentPlaceholder
        {
            get => _currentPlaceholder;
            private set => SetProperty(ref _currentPlaceholder, value);
        }

        public bool CanSendMessage
        {
            get => _canSendMessage;
            private set => SetProperty(ref _canSendMessage, value);
        }

        public bool CanSendImage
        {
            get => _canSendImage;
            private set => SetProperty(ref _canSendImage, value);
        }

        public IPEndPoint? ExternalEndPoint
        {
            get => _externalAddress;
            private set => SetProperty(ref _externalAddress, value);
        }

        public IPEndPoint? LocalEndPoint
        {
            get => _localAddress;
            private set => SetProperty(ref _localAddress, value);
        }

        public ConcurrentObservableCollection<BaseMessageModel>? Messages
        {
            get => _messages;
            private set => SetProperty(ref _messages, value);
        }

        public BaseMessageModel? SelectedMessage
        {
            get => _selectedMessage;
            set => SetProperty(ref _selectedMessage, value);
        }

        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                if (value >= 0)
                {
                    SetProperty(ref _tabIndex, value);
                }
                else
                {
                    SetProperty(ref _tabIndex, -1);
                }
            }
        }

        public PeerModel? SelectedPeer
        {
            get => _selectedPeer;
            set
            {
                SetProperty(ref _selectedPeer, value);

                if (SelectedPeer != null)
                {
                    Messages = SelectedPeer.Messages;
                    SelectedPeer.SendNotificationsToAllUnreadIncomingMessages();
                    CanSendMessage = true;

                    FocusOnMessageTextBox();

                    if (SelectedPeer is UserModel)
                    {
                        CurrentPlaceholder = _userMessagePlaceholder;
                        CanSendImage = true;
                    }
                    else
                    if (SelectedPeer is TrackerModel)
                    {
                        CanSendImage = false;
                        CurrentPlaceholder = _trackerMessagePlaceholder;
                    }
                }
                else
                {
                    CanSendImage = false;
                    CanSendMessage = false;
                    Messages = null;
                    CurrentPlaceholder = _noMessagePlaceholder;
                }
            }
        }

        public TrackerConnectionStatusType TrackerConnectionStatus
        {
            get => _trackerConnectionStatus;
            private set => SetProperty(ref _trackerConnectionStatus, value);
        }

        private void StartApp()
        {
            _client.StartListening();

            _localAddressUpdater.Start();
            _keepAliveTimer.Start();
        }

        private void ShutdownApp()
        {
            _tracker?.Disconnect();
            _client.DisconnectAll();
            _client.Stop();

            Application.Current.Shutdown();
        }

        private void RestartNicknameUpdateTimerTick()
        {
            _nicknameUpdateTimer.Stop();
            _nicknameUpdateTimer.Start();
        }

        private void OnNcknameUpdateTimerTick(object? sender, EventArgs e)
        {
            _nicknameUpdateTimer.Stop();
            NicknameUpdateState = NicknameUpdateState.Updated;

            _tracker?.SendUpdatedPersonalInfo(Nickname);
            _connectedUsers.SendUpdatedInfoToConnectedUsers(Nickname);

            var resetTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            resetTimer.Interval = new TimeSpan(0, 0, 1);
            resetTimer.Tick += (s, e) =>
            {
                resetTimer.Stop();
                NicknameUpdateState = NicknameUpdateState.None;
            };
            resetTimer.Start();
        }

        private void OnLocalAddressUpdaterTick(object? sender, EventArgs e)
        {
            _localAddressUpdater.Stop();
            LocalEndPoint = new IPEndPoint(new LocalAddressResolver().GetLocalAddress(), _client.LocalPort);
        }

        private void OnConnectedPeerAdded(object? sender, UserUpdatedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedUsers));

            if (WindowState == WindowState.Minimized ||
                TabIndex != _chatTabIndex)
            {
                Notify($"New user {e.User.Nickname}",
                    $"User {e.User.Nickname} ({e.User.ID}) has connected",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void OnConnectedPeerRemoved(object? sender, UserUpdatedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedUsers));

            if (WindowState == WindowState.Minimized ||
                TabIndex != _chatTabIndex)
            {
                Notify($"Disconnect from user {e.User.Nickname}",
                    $"User {e.User.Nickname} ({e.User.ID}) has disconnected",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void OnPeerAdded(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine("(OnPeerAdded) " + e.Peer.Id);
        }

        private void OnPeerConnected(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine("(OnPeerConnected) " + e.Peer.Id);

            var profilePictureByteArray = Array.Empty<byte>();
            var profilePictureExtension = string.Empty;

            if (ProfilePicture != null &&
                _profilePictureBytes != null)
            {
                profilePictureByteArray = _profilePictureBytes;
                profilePictureExtension = ProfilePicture.FileExtension;
            }

            var introducePeerToPeerMessage = new IntroducePeerToPeerMessage(ID, Nickname, profilePictureByteArray, profilePictureExtension);
            e.Peer.SendEncrypted(introducePeerToPeerMessage);
        }

        private void OnPeerRemoved(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine("(OnPeerRemoved) " + e.Peer.Id);

            var user = _connectedUsers.GetUserByPeer(e.Peer);
            if (user == null)
            {
                return;
            }

            if (user == SelectedPeer)
            {
                SelectedPeer = null;
            }

            _connectedUsers.Remove(user.ID);
        }

        private async Task OnMessageFromPeerReceived(object? sender, NetEventArgs e)
        {
            var source = e.EncryptedPeer;
            var type = e.Type;
            var json = e.Json;

            Debug.WriteLine("(OnMessageFromPeerReceived) source = " + source.EndPoint);
            Debug.WriteLine("(OnMessageFromPeerReceived) type = " + type);

            var author = _connectedUsers.GetUserByPeer(source);

            switch (type)
            {
                case NetworkMessageType.KeepAlive:
                    Debug.WriteLine($"KeepAliveMessage from peer {source.EndPoint}");
                    break;

                case NetworkMessageType.IntroducePeerToPeer:
                    var introducePeerToPeerMessage = JsonConvert.DeserializeObject<IntroducePeerToPeerMessage>(json);
                    if (introducePeerToPeerMessage == null)
                    {
                        return;
                    }

                    await _connectedUsers.Add(introducePeerToPeerMessage, source);

                    var profilePictureByteArray = Array.Empty<byte>();
                    var profilePictureExtension = string.Empty;

                    if (ProfilePicture != null &&
                        _profilePictureBytes != null)
                    {
                        profilePictureByteArray = _profilePictureBytes;
                        profilePictureExtension = ProfilePicture.FileExtension;
                    }

                    var introducePeerToPeerResponseMessage = new IntroducePeerToPeerResponse(ID, Nickname, profilePictureByteArray, profilePictureExtension);
                    source.SendEncrypted(introducePeerToPeerResponseMessage);
                    break;

                case NetworkMessageType.IntroducePeerToPeerResponse:
                    var introducePeerToPeerResponse = JsonConvert.DeserializeObject<IntroducePeerToPeerResponse>(json);
                    if (introducePeerToPeerResponse == null)
                    {
                        return;
                    }

                    await _connectedUsers.Add(introducePeerToPeerResponse, source);
                    break;

                case NetworkMessageType.UpdatedInfoForPeer:
                    if (author == null)
                    {
                        return;
                    }

                    var updatedInfoMessage = JsonConvert.DeserializeObject<UpdatedInfoForPeerMessage>(json);
                    if (updatedInfoMessage == null)
                    {
                        return;
                    }

                    Debug.WriteLine("Updated nickname: " + updatedInfoMessage.UpdatedNickname);

                    author.SetUpdatedPersonalInfo(updatedInfoMessage.UpdatedNickname);
                    break;

                case NetworkMessageType.UpdatedProfilePictureForPeer:
                    if (author == null)
                    {
                        return;
                    }

                    var updatedPictureMessage = JsonConvert.DeserializeObject<UpdatedProfilePictureForPeerMessage>(json);
                    if (updatedPictureMessage == null)
                    {
                        return;
                    }

                    Debug.WriteLine($"Updated profile picture array is null: {updatedPictureMessage.UpdatedPictureArray == null}");
                    Debug.WriteLine($"Updated profile picture array length: {(updatedPictureMessage.UpdatedPictureArray == null ? 0 : updatedPictureMessage.UpdatedPictureArray.Length)}");

                    await author.TrySetUpdatedPicture(updatedPictureMessage.UpdatedPictureArray, updatedPictureMessage.UpdatedPictureExtension);
                    break;

                case NetworkMessageType.TextMessage:
                    if (author == null)
                    {
                        return;
                    }

                    var textMessageFromPeer = JsonConvert.DeserializeObject<TextMessageToPeer>(json);
                    if (textMessageFromPeer == null)
                    {
                        return;
                    }

                    var receivedMessage = author.AddIncomingMessage(textMessageFromPeer);
                    if (receivedMessage == null)
                    {
                        return;
                    }

                    if (SelectedPeer == author)
                    {
                        author.DismissNewMessagesSignal();
                        author.SendReadNotification(receivedMessage);
                    }
                    else
                    {
                        author.SendReceiptNotification(receivedMessage);

                        if (WindowState == WindowState.Minimized ||
                            TabIndex != _chatTabIndex)
                        {
                            Notify("New message!",
                                $"Incoming message from {author.Nickname} ({author.ID})",
                                1500,
                                System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
                    break;

                case NetworkMessageType.ImageIntroduceMessage:
                    if (author == null)
                    {
                        return;
                    }

                    var imageIntroduceMessage = JsonConvert.DeserializeObject<ImageIntroduceMessage>(json);
                    if (imageIntroduceMessage == null)
                    {
                        return;
                    }

                    var receivedImageIntroduceMessage = author.AddIncomingMessage(imageIntroduceMessage);
                    if (receivedImageIntroduceMessage == null)
                    {
                        return;
                    }

                    if (SelectedPeer == author)
                    {
                        author.DismissNewMessagesSignal();
                        author.SendReadNotification(receivedImageIntroduceMessage);
                    }
                    else
                    {
                        author.SendReceiptNotification(receivedImageIntroduceMessage);

                        if (WindowState == WindowState.Minimized ||
                            TabIndex != _chatTabIndex)
                        {
                            Notify($"Receiving picture from user {author.Nickname}",
                                $"Incoming picture from {author.Nickname} ({author.ID})",
                                1500,
                                System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
                    break;

                case NetworkMessageType.UpdateImageMessage:
                    if (author == null)
                    {
                        return;
                    }

                    var updateImageMessage = JsonConvert.DeserializeObject<UpdateImageMessage>(json);
                    if (updateImageMessage == null)
                    {
                        return;
                    }

                    await author.SetUpdatedImageInMessage(updateImageMessage.MessageID,
                        updateImageMessage.PictureBytes,
                        updateImageMessage.PictureExtension);
                    break;

                case NetworkMessageType.ImageSendingFailed:
                    if (author == null)
                    {
                        return;
                    }

                    var imageSendingFailedMessage = JsonConvert.DeserializeObject<ImageSendingFailedMessage>(json);
                    if (imageSendingFailedMessage == null)
                    {
                        return;
                    }

                    author.SetImageMessageAsFailed(imageSendingFailedMessage.MessageID);
                    break;

                case NetworkMessageType.MessageReceiptNotification:
                    if (author == null)
                    {
                        return;
                    }

                    var messageReceiptNotification = JsonConvert.DeserializeObject<MessageReceiptNotification>(json);
                    if (messageReceiptNotification == null)
                    {
                        return;
                    }

                    author.MarkMessageAsDelivered(messageReceiptNotification.MessageID);
                    break;

                case NetworkMessageType.MessageReadNotification:
                    if (author == null)
                    {
                        return;
                    }

                    var messageReadNotification = JsonConvert.DeserializeObject<MessageReadNotification>(json);
                    if (messageReadNotification == null)
                    {
                        return;
                    }

                    author.MarkMessageAsReadAndDelivered(messageReadNotification.MessageID);
                    break;
            }
        }

        private void OnTrackerAdded(object? sender, EventArgs e)
        {
            if (_client.Tracker != null)
            {
                _tracker = new TrackerModel(_client.Tracker);
                OnPropertyChanged(nameof(TrackerAddress));
            }
        }

        private void OnTrackerConnected(object? sender, EventArgs e)
        {
            IsConnectedToTracker = true;
            OnPropertyChanged(nameof(TrackerAddress));

            _tracker?.SendIntroductionMessage(ID, Nickname);

            ExpectedTracker = _client.ExpectedTracker;
            TrackerConnectionStatus = TrackerConnectionStatusType.Connected;

            if (WindowState == WindowState.Minimized &&
                _tracker != null)
            {
                Notify($"Tracker {_tracker.EndPoint} has connected",
                    $"Tracker is ready to work! 😊",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }

            var resetTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            resetTimer.Interval = new TimeSpan(0, 0, 5);
            resetTimer.Tick += (s, e) =>
            {
                resetTimer.Stop();
                TrackerConnectionStatus = TrackerConnectionStatusType.None;
            };
            resetTimer.Start();
        }

        private void OnTrackerRemoved(object? sender, TrackerDisconnectedEventArgs e)
        {
            ExpectedTracker = e.TrackerEndPoint;
            TrackerConnectionStatus = TrackerConnectionStatusType.DisconnectFromTracker;

            var oldTrackerAddress = _tracker?.EndPoint;
            if (oldTrackerAddress == null)
            {
                oldTrackerAddress = _noTrackerAddressPlaceholder;
            }

            if (_tracker == SelectedPeer)
            {
                CanSendMessage = false;
                Messages = null;
            }

            _tracker = null;
            IsConnectedToTracker = false;
            ExternalEndPoint = null;

            OnPropertyChanged(nameof(TrackerAddress));

            if (WindowState == WindowState.Minimized)
            {
                Notify($"Tracker {oldTrackerAddress} is disconnected",
                    $"Tracker {oldTrackerAddress} said bye-bye!",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void OnMessageFromTrackerReceived(object? sender, NetEventArgs e)
        {
            var source = e.EncryptedPeer;
            var type = e.Type;
            var json = e.Json;

            Debug.WriteLine("(OnMessageFromTrackerReceived) source = " + source.EndPoint);
            Debug.WriteLine("(OnMessageFromTrackerReceived) type = " + type);

            switch (type)
            {
                case NetworkMessageType.KeepAlive:
                    Debug.WriteLine($"KeepAliveMessage from tracker {source.EndPoint}");
                    break;

                case NetworkMessageType.IntroduceClientToTrackerError:
                    MessageBox.Show("Tracker already has user with such ID: " + ID,
                        "Tracker connection error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    _tracker?.Disconnect();
                    break;

                case NetworkMessageType.IntroduceClientToTrackerResponse:
                    var introductionResponse = JsonConvert.DeserializeObject<IntroduceClientToTrackerResponseMessage>(json);
                    if (introductionResponse == null)
                    {
                        return;
                    }

                    if (IPEndPoint.TryParse(introductionResponse.EndPointString, out var externalEndPoint))
                    {
                        ExternalEndPoint = externalEndPoint;
                    }
                    break;

                case NetworkMessageType.CommandReceiptNotification:
                    var commandReceiptNotification = JsonConvert.DeserializeObject<CommandReceiptNotificationMessage>(json);
                    if (commandReceiptNotification == null)
                    {
                        return;
                    }

                    _tracker?.MarkCommandAsReadAndDelivered(commandReceiptNotification.CommandID);
                    break;

                case NetworkMessageType.CommandToTrackerError:
                    var commandErrorMessage = JsonConvert.DeserializeObject<CommandToTrackerErrorMessage>(json);
                    if (commandErrorMessage == null)
                    {
                        return;
                    }

                    _tracker?.PrintInfo($"Unrecognized command: '{commandErrorMessage.WrongCommand} {commandErrorMessage.Argument}'");

                    if (WindowState == WindowState.Minimized ||
                        TabIndex != _chatTabIndex)
                    {
                        Notify("Error message from tracker",
                            $"Incoming message from tracker {_tracker?.EndPoint}",
                            1500,
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                    break;

                case NetworkMessageType.UserConnectionResponse:
                    var userConnectionResponseMessage = JsonConvert.DeserializeObject<UserConnectionResponseMessage>(json);
                    if (userConnectionResponseMessage == null)
                    {
                        return;
                    }

                    if (IPEndPoint.TryParse(userConnectionResponseMessage.EndPointString, out var peerEndPoint))
                    {
                        _tracker?.PrintInfo($"Connecting to {userConnectionResponseMessage.ID} ({peerEndPoint})...");
                        _client.ConnectToPeer(peerEndPoint);
                    }
                    break;

                case NetworkMessageType.ForwardedConnectionRequest:
                    var forwardedConnectionRequestMessage = JsonConvert.DeserializeObject<ForwardedConnectionRequestMessage>(json);
                    if (forwardedConnectionRequestMessage == null)
                    {
                        return;
                    }

                    if (IPEndPoint.TryParse(forwardedConnectionRequestMessage.EndPointString, out var otherPeerEndPoint))
                    {
                        _client.ConnectToPeer(otherPeerEndPoint);
                    }
                    break;

                case NetworkMessageType.UserNotFoundError:
                    var userNotFoundErrorMessage = JsonConvert.DeserializeObject<UserNotFoundErrorMessage>(json);
                    if (userNotFoundErrorMessage == null)
                    {
                        return;
                    }

                    _tracker?.PrintInfo(string.Format("User not found, unknown ID or nickname: {0}", userNotFoundErrorMessage.UserInfo));

                    if (WindowState == WindowState.Minimized ||
                        TabIndex != _chatTabIndex)
                    {
                        Notify("Response message from tracker",
                            $"Incoming message from tracker {_tracker?.EndPoint}",
                            1500,
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                    break;

                case NetworkMessageType.PingResponse:
                    var pingResponseMessage = JsonConvert.DeserializeObject<PingResponseMessage>(json);
                    if (pingResponseMessage == null)
                    {
                        return;
                    }

                    _tracker?.PrintInfo(string.Format("Pong!\nPing: {0} ms", pingResponseMessage.Ping));

                    if (WindowState == WindowState.Minimized ||
                        TabIndex != _chatTabIndex)
                    {
                        Notify("Response message from tracker",
                            $"Incoming message from tracker {_tracker?.EndPoint}",
                            1500,
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                    break;

                case NetworkMessageType.TimeResponse:
                    var timeResponseMessage = JsonConvert.DeserializeObject<TimeResponseMessage>(json);
                    if (timeResponseMessage == null)
                    {
                        return;
                    }

                    var converter = new Converters.DateTimeConverter();
                    var formattedTime = converter.ConvertTime(timeResponseMessage.Time);
                    _tracker?.PrintInfo(string.Format("Tracker's time: {0}", formattedTime));

                    if (WindowState == WindowState.Minimized ||
                        TabIndex != _chatTabIndex)
                    {
                        Notify("Response message from tracker",
                            $"Incoming message from tracker {_tracker?.EndPoint}",
                            1500,
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                    break;

                case NetworkMessageType.ListOfUsersWithDesiredNickname:
                    var listOfUsersResponse = JsonConvert.DeserializeObject<ListOfUsersWithDesiredNicknameMessage>(json);
                    if (listOfUsersResponse == null)
                    {
                        return;
                    }

                    var usersQuery = listOfUsersResponse.Users;
                    if (usersQuery.Length == 0)
                    {
                        return;
                    }

                    _tracker?.PrintListOfUsers(usersQuery);

                    if (usersQuery.Length == 1)
                    {
                        CurrentMessage = $"connect {usersQuery[0].ID}";
                        SendMessage();
                    }

                    if (WindowState == WindowState.Minimized ||
                        TabIndex != _chatTabIndex)
                    {
                        Notify("Response message from tracker",
                            $"Incoming message from tracker {_tracker?.EndPoint}",
                            1500,
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                    break;
            }
        }

        private void OnTrackerConnectionAttemptFailed(object? sender, EventArgs e)
        {
            ExpectedTracker = _client.ExpectedTracker;
            TrackerConnectionStatus = TrackerConnectionStatusType.FailedToConnect;
        }

        private void ConnectToTracker()
        {
            var inputBox = new InputBoxUtils();
            if (inputBox.AskServerAddressAndPort(IPAddress.Parse(_defaultTrackerIPAddress), _defaultTrackerPort, out IPEndPoint? address) &&
                address != null)
            {
                if (_client.IsConnectedToTracker(address))
                {
                    MessageBox.Show(
                        "Already connected to this tracker",
                        "Tracker connection warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    _client.ConnectToTracker(address);

                    TrackerConnectionStatus = TrackerConnectionStatusType.TryingToConnect;
                    ExpectedTracker = _client.ExpectedTracker;
                }
            }
            else
            {
                MessageBox.Show("Tracker address is not valid! Try enter correct IP address and port (example: 10.0.8.100:56000)",
                    "Tracker address error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool TryParseCommand(string input, out string command, out string argument)
        {
            var spacePosition = input.IndexOf(' ');
            if (spacePosition == -1)
            {
                command = input.Substring(0, input.Length).ToLower();
                argument = string.Empty;
            }
            else
            {
                command = input.Substring(0, spacePosition).ToLower();
                argument = input.Substring(spacePosition + 1, input.Length - spacePosition - 1);
            }

            return true;
        }

        private async Task SelectImageToSend()
        {
            if (TrySelectFile("Select a picture to send", Constants.ImageFilter, out var path) &&
                TryGetFileSize(path, out var size))
            {
                if (size == 0)
                {
                    MessageBox.Show($"Picture {Path.GetFileName(path)} is empty!",
                        "Image load error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }
                else
                if (size > Constants.MaxPictureSize)
                {
                    var bytesConverter = new Converters.BytesToMegabytesConverter();
                    MessageBox.Show($"Picture {Path.GetFileName(path)} is too big.\n" +
                        $"Maximum size: {bytesConverter.ConvertSize(Constants.MaxProfilePictureSize)}, " +
                        $"size of selected image: {bytesConverter.ConvertSize(size)}",
                        "Image load error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                await SendImage(path);
            }
        }

        private async Task SendImagesFromFileDrop(FilesDroppedEventArgs? args)
        {
            if (args == null)
            {
                return;
            }

            foreach (var imagePath in args.FilesPath)
            {
                var imageExtension = Path.GetExtension(imagePath);

                if (Constants.AllowedImageExtensions.Contains(imageExtension) &&
                    TryGetFileSize(imagePath, out var size))
                {
                    if (size == 0)
                    {
                        MessageBox.Show($"Picture {Path.GetFileName(imagePath)} is empty!",
                            "Picture load error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        break;
                    }
                    else
                    if (size > Constants.MaxPictureSize)
                    {
                        var bytesConverter = new Converters.BytesToMegabytesConverter();
                        MessageBox.Show($"Picture {Path.GetFileName(imagePath)} is too big.\n" +
                            $"Maximum size: {bytesConverter.ConvertSize(Constants.MaxPictureSize)}, " +
                            $"size of selected image: {bytesConverter.ConvertSize(size)}",
                            "Picture load error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        break;
                    }

                    await SendImage(imagePath);
                }
            }
        }

        private async Task SendImage(string path)
        {
            if (SelectedPeer is UserModel user)
            {
                var message = user.SendImageIntroductionMessage();

                var image = new ImageItem(path,
                    Constants.ImageMessageThumbnailSize.Item1,
                    Constants.ImageMessageThumbnailSize.Item2);

                var isImageLoaded = await image.TryLoadImage();
                var bytes = await image.GetPictureBytes();

                if (isImageLoaded &&
                    bytes != null)
                {
                    message.UpdateImage(image);

                    user.SendUpdatedImageMessage(message.MessageID, bytes, image.FileExtension);
                }
                else
                {
                    message.SetAsFailed();

                    user.SendMessageAboutFailedImageSending(message.MessageID);
                }
            }
            else
            if (SelectedPeer is TrackerModel tracker)
            {
                tracker.PrintInfo("You can't send images to the tracker.");
            }
        }

        private void SendMessage()
        {
            if (SelectedPeer is UserModel user)
            {
                user.SendTextMessage(new MessageModel(CurrentMessage));
            }
            else
            if (SelectedPeer is TrackerModel tracker)
            {
                if (TryParseCommand(CurrentMessage, out var command, out var argument))
                {
                    if (command == "help")
                    {
                        tracker.PrintHelp();
                    }
                    else
                    {
                        tracker.SendCommandMessage(command, argument);
                    }
                }
                else
                {
                    tracker.PrintSupport(CurrentMessage);
                }
            }

            CurrentMessage = string.Empty;
        }

        private void PutAsciiArt(AsciiArtsType artType)
        {
            switch (artType)
            {
                case AsciiArtsType.Goose:
                    CurrentMessage += AsciiArts.Goose;
                    break;

                case AsciiArtsType.Shrek:
                    CurrentMessage += AsciiArts.Shrek;
                    break;

                case AsciiArtsType.MegamindAsking:
                    CurrentMessage += AsciiArts.MegamindAsking;
                    break;

                case AsciiArtsType.Doge:
                    CurrentMessage += AsciiArts.Doge;
                    break;

                case AsciiArtsType.Facepalm:
                    CurrentMessage += AsciiArts.Facepalm;
                    break;

                case AsciiArtsType.Jerma:
                    CurrentMessage += AsciiArts.Jerma;
                    break;
            }

            ScrollMessageBoxToEnd();
        }

        private void SelectTrackerDialog()
        {
            if (_tracker != null)
            {
                TabIndex = _chatTabIndex;
                SelectedPeer = _tracker;
                FocusOnMessageTextBox();
            }
        }

        private bool TrySelectFile(string title, string filter, out string path)
        {
            var selectFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            if (selectFileDialog.ShowDialog() != true)
            {
                path = string.Empty;

                return false;
            }

            path = selectFileDialog.FileName;

            return true;
        }

        private bool TryGetFileSize(string path, out long size)
        {
            size = 0;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                size = stream.Length;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ShowOwnProfilePicture(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            switch (e.LeftButton)
            {
                case MouseButtonState.Released:
                    ShowImageFullScreen(ProfilePicture);
                    break;
            }
        }

        private void ShowPeerProfilePicture(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            switch (e.LeftButton)
            {
                case MouseButtonState.Released:
                    if (SelectedPeer is UserModel user)
                    {
                        ShowImageFullScreen(user.Picture);
                    }
                    break;
            }
        }

        private void ShowPictureFromMessage(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            switch (e.LeftButton)
            {
                case MouseButtonState.Released:
                    if (SelectedMessage is ImageMessageModel message)
                    {
                        ShowImageFullScreen(message.Image);
                    }
                    break;
            }
        }

        private void ShowImageFullScreen(ImageItem? image)
        {
            if (image == null)
            {
                return;
            }

            var showcaseWindow = new ImageShowcaseWindow(image);
            showcaseWindow.ShowDialog();
        }

        private async Task ChangeProfilePicture()
        {
            if (TrySelectFile("Select new profile picture", Constants.ImageFilter, out var path))
            {
                await UpdateProfilePicture(path);
            }
        }

        private async Task GetNewProfilePictureFromDrop(FilesDroppedEventArgs? args)
        {
            if (args == null ||
                args.FilesPath.Length == 0)
            {
                return;
            }

            var imagePath = args.FilesPath[0];
            var imageExtension = Path.GetExtension(imagePath);
            if (Constants.AllowedImageExtensions.Contains(imageExtension))
            {
                await UpdateProfilePicture(imagePath);
            }
            else
            {
                MessageBox.Show($"File {imagePath} has incorrect extension.\n" +
                    $"Allowed extensions: {string.Join("; ", Constants.AllowedImageExtensions)}",
                    "Image load error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task UpdateProfilePicture(string path)
        {
            var gotSize = TryGetFileSize(path, out long size);
            if (!gotSize)
            {
                return;
            }

            if (size == 0)
            {
                MessageBox.Show("Selected profile picture is empty!",
                    "Image load error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            else
            if (size > Constants.MaxProfilePictureSize)
            {
                var bytesConverter = new Converters.BytesToMegabytesConverter();
                MessageBox.Show($"Selected profile picture is too big.\n" +
                    $"Maximum size: {bytesConverter.ConvertSize(Constants.MaxProfilePictureSize)}, " +
                    $"size of selected image: {bytesConverter.ConvertSize(size)}",
                    "Image load error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
           
            ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.Loading;

            ProfilePicture = new ImageItem(path,
                Constants.ProfilePictureThumbnailSize.Item1,
                Constants.ProfilePictureThumbnailSize.Item2);

            var isLoaded = await ProfilePicture.TryLoadImage();
            if (!isLoaded)
            {
                ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.ErrorOccurred;

                MessageBox.Show($"Can't load profile picture from {path}",
                    "Image load error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var bytes = await ProfilePicture.GetPictureBytes();
            if (bytes != null)
            {
                ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.Completed;
                _profilePictureBytes = bytes;
                _connectedUsers.SendUpdatedProfilePictureToConnectedUsers(_profilePictureBytes, ProfilePicture.FileExtension);

                Debug.WriteLine($"(UpdateProfilePicture) Length of profile picture array: {_profilePictureBytes.Length}");

                var resetTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
                resetTimer.Interval = new TimeSpan(0, 0, 2);
                resetTimer.Tick += (s, e) =>
                {
                    resetTimer.Stop();
                    ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.None;
                };
                resetTimer.Start();
            }
            else
            {
                ProfilePicture = null;
                _profilePictureBytes = null;
                ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.ErrorOccurred;

                MessageBox.Show($"Couldn't load new profile picture properly: {path}",
                    "Image load error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void SetSelectedUserModel(UserModel userModel)
        {
            if (ConnectedUsers.Contains(userModel))
            {
                SelectedPeer = userModel;
            }
            else
            {
                Debug.WriteLine("(SetSelectedUserModel) Can't select userModel: " + userModel.ID);
            }
        }

        public void PassScrollingDelegate(Action scrollToEndDelegate)
        {
            _scrollMessageBoxToEnd = scrollToEndDelegate;
        }

        public void PassMessageTextBoxFocusDelegate(Action focusDelegate)
        {
            _focusOnMessageBox = focusDelegate;
        }

        private void ScrollMessageBoxToEnd()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _scrollMessageBoxToEnd?.Invoke();
            });
        }

        private void FocusOnMessageTextBox()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _focusOnMessageBox?.Invoke();
            });
        }

        private void OnKeepAliveTimerTick(object? sender, EventArgs e)
        {
            _tracker?.SendKeepAliveMessage();
            _connectedUsers.SendKeepAliveMessageToConnectedUsers();
        }
    }

    public partial class MainWindowViewModel
    {
        private NotifyIconWrapper.NotifyRequestRecord? _notifyRequest;
        private bool _showInTaskbar;
        private WindowState _windowState;

        public ICommand? LoadedCommand { get; private set; }
        public ICommand? ClosingCommand { get; private set; }
        public ICommand? NotifyIconOpenCommand { get; private set; }
        public ICommand? NotifyIconExitCommand { get; private set; }

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                ShowInTaskbar = true;
                SetProperty(ref _windowState, value);
                ShowInTaskbar = value != WindowState.Minimized;
            }
        }

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set => SetProperty(ref _showInTaskbar, value);
        }

        public NotifyIconWrapper.NotifyRequestRecord? NotifyRequest
        {
            get => _notifyRequest;
            set => SetProperty(ref _notifyRequest, value);
        }

        private void Notify(string title, string message, int durationMs, System.Windows.Forms.ToolTipIcon icon)
        {
            NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
            {
                Title = title,
                Text = message,
                Duration = durationMs,
                Icon = icon,
            };
        }

        private void Loaded()
        {
            WindowState = WindowState.Normal;

            StartApp();
        }

        private void Closing(CancelEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }

        private void InitializeSystemTrayCommands()
        {
            LoadedCommand = new RelayCommand(Loaded);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            NotifyIconOpenCommand = new RelayCommand(() => { WindowState = WindowState.Normal; });
            NotifyIconExitCommand = new RelayCommand(ShutdownApp);
        }
    }
}
