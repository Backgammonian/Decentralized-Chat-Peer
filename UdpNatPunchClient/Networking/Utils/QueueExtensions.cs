using System.Collections.Generic;

namespace Networking.Utils
{
    public static class QueueExtensions
    {
        public static double CalculateAverageValue(this Queue<double> queue)
        {
            var result = 0.0;
            foreach (var value in queue)
            {
                result += value;
            }

            return result / queue.Count;
        }
    }
}
