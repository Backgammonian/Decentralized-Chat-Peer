using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking.Utils;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class MessageModel : ObservableObject
    {
        private DeliveryState _deliveryState;

        public MessageModel(string authorID, string content)
        {
            MessageID = RandomGenerator.GetRandomString(20);
            AuthorID = authorID;
            Content = content;
            Time = DateTime.Now;
            Direction = MessageDirection.Outcoming;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public MessageModel(TextMessageToPeer messageFromOutside)
        {
            MessageID = messageFromOutside.MessageID;
            AuthorID = messageFromOutside.AuthorID;
            Content = messageFromOutside.Content;
            Time = DateTime.Now;
            Direction = MessageDirection.Incoming;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public MessageModel(string input, MessageDirection direction)
        {
            MessageID = RandomGenerator.GetRandomString(20);
            AuthorID = string.Empty;
            Content = input;
            Time = DateTime.Now;
            Direction = direction;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public string MessageID { get; }
        public string AuthorID { get; }
        public string Content { get; }
        public DateTime Time { get; }
        public MessageDirection Direction { get; }

        public DeliveryState DeliveryState
        {
            get => _deliveryState;
            private set => SetProperty(ref _deliveryState, value);
        }

        public void MarkAsDelivered()
        {
            if (Direction == MessageDirection.Incoming)
            {
                return;
            }

            if (DeliveryState == DeliveryState.ReadAndDelivered)
            {
                return;
            }

            DeliveryState = DeliveryState.Delivered;
        }

        public void MarkAsReadAndDelivered()
        {
            if (Direction == MessageDirection.Incoming)
            {
                return;
            }

            DeliveryState = DeliveryState.ReadAndDelivered;
        }
    }
}
