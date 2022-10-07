using System;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Timers;
using Microsoft.Win32;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Meziantou.Framework.WPF.Collections;
using SystemTrayApp.WPF;
using NetworkingLib;
using NetworkingLib.Messages;
using NetworkingLib.Utils;
using InputBox;
using ImageShowcase;
using DropFiles;
using Helpers;
using Extensions;
using UdpNatPunchClient.Models;

namespace UdpNatPunchClient
{
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        private const string _userMessagePlaceholder = "Write a message to user...";
        private const string _trackerMessagePlaceholder = "Write a message to tracker...";
        private const string _noMessagePlaceholder = "---";
        private const string _noTrackerAddressPlaceholder = "---";
        private const string _title = "Chat Client";
        private const int _profileTabIndex = 0;
        private const int _chatTabIndex = 1;
        private const int _defaultTrackerPort = 56000;
        private static readonly IPAddress _defaultTrackerIPAddress = LocalAddressResolver.GetLocalAddress();

        private readonly Client _client;
        private readonly Users _connectedUsers;
        private readonly Timer _nicknameUpdateTimer;
        private readonly Timer _localAddressUpdater;
        private readonly AvailableFiles _availableFiles;
        private readonly Downloads _downloads;
        private readonly SharedFiles _sharedFiles;
        private readonly Uploads _uploads;
        private string _titleText = string.Empty;
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
        private bool _canSendMedia;
        private string? _currentPlaceholder;
        private IPEndPoint? _externalAddress;
        private IPEndPoint? _localAddress;
        private int _tabIndex;
        private byte[]? _profilePictureBytes;
        private BaseMessageModel? _selectedMessage;
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
            SendFileCommand = new AsyncRelayCommand(SelectFileToSend);
            SendMediaFromDropCommand = new AsyncRelayCommand<FilesDroppedEventArgs?>(SendMediaFromFileDrop);
            ShowOwnProfilePictureCommand = new RelayCommand<MouseEventArgs?>(ShowOwnProfilePicture);
            ShowPeerProfilePictureCommand = new RelayCommand<MouseEventArgs?>(ShowPeerProfilePicture);
            ShowPictureFromMessageCommand = new RelayCommand<MouseEventArgs?>(ShowPictureFromMessage);
            DownloadFileCommand = new RelayCommand<AvailableFile>(DownloadFile);
            CancelDownloadCommand = new RelayCommand<Download>(CancelDownload);
            OpenFileInFolderCommand = new RelayCommand<Download>(OpenFileInFolder);
            RemoveSharedFileCommand = new RelayCommand<SharedFile>(RemoveSharedFile);

            _client = new Client();
            _client.PeerAdded += OnPeerAdded;
            _client.PeerConnected += OnPeerConnected;
            _client.PeerRemoved += OnPeerRemoved;
            _client.MessageFromPeerReceived += OnMessageFromPeerReceived;
            _client.ServerAdded += OnTrackerAdded;
            _client.ServerConnected += OnTrackerConnected;
            _client.ServerRemoved += OnTrackerRemoved;
            _client.MessageFromServerReceived += OnMessageFromTrackerReceived;
            _client.ServerConnectionAttemptFailed += OnTrackerConnectionAttemptFailed;

            _connectedUsers = new Users();
            _connectedUsers.UserAdded += OnConnectedPeerAdded;
            _connectedUsers.UserRemoved += OnConnectedPeerRemoved;

            _localAddressUpdater = new Timer();
            _localAddressUpdater.Interval = 2000;
            _localAddressUpdater.Elapsed += OnLocalAddressUpdaterTick;

            _nicknameUpdateTimer = new Timer();
            _nicknameUpdateTimer.Interval = 1000;
            _nicknameUpdateTimer.Elapsed += OnNicknameUpdateTimerTick;

            _availableFiles = new AvailableFiles();
            _availableFiles.FileAdded += OnAvailableFileAdded;
            _availableFiles.FileRemoved += OnAvailableFileRemoved;

            _downloads = new Downloads();
            _downloads.DownloadFinished += OnDownloadFinished;
            _downloads.DownloadsListUpdated += OnDownloadsListUpdated;

            _sharedFiles = new SharedFiles();
            _sharedFiles.SharedFileAdded += OnSharedFileAdded;
            _sharedFiles.SharedFileError += OnSharedFileError;
            _sharedFiles.SharedFileHashCalculated += OnSharedFileHashCalculated;
            _sharedFiles.SharedFileRemoved += OnSharedFileRemoved;

            _uploads = new Uploads();
            _uploads.UploadAdded += OnUploadAdded;
            _uploads.UploadRemoved += OnUploadRemoved;

            _tracker = null;

            ID = RandomGenerator.GetRandomString(30);
            TextArts = new ObservableCollection<AsciiArtsType>(Enum.GetValues(typeof(AsciiArtsType)).Cast<AsciiArtsType>());
            CurrentMessage = string.Empty;
            IsConnectedToTracker = false;
            SelectedPeer = null;
            CanSendMessage = false;
            CanSendMedia = false;
            ExternalEndPoint = null;
            LocalEndPoint = new IPEndPoint(LocalAddressResolver.GetLocalAddress(), _client.LocalPort);
            NicknameUpdateState = NicknameUpdateState.None;
            ProfilePictureLoadingStatus = ProfilePictureLoadingStatusType.None;
            TrackerConnectionStatus = TrackerConnectionStatusType.None;
        }

        #region Command bindings
        public ICommand ConnectToTrackerCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand PutAsciiArtCommand { get; }
        public ICommand SelectTrackerDialogCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand ShowOwnProfilePictureCommand { get; }
        public ICommand ShowPeerProfilePictureCommand { get; }
        public ICommand ShowPictureFromMessageCommand { get; }
        public ICommand SendImageCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand GetNewProfilePictureFromDropCommand { get; }
        public ICommand SendMediaFromDropCommand { get; }
        public ICommand DownloadFileCommand { get; }
        public ICommand CancelDownloadCommand { get; }
        public ICommand OpenFileInFolderCommand { get; }
        public ICommand RemoveSharedFileCommand { get; }
        #endregion

        #region Data bindings
        public string ID { get; }
        public ObservableCollection<AsciiArtsType> TextArts { get; }
        public string TrackerAddress => _tracker == null ? Converters.AddressConverter.NoAddress : _tracker.EndPoint;
        public ObservableCollection<UserModel> ConnectedUsers => new ObservableCollection<UserModel>(_connectedUsers.List);
        public ObservableCollection<AvailableFile> AvailableFiles => new ObservableCollection<AvailableFile>(_availableFiles.AvailableFilesList);
        public ObservableCollection<Download> Downloads => new ObservableCollection<Download>(_downloads.DownloadsList);
        public ObservableCollection<SharedFile> SharedFiles => new ObservableCollection<SharedFile>(_sharedFiles.SharedFilesList);
        public ObservableCollection<Upload> Uploads => new ObservableCollection<Upload>(_uploads.UploadsList);
        public string SystemTrayText => $"{_title} - {Nickname} ({ID})";

        public IPEndPoint? ExpectedTracker
        {
            get => _expectedAddress;
            private set => SetProperty(ref _expectedAddress, value);
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
                if (value.IsNotEmpty())
                {
                    if (value.Length > Constants.MaxNicknameLength)
                    {
                        SetProperty(ref _nickname, value.Substring(0, Constants.MaxNicknameLength));
                    }
                    else
                    {
                        SetProperty(ref _nickname, value);
                    }

                    OnPropertyChanged(nameof(SystemTrayText));
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

        public bool CanSendMedia
        {
            get => _canSendMedia;
            private set => SetProperty(ref _canSendMedia, value);
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
                        CanSendMedia = true;
                    }
                    else
                    if (SelectedPeer is TrackerModel)
                    {
                        CanSendMedia = false;
                        CurrentPlaceholder = _trackerMessagePlaceholder;
                    }
                }
                else
                {
                    CanSendMedia = false;
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
        #endregion

        #region Startup and shutdown methods
        private void StartApp()
        {
            _client.StartListening();
            _localAddressUpdater.Start();
        }

        private void ShutdownApp()
        {
            var question = MessageBox.Show("Do you want to close Chat Client? Existing connections with users will be shut down!",
                "Shutdown Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (question != MessageBoxResult.Yes)
            {
                return;
            }

            _tracker?.Disconnect();
            _connectedUsers.DisconnectAll();
            _client.Stop();

            Application.Current.Shutdown();
        }
        #endregion

        #region Event handlers
        private void RestartNicknameUpdateTimerTick()
        {
            _nicknameUpdateTimer.Stop();
            _nicknameUpdateTimer.Start();
        }

        private void OnNicknameUpdateTimerTick(object? sender, ElapsedEventArgs e)
        {
            _nicknameUpdateTimer.Stop();
            NicknameUpdateState = NicknameUpdateState.Updated;

            _tracker?.SendUpdatedPersonalInfo(Nickname);
            _connectedUsers.SendUpdatedInfoToConnectedUsers(Nickname);

            var resetTimer = new Timer();
            resetTimer.Interval = 1000;
            resetTimer.Elapsed += (s, e) =>
            {
                resetTimer.Stop();
                NicknameUpdateState = NicknameUpdateState.None;
            };
            resetTimer.Start();
        }

        private void OnLocalAddressUpdaterTick(object? sender, ElapsedEventArgs e)
        {
            _localAddressUpdater.Stop();
            LocalEndPoint = new IPEndPoint(LocalAddressResolver.GetLocalAddress(), _client.LocalPort);
        }

        private void OnConnectedPeerAdded(object? sender, UserUpdatedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedUsers));

            if (WindowState == WindowState.Minimized ||
                TabIndex != _chatTabIndex)
            {
                Notify($"New user {e.User.Nickname}",
                    $"User {e.User.Nickname} ({e.User.UserID}) has connected",
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
                    $"User {e.User.Nickname} ({e.User.UserID}) has disconnected",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void OnPeerAdded(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine($"(OnPeerAdded) {e.PeerID}");
        }

        private void OnPeerConnected(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine($"(OnPeerConnected) {e.PeerID}");

            var profilePictureByteArray = Array.Empty<byte>();
            var profilePictureExtension = string.Empty;

            if (ProfilePicture != null &&
                _profilePictureBytes != null)
            {
                profilePictureByteArray = _profilePictureBytes;
                profilePictureExtension = ProfilePicture.FileExtension;
            }

            var introducePeerToPeerMessage = new IntroducePeerToPeerMessage(ID, Nickname, profilePictureByteArray, profilePictureExtension);

            var peer = _client.GetPeerByID(e.PeerID);
            peer?.SendEncrypted(introducePeerToPeerMessage, 1);
        }

        private void OnPeerRemoved(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine($"(OnPeerRemoved) {e.PeerID}");

            var user = _connectedUsers.GetUserByPeerID(e.PeerID);
            if (user == null)
            {
                return;
            }

            if (user == SelectedPeer)
            {
                SelectedPeer = null;
            }

            _connectedUsers.Remove(user.UserID);
            _downloads.CancelAllDownloadsFromServer(e.PeerID);
            _uploads.CancelAllUploadsOfPeer(e.PeerID);
            _availableFiles.RemoveAllFilesFromServer(e.PeerID);
        }

        private async Task OnMessageFromPeerReceived(object? sender, NetEventArgs e)
        {
            var source = e.EncryptedPeer;
            var type = e.Type;
            var json = e.Json;

            Debug.WriteLine($"(OnMessageFromPeerReceived) source = {source.EndPoint}");
            Debug.WriteLine($"(OnMessageFromPeerReceived) type = {type}");

            var author = _connectedUsers.GetUserByPeerID(source.Id);

            switch (type)
            {
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
                    source.SendEncrypted(introducePeerToPeerResponseMessage, 1);
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
                                $"Incoming message from {author.Nickname} ({author.UserID})",
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
                                $"Incoming picture from {author.Nickname} ({author.UserID})",
                                1500,
                                System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
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

                case NetworkMessageType.FileMessage:
                    if (author == null)
                    {
                        return;
                    }

                    var fileMessage = JsonConvert.DeserializeObject<FileMessage>(json);
                    if (fileMessage == null)
                    {
                        return;
                    }

                    var receivedFileMessage = author.AddIncomingMessage(fileMessage);
                    _availableFiles.Add(receivedFileMessage.AvailableFile);

                    if (SelectedPeer == author)
                    {
                        author.DismissNewMessagesSignal();
                        author.SendReadNotification(receivedFileMessage);
                    }
                    else
                    {
                        author.SendReceiptNotification(receivedFileMessage);

                        if (WindowState == WindowState.Minimized ||
                            TabIndex != _chatTabIndex)
                        {
                            Notify($"New file link from user {author.Nickname}",
                                $"Incoming file link from {author.Nickname} ({author.UserID})",
                                1500,
                                System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
                    break;

                case NetworkMessageType.FileRequest:
                    if (author == null)
                    {
                        return;
                    }

                    var fileRequestMessage = JsonConvert.DeserializeObject<FileRequestMessage>(json);
                    if (fileRequestMessage == null)
                    {
                        return;
                    }

                    StartSendingFileToPeer(author, fileRequestMessage);
                    break;

                case NetworkMessageType.FileRequestError:
                    if (author == null)
                    {
                        return;
                    }

                    var fileRequestErrorMessage = JsonConvert.DeserializeObject<FileRequestErrorMessage>(json);
                    if (fileRequestErrorMessage == null)
                    {
                        return;
                    }

                    var failedDownload = _downloads.Get(fileRequestErrorMessage.DownloadID);
                    if (failedDownload != null)
                    {
                        failedDownload.CancelWithDeletion();
                    }

                    author.PrintError($"(Download error) File '{fileRequestErrorMessage.FileName}' is unavailable.");
                    break;

                case NetworkMessageType.FileSegment:
                    if (author == null)
                    {
                        return;
                    }

                    var fileSegmentMessage = JsonConvert.DeserializeObject<FileSegmentMessage>(json);
                    if (fileSegmentMessage == null)
                    {
                        return;
                    }

                    var download = _downloads.Get(fileSegmentMessage.UploadID);
                    if (download == null)
                    {
                        return;
                    }

                    download.Write(fileSegmentMessage.Segment);
                    break;

                case NetworkMessageType.FileSegmentAck:
                    if (author == null)
                    {
                        return;
                    }

                    var fileSegmentAckMessage = JsonConvert.DeserializeObject<FileSegmentAckMessage>(json);
                    if (fileSegmentAckMessage == null)
                    {
                        return;
                    }

                    var upload = _uploads.Get(fileSegmentAckMessage.DownloadID);
                    if (upload == null)
                    {
                        return;
                    }

                    upload.AddAck();
                    break;

                case NetworkMessageType.CancelDownload:
                    if (author == null)
                    {
                        return;
                    }

                    var downloadCancellationMessage = JsonConvert.DeserializeObject<DownloadCancellationMessage>(json);
                    if (downloadCancellationMessage == null)
                    {
                        return;
                    }

                    var downloadToCancel = _downloads.Get(downloadCancellationMessage.DownloadID);
                    if (downloadToCancel == null)
                    {
                        return;
                    }

                    downloadToCancel.Cancel();
                    break;

                case NetworkMessageType.CancelUpload:
                    if (author == null)
                    {
                        return;
                    }

                    var uploadCancellationMessage = JsonConvert.DeserializeObject<UploadCancellationMessage>(json);
                    if (uploadCancellationMessage == null)
                    {
                        return;
                    }

                    var uploadToCancel = _uploads.Get(uploadCancellationMessage.UploadID);
                    if (uploadToCancel != null)
                    {
                        uploadToCancel.Cancel();
                    }
                    break;

                case NetworkMessageType.FileIsNotAvailable:
                    if (author == null)
                    {
                        return;
                    }

                    var fileIsNotAvailableMessage = JsonConvert.DeserializeObject<FileIsNotAvailableMessage>(json);
                    if (fileIsNotAvailableMessage == null)
                    {
                        return;
                    }

                    var availableFile = _availableFiles.GetByFileID(fileIsNotAvailableMessage.FileID);
                    if (availableFile != null)
                    {
                        _availableFiles.Remove(fileIsNotAvailableMessage.FileID);
                    }
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
            if (_client.Server != null)
            {
                _tracker = new TrackerModel(_client.Server);
                OnPropertyChanged(nameof(TrackerAddress));
            }
        }

        private void OnTrackerConnected(object? sender, EventArgs e)
        {
            IsConnectedToTracker = true;
            OnPropertyChanged(nameof(TrackerAddress));

            _tracker?.SendIntroductionMessage(ID, Nickname);

            ExpectedTracker = _client.ExpectedServer;
            TrackerConnectionStatus = TrackerConnectionStatusType.Connected;

            if (WindowState == WindowState.Minimized &&
                _tracker != null)
            {
                Notify($"Tracker {_tracker.EndPoint} has connected",
                    $"Tracker is ready to work! 😊",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }

            var resetTimer = new Timer();
            resetTimer.Interval = 5000;
            resetTimer.Elapsed += (s, e) =>
            {
                resetTimer.Stop();
                TrackerConnectionStatus = TrackerConnectionStatusType.None;
            };
            resetTimer.Start();
        }

        private void OnTrackerRemoved(object? sender, ServerDisconnectedEventArgs e)
        {
            ExpectedTracker = e.ServerEndPoint;
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

                    _tracker?.PrintError($"Unrecognized command: '{commandErrorMessage.WrongCommand} {commandErrorMessage.Argument}'");

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

                    _tracker?.PrintError(string.Format("User not found, unknown ID or nickname: {0}", userNotFoundErrorMessage.UserInfo));

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

                    var formattedTime = timeResponseMessage.Time.ConvertTime();
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
                    var listOfUsersWithDesiredNickname = JsonConvert.DeserializeObject<ListOfUsersWithDesiredNicknameMessage>(json);
                    if (listOfUsersWithDesiredNickname == null)
                    {
                        return;
                    }

                    var usersListResponse = listOfUsersWithDesiredNickname.Users;
                    var positionOfUserWithSameID = usersListResponse.FindIndex(user => user.ID == ID);
                    if (positionOfUserWithSameID != -1)
                    {
                        usersListResponse.RemoveAt(positionOfUserWithSameID);
                    }

                    if (usersListResponse.Count == 0)
                    {
                        _tracker?.PrintError($"Tracker couldn't find any users with such ID or nickname: {listOfUsersWithDesiredNickname.NicknameQuery}");

                        return;
                    }

                    if (usersListResponse.Count == 1)
                    {
                        CurrentMessage = $"connect {usersListResponse[0].ID}";
                        SendMessage();
                    }
                    else
                    {
                        _tracker?.PrintListOfUsers(usersListResponse);
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
            ExpectedTracker = _client.ExpectedServer;
            TrackerConnectionStatus = TrackerConnectionStatusType.FailedToConnect;
        }

        private void OnUploadAdded(object? sender, UploadEventArgs e)
        {
            OnPropertyChanged(nameof(Uploads));
        }

        private void OnUploadRemoved(object? sender, UploadEventArgs e)
        {
            OnPropertyChanged(nameof(Uploads));
        }

        private void OnSharedFileAdded(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SharedFiles));
        }

        private void OnSharedFileRemoved(object? sender, SharedFileEventArgs e)
        {
            OnPropertyChanged(nameof(SharedFiles));
            _uploads.CancelAllUploadsOfFile(e.SharedFile.Hash);
            _connectedUsers.SendFileIsNotAvailableMessage(e.SharedFile.ID);
        }

        private void OnSharedFileHashCalculated(object? sender, EventArgs e)
        {
            Debug.WriteLine("(OnSharedFileHashCalculated)");
        }

        private void OnSharedFileError(object? sender, SharedFileEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show($"Error: can't load and send file {e.SharedFile.FilePath}",
                    "File sharing error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }));
        }

        private void OnDownloadsListUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Downloads));
        }

        private void OnDownloadFinished(object? sender, DownloadFinishedEventArgs e)
        {
            Notify("Download is finished",
                $"Download of file {e.DownloadedFileName} has finished!",
                1500,
                System.Windows.Forms.ToolTipIcon.Info);
        }

        private void OnAvailableFileAdded(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AvailableFiles));
        }

        private void OnAvailableFileRemoved(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AvailableFiles));
        }
        #endregion

        #region Command implementations
        private void ConnectToTracker()
        {
            var inputBox = new InputBoxWindow();
            var result = inputBox.AskServerAddressAndPort("Connection to Tracker",
                $"Enter IPv4 address of Tracker (example: {_defaultTrackerIPAddress}).\n" +
                    $"Also you can specify port (example: {_defaultTrackerIPAddress}:{_defaultTrackerPort})",
                new IPEndPoint(_defaultTrackerIPAddress, _defaultTrackerPort),
                out IPEndPoint? address);

            if (result &&
                address != null)
            {
                if (_client.IsConnectedToServer(address))
                {
                    MessageBox.Show(
                        "Already connected to this tracker",
                        "Tracker connection warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    _client.ConnectToServer(address);

                    TrackerConnectionStatus = TrackerConnectionStatusType.TryingToConnect;
                    ExpectedTracker = _client.ExpectedServer;
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

        private async Task SelectImageToSend()
        {
            if (FileSelector.TrySelectFile("Select a picture to send", Constants.ImageFilter, out var path) &&
                FileSelector.TryGetFileSize(path, out var size))
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

        private async Task SelectFileToSend()
        {
            if (FileSelector.TrySelectFile("Select a file to send", Constants.AllFilesFilter, out var path))
            {
                await SendFile(path);
            }
        }

        private async Task SendMediaFromFileDrop(FilesDroppedEventArgs? args)
        {
            if (args == null)
            {
                return;
            }

            foreach (var mediaPath in args.FilesPath)
            {
                var mediaExtension = Path.GetExtension(mediaPath);

                if (Constants.AllowedImageExtensions.Contains(mediaExtension) &&
                    FileSelector.TryGetFileSize(mediaPath, out var size))
                {
                    if (size == 0)
                    {
                        MessageBox.Show($"Picture {Path.GetFileName(mediaPath)} is empty!",
                            "Picture load error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        break;
                    }
                    else
                    if (size > Constants.MaxPictureSize)
                    {
                        var bytesConverter = new Converters.BytesToMegabytesConverter();
                        MessageBox.Show($"Picture {Path.GetFileName(mediaPath)} is too big.\n" +
                            $"Maximum size: {bytesConverter.ConvertSize(Constants.MaxPictureSize)}, " +
                            $"size of selected image: {bytesConverter.ConvertSize(size)}",
                            "Picture load error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        break;
                    }

                    await SendImage(mediaPath);
                }
                else
                {
                    await SendFile(mediaPath);
                }
            }
        }

        private async Task SendFile(string path)
        {
            Debug.WriteLine($"(SendFile) Sending file: {path}");

            if (SelectedPeer is UserModel user)
            {
                var sharedFile = await _sharedFiles.AddFile(path);
                if (sharedFile != null)
                {
                    var fileInfo = new SharedFileInfo(sharedFile);
                    user.SendFileMessage(fileInfo);
                }
                else
                {
                    user.PrintError($"(Error) Can't load file {path}.");
                }
            }
            else
            if (SelectedPeer is TrackerModel tracker)
            {
                tracker.PrintError("You can't send files to the tracker.");
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
                tracker.PrintError("You can't send images to the tracker.");
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
                if (CurrentMessage.TryParseCommand(out var command, out var argument))
                {
                    if (command == "help")
                    {
                        tracker.PrintHelp();
                    }
                    else
                    if (command == "connect" &&
                        argument == ID)
                    {
                        tracker.PrintError("Error: you can't connect to youself.");
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

        private void DownloadFile(AvailableFile? file)
        {
            if (file == null)
            {
                return;
            }

            if (!file.IsAvailable)
            {
                MessageBox.Show($"File {file.Name} from peer {file.Server.Nickname} ({file.Server.UserID}) is not available!",
                    "Download error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (!(SelectedPeer is UserModel))
            {
                MessageBox.Show($"File {file.Name} is unreachable - unknown file source.",
                    "Download error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            var fileExtension = Path.GetExtension(file.Name);

            var saveFileDialog = new SaveFileDialog()
            {
                FileName = fileName,
                DefaultExt = fileExtension,
                ValidateNames = true,
                Filter = fileExtension.GetAppropriateFileFilter()
            };

            var dialogResult = saveFileDialog.ShowDialog();
            if (!dialogResult.HasValue ||
                !dialogResult.Value)
            {
                return;
            }

            var path = saveFileDialog.FileName;
            Debug.WriteLine($"(DownloadFile) Path: {path}");

            var newDownload = new Download(file, path);

            var duplicateDownload = _downloads.GetDownloadWithSamePath(newDownload.FilePath);
            if (duplicateDownload != null)
            {
                var confirmDownloadRestart = MessageBox.Show($"File '{file.Name}' is already downloading! Do you want to restart download of this file?",
                    "Restart Download Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (confirmDownloadRestart == MessageBoxResult.Yes)
                {
                    duplicateDownload.Cancel();
                    PrepareForFileReceiving(newDownload);
                }

                return;
            }

            if (File.Exists(newDownload.FilePath))
            {
                var confirmDownloadRepeat = MessageBox.Show($"File '{file.Name}' already exists. Do you want to download this file again?",
                    "Download Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (confirmDownloadRepeat == MessageBoxResult.Yes)
                {
                    PrepareForFileReceiving(newDownload);
                }

                return;
            }

            PrepareForFileReceiving(newDownload);
        }

        private void CancelDownload(Download? download)
        {
            if (download == null)
            {
                return;
            }

            var confirmDownloadCancellation = MessageBox.Show($"Do you want to cancel the download of the file '{download.Name}'?",
                "Cancel Download Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmDownloadCancellation == MessageBoxResult.Yes)
            {
                download.Cancel();
            }
        }

        private void OpenFileInFolder(Download? download)
        {
            if (download == null)
            {
                return;
            }

            if (!File.Exists(download.FilePath))
            {
                MessageBox.Show($"File '{download.Name}' was removed or deleted.",
                    "File not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var argument = $"/select, \"{download.FilePath}\"";
            Process.Start("explorer.exe", argument);
        }

        private void RemoveSharedFile(SharedFile? sharedFile)
        {
            if (sharedFile == null)
            {
                return;
            }

            var messageBoxResult = MessageBox.Show($"Do you want to stop sharing this file: '{sharedFile.Name}'? " +
                $"Users' current downloads will be stopped.",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _sharedFiles.RemoveFile(sharedFile.ID);
            }
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

        private void ShowOwnProfilePicture(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                ShowImageFullScreen(ProfilePicture);
            }
        }

        private void ShowPeerProfilePicture(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released &&
                SelectedPeer is UserModel user)
            {
                ShowImageFullScreen(user.Picture);
            }
        }

        private void ShowPictureFromMessage(MouseEventArgs? e)
        {
            if (e == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released &&
                SelectedMessage is ImageMessageModel message)
            {
                ShowImageFullScreen(message.Image);
            }
        }

        private async Task ChangeProfilePicture()
        {
            if (FileSelector.TrySelectFile("Select new profile picture", Constants.ImageFilter, out var path))
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
            var haveGotSize = FileSelector.TryGetFileSize(path, out long size);
            if (!haveGotSize)
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

                var resetTimer = new Timer();
                resetTimer.Interval = 2000;
                resetTimer.Elapsed += (s, e) =>
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
        #endregion

        #region Miscellaneous methods
        private void PrepareForFileReceiving(Download download)
        {
            if (_downloads.TryAddDownload(download))
            {
                download.SendFileRequest();

                Notify("New download",
                    $"File {download.Name} is now downloading!",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Info);
            }
            else
            {
                Notify("Download error",
                    $"Can't start the download of file {download.Name}",
                    1500,
                    System.Windows.Forms.ToolTipIcon.Error);
            }
        }

        private void StartSendingFileToPeer(UserModel destination, FileRequestMessage message)
        {
            var desiredFile = _sharedFiles.GetByID(message.FileID);
            if (desiredFile == null)
            {
                destination.SendFileRequestErrorMessage(message.NewDownloadID, message.FileName);

                return;
            }

            var upload = new Upload(message.NewDownloadID, desiredFile, destination);
            if (_uploads.TryAdd(upload))
            {
                upload.StartUpload();

                Debug.WriteLine($"(StartSendingFileToPeer) Upload {upload.ID} of file {desiredFile.Name} has started. " +
                    $"Segments count: {desiredFile.NumberOfSegments}");
            }
            else
            {
                destination.SendFileRequestErrorMessage(message.NewDownloadID, message.FileName);

                //starting the message box from separate (non-UI) thread
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Error: can't start the upload of file {desiredFile.FilePath}",
                        "Upload start error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }));
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

        public void SetSelectedUserModel(UserModel userModel)
        {
            if (ConnectedUsers.Contains(userModel))
            {
                SelectedPeer = userModel;
            }
            else
            {
                Debug.WriteLine("(SetSelectedUserModel) Can't select userModel: " + userModel.UserID);
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
        #endregion
    }

    #region Notifications and system tray
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
    #endregion
}
