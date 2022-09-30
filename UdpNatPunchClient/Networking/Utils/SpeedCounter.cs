using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace Networking.Utils
{
    public sealed class SpeedCounter
    {
        private const int _speedValuesInitialQueueCount = 20;

        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
        private readonly Queue<double> _speedValues;
        private long _oldAmountOfBytes, _newAmountOfBytes;
        private DateTime _oldTimeStamp, _newTimeStamp;

        public SpeedCounter()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _speedValues = new Queue<double>();
            for (int i = 0; i < _speedValuesInitialQueueCount; i++)
            {
                _speedValues.Enqueue(0);
            }

            _timer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / NetworkingConstants.SpeedTimerFrequency);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public event EventHandler? Updated;

        public double Speed { get; private set; }
        public double AverageSpeed { get; private set; }
        public long Bytes { get; private set; }

        private void PerformCalculations()
        {
            _oldAmountOfBytes = _newAmountOfBytes;
            _newAmountOfBytes = Bytes;
            _oldTimeStamp = _newTimeStamp;
            _newTimeStamp = DateTime.Now;

            var value = (_newAmountOfBytes - _oldAmountOfBytes) / (_newTimeStamp - _oldTimeStamp).TotalSeconds;
            _speedValues.Dequeue();
            _speedValues.Enqueue(value);
           
            Speed = _speedValues.CalculateAverageValue();

            var seconds = _stopwatch.Elapsed.Seconds > 0 ? _stopwatch.Elapsed.Seconds : 0.01;
            AverageSpeed = Bytes / Convert.ToDouble(seconds);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            PerformCalculations();
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void AddBytes(long newBytes)
        {
            Bytes += newBytes;
        }

        public void Stop()
        {
            Speed = 0;
            Updated?.Invoke(this, EventArgs.Empty);
            _timer.Stop();
        }
    }
}
