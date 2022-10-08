using System.Collections.Generic;

namespace NetworkingLib.Utils
{
    public static class QueueExtensions
    {
        public static double CalculateAverageValue(this Queue<double> queue)
        {
            if (queue.Count == 0)
            {
                return 0;
            }

            var result = 0.0;
            foreach (var value in queue)
            {
                result += value;
            }

            return result / queue.Count;
        }
    }
}
