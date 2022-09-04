using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Networking.Utils;

namespace UdpNatPunchClient.Models
{
    public class BaseMessageModel : ObservableObject
    {
        protected DeliveryState _deliveryState;

        public BaseMessageModel()
        {
            MessageID = RandomGenerator.GetRandomString(20);
            Time = DateTime.Now;
            Direction = MessageDirection.Outgoing;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public BaseMessageModel(string messageID)
        {
            MessageID = messageID;
            Time = DateTime.Now;
            Direction = MessageDirection.Incoming;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public BaseMessageModel(MessageDirection direction)
        {
            MessageID = RandomGenerator.GetRandomString(20);
            Time = DateTime.Now;
            Direction = direction;
            DeliveryState = DeliveryState.NotDelivered;
        }

        public string MessageID { get; }
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
