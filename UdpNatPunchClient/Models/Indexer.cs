namespace UdpNatPunchClient.Models
{
    public sealed class Indexer
    {
        private long _currentIndex;

        public Indexer()
        {
            _currentIndex = 0;
        }

        public long GetNewIndex()
        {
            _currentIndex += 1;
            _currentIndex = _currentIndex == long.MaxValue ? 0 : _currentIndex;
            return _currentIndex;
        }
    }
}
