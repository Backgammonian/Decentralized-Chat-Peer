﻿using System;

namespace NetworkingLib.Messages
{
    public sealed class TimeResponseMessage : BaseMessage
    {
        public TimeResponseMessage()
        {
            Type = NetworkMessageType.TimeResponse;
            Time = DateTime.Now;
        }

        public DateTime Time { get; set; }
    }
}
