using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Net;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Meziantou.Framework.WPF.Collections;
using Networking;
using Networking.Messages;
using Networking.Utils;
using InputBox;
using UdpNatPunchClient.Models;

namespace UdpNatPunchClient
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly Client _client;
        private readonly Users _connectedUsers;
        private TrackerModel? _tracker;
        private bool _isConnectedToTracker;
        private string _currentMessage = string.Empty;
        private ConcurrentObservableCollection<MessageModel>? _messages;
        private PeerModel? _selectedPeer;
        private Action? _scrollMessageBoxToEnd;
        private bool _canSendMessage;
        private string _currentPlaceholder;

        private const string _userMessagePlaceholder = "Write a message to user...";
        private const string _trackerMessagePlaceholder = "Write a message to tracker...";
        private const string _noMessagePlaceholder = "---";

        public MainWindowViewModel()
        {
            ConnectToTrackerCommand = new RelayCommand(ConnectToTracker);
            SendMessageCommand = new RelayCommand(SendMessage);
            PutAsciiArtCommand = new RelayCommand<AsciiArtsType>(PutAsciiArt);
            SelectTrackerDialogCommand = new RelayCommand(SelectTrackerDialog);

            ID = RandomGenerator.GetRandomString(30);
            CurrentMessage = string.Empty;
            IsConnectedToTracker = false;
            SelectedPeer = null;
            CanSendMessage = false;

            TextArts = new ObservableCollection<AsciiArtsType>(Enum.GetValues(typeof(AsciiArtsType)).Cast<AsciiArtsType>());

            _client = new Client();
            _client.PeerAdded += OnPeerAdded;
            _client.PeerConnected += OnPeerConnected;
            _client.PeerRemoved += OnPeerRemoved;
            _client.MessageFromPeerReceived += OnMessageFromPeerReceived;
            _client.TrackerAdded += OnTrackerAdded;
            _client.TrackerConnected += OnTrackerConnected;
            _client.TrackerRemoved += OnTrackerRemoved;
            _client.MessageFromTrackerReceived += OnMessageFromTrackerReceived;

            _connectedUsers = new Users();
            _connectedUsers.UserAdded += OnConnectedPeerAdded;
            _connectedUsers.UserRemoved += OnConnectedPeerRemoved;

            _tracker = null;
        }

        public ICommand ConnectToTrackerCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand PutAsciiArtCommand { get; }
        public ICommand SelectTrackerDialogCommand { get; }

        public string ID { get; }
        public ObservableCollection<AsciiArtsType> TextArts { get; }
        public string TrackerAddress => _tracker == null ? "---" : _tracker.EndPoint;
        public ObservableCollection<UserModel> ConnectedUsers => new ObservableCollection<UserModel>(_connectedUsers.List.OrderBy(user => user.ConnectionTime));

        public bool IsConnectedToTracker
        {
            get => _isConnectedToTracker;
            private set => SetProperty(ref _isConnectedToTracker, value);
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        public string CurrentPlaceholder
        {
            get => _currentPlaceholder;
            set => SetProperty(ref _currentPlaceholder, value);
        }

        public bool CanSendMessage
        {
            get => _canSendMessage;
            private set => SetProperty(ref _canSendMessage, value);
        }

        public ConcurrentObservableCollection<MessageModel>? Messages
        {
            get => _messages;
            private set => SetProperty(ref _messages, value);
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

                    ScrollMessagesToEnd();

                    if (SelectedPeer is UserModel)
                    {
                        CurrentPlaceholder = _userMessagePlaceholder;
                    }
                    else
                    if (SelectedPeer is TrackerModel)
                    {
                        CurrentPlaceholder = _trackerMessagePlaceholder;
                    }
                }
                else
                {
                    CanSendMessage = false;
                    Messages = null;
                    CurrentPlaceholder = _noMessagePlaceholder;
                }
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

        public void StartApp()
        {
            _client.StartListening();
        }

        public void ShutdownApp()
        {
            _tracker?.Disconnect();
            _client.DisconnectAll();
            _client.Stop();
        }

        private void OnConnectedPeerAdded(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedUsers));
        }

        private void OnConnectedPeerRemoved(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedUsers));
        }

        private void OnPeerAdded(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine("(OnPeerAdded) " + e.Peer.Id);
        }

        private void OnPeerConnected(object? sender, EncryptedPeerEventArgs e)
        {
            Debug.WriteLine("(OnPeerConnected) " + e.Peer.Id);

            var introducePeerToPeerMessage = new IntroducePeerToPeerMessage(ID);
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

        private void OnMessageFromPeerReceived(object? sender, NetEventArgs e)
        {
            var source = e.EncryptedPeer;
            var type = e.Type;
            var json = e.Json;

            Debug.WriteLine("(OnMessageFromPeerReceived) source = " + source.EndPoint);
            Debug.WriteLine("(OnMessageFromPeerReceived) type = " + type);
            Debug.WriteLine("(OnMessageFromPeerReceived) json = " + json);

            var author = _connectedUsers.GetUserByPeer(source);

            switch (type)
            {
                case NetworkMessageType.IntroducePeerToPeer:
                    var introducePeerToPeerMessage = JsonConvert.DeserializeObject<IntroducePeerToPeerMessage>(json);
                    if (introducePeerToPeerMessage == null)
                    {
                        return;
                    }

                    _connectedUsers.Add(introducePeerToPeerMessage.ID, source);

                    var introducePeerToPeerResponseMessage = new IntroducePeerToPeerResponse(ID);
                    source.SendEncrypted(introducePeerToPeerResponseMessage);
                    break;

                case NetworkMessageType.IntroducePeerToPeerResponse:
                    var introducePeerToPeerResponse = JsonConvert.DeserializeObject<IntroducePeerToPeerResponse>(json);
                    if (introducePeerToPeerResponse == null)
                    {
                        return;
                    }

                    _connectedUsers.Add(introducePeerToPeerResponse.ID, source);
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

                        ScrollMessagesToEnd();
                    }
                    else
                    {
                        author.SendReceiptNotification(receivedMessage);
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
            if (_client.Tracker != null)
            {
                _tracker = new TrackerModel(_client.Tracker);

                OnPropertyChanged(nameof(TrackerAddress));
            }
        }

        private void OnTrackerConnected(object? sender, EventArgs e)
        {
            IsConnectedToTracker = true;
            _tracker?.SendIntroductionMessage(ID);

            OnPropertyChanged(nameof(TrackerAddress));
        }

        private void OnTrackerRemoved(object? sender, EventArgs e)
        {
            if (_tracker == SelectedPeer)
            {
                CanSendMessage = false;
                Messages = null;
            }

            _tracker = null;
            IsConnectedToTracker = false;

            OnPropertyChanged(nameof(TrackerAddress));
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
                    MessageBox.Show("Tracker already has user with such ID: " + ID, "Tracker connection error");
                    _tracker?.Disconnect();
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

                    _tracker?.PrintInfo(string.Format("Unrecognized command: '/{0} {1}'", commandErrorMessage.WrongCommand, commandErrorMessage.Argument));
                    break;

                case NetworkMessageType.UserConnectionResponse:
                    var userConnectionResponseMessage = JsonConvert.DeserializeObject<UserConnectionResponseMessage>(json);
                    if (userConnectionResponseMessage == null)
                    {
                        return;
                    }

                    if (IPEndPoint.TryParse(userConnectionResponseMessage.EndPointString, out var peerEndPoint))
                    {
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
                    break;

                case NetworkMessageType.PingResponse:
                    var pingResponseMessage = JsonConvert.DeserializeObject<PingResponseMessage>(json);
                    if (pingResponseMessage == null)
                    {
                        return;
                    }
                    
                    _tracker?.PrintInfo(string.Format("Pong!\n{0} ms, {1}", pingResponseMessage.Ping, pingResponseMessage.PacketLossPercent)); //TODOOOOOOOOOOOOOO
                    break;

                case NetworkMessageType.TimeResponse:
                    var timeResponseMessage = JsonConvert.DeserializeObject<TimeResponseMessage>(json);
                    if (timeResponseMessage == null)
                    {
                        return;
                    }

                    var converter = new Converters.DateTimeConverter();
                    var formattedTime = (string)converter.Convert(timeResponseMessage.Time, null, null, null);
                    _tracker?.PrintInfo(string.Format("Tracker's time: {0}", formattedTime));
                    break;
            }
        }

        private void ConnectToTracker()
        {
            var inputBox = new InputBoxUtils();
            if (!inputBox.AskServerAddressAndPort(out IPEndPoint? address) ||
                address == null)
            {
                MessageBox.Show(
                    "Tracker address is not valid! Try enter correct IP address and port (example: 10.0.8.100:55000)",
                    "Tracker address error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (_client.IsConnectedToTracker(address))
            {
                MessageBox.Show(
                    "Already connected to this tracker",
                    "Tracker connection warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            _client.ConnectToTracker(address);
        }

        private void SendMessage()
        {
            if (SelectedPeer == null)
            {
                return;
            }

            if (SelectedPeer is UserModel user)
            {
                user.SendTextMessage(new MessageModel(ID, CurrentMessage));
            }
            else
            if (SelectedPeer is TrackerModel tracker)
            {
                if (!TryParseCommand(CurrentMessage, out var command, out var argument))
                {
                    tracker.SendCommandMessage(command.ToLower(), argument);
                }
                else
                {
                    tracker.PrintInfo(string.Format("Not valid command input: {0}", CurrentMessage));
                }
            }
            
            CurrentMessage = string.Empty;
        }

        private bool TryParseCommand(string input, out string command, out string argument)
        {
            command = string.Empty;
            argument = string.Empty;

            if (input[0] != '/')
            {
                return false;
            }

            var spacePosition = input.IndexOf(' ');
            if (spacePosition == -1)
            {
                command = input.Substring(1, input.Length - 1);
            }
            else
            {
                command = input.Substring(1, spacePosition - 1);
                argument = input.Substring(spacePosition + 1, input.Length - spacePosition - 1);
            }

            return true;
        }

        private void ScrollMessagesToEnd()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _scrollMessageBoxToEnd?.Invoke();
            });
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

                case AsciiArtsType.Shishka:
                    CurrentMessage += AsciiArts.Shishka;
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

            ScrollMessagesToEnd();
        }

        private void SelectTrackerDialog()
        {
            if (_tracker != null)
            {
                SelectedPeer = _tracker;
            }
        }
    }
}
