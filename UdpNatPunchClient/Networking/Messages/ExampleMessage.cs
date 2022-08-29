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

        public string SomeString { get; set; }
        public List<int> SomeList { get; set; }
        public double[] SomeArray { get; set; }
    }
}
