using System.Collections.Generic;

namespace Networking.Messages
{
    public sealed class ExampleMessage : BaseMessage
    {
        public ExampleMessage(string someString, List<int> someList, double[] someArray)
        {
            Type = NetworkMessageType.ExampleMessage;
            SomeString = someString;
            SomeList = someList;
            SomeArray = someArray;
        }

        public string SomeString { get; }
        public List<int> SomeList { get; }
        public double[] SomeArray { get; }
    }
}
