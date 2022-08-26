using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking.Utils;
using Networking.Messages;

namespace UdpNatPunchClient.Models
{
    public class BaseMessageModel : ObservableObject
    {
        protected DeliveryState _deliveryState;

        public BaseMessageModel(string authorID)
        {
            MessageID = RandomGenerator.GetRandomString(20);
            AuthorID = authorID;
            Time = DateTime.Now;
            Direction = MessageDirection.Outgoing;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public BaseMessageModel(TextMessageToPeer messageFromOutside)
        {
            MessageID = messageFromOutside.MessageID;
            AuthorID = messageFromOutside.AuthorID;
            Time = DateTime.Now;
            Direction = MessageDirection.Incoming;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public BaseMessageModel(MessageDirection direction)
        {
            MessageID = RandomGenerator.GetRandomString(20);
            AuthorID = RandomGenerator.GetRandomString(40);
            Time = DateTime.Now;
            Direction = direction;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public string MessageID { get; }
        public string AuthorID { get; }
        public DateTime Time { get; }
        public MessageDirection Direction { get; }

        public DeliveryState DeliveryState
        {
            get => _deliveryState;
            protected set => SetProperty(ref _deliveryState, value);
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
