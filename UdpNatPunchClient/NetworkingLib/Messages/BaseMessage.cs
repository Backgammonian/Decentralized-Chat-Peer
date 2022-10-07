using Newtonsoft.Json;
using LiteNetLib.Utils;

namespace NetworkingLib.Messages
{
    public abstract class BaseMessage
    {
        public NetworkMessageType Type { get; protected set; }

        public NetDataWriter GetContent()
        {
            var writer = new NetDataWriter();

            writer.Put((byte)Type);
            writer.Put(JsonConvert.SerializeObject(this));

            return writer;
        }
    }
}
