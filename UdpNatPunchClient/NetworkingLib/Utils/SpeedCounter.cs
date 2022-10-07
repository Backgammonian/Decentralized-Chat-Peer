using System;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;

namespace NetworkingLib.Utils
{
    public sealed class SpeedCounter : IDisposable
    {
        private const int _speedValuesInitialQueueCount = 20;

        private readonly Timer _timer;
        private readonly Stopwatch _stopwatch;
        private readonly Queue<double> _speedValues;
        private long _oldAmountOfBytes, _newAmountOfBytes;
        private DateTime _oldTimeStamp, _newTimeStamp;
        private bool _isDisposed;

        public SpeedCounter()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _speedValues = new Queue<double>();
            for (int i = 0; i < _speedValuesInitialQueueCount; i++)
            {
                _speedValues.Enqueue(0);
            }

            _timer = new Timer();
            _timer.Interval = 1000 / NetworkingConstants.SpeedTimerFrequency;
            _timer.Elapsed += OnTimerTick;
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

        private void OnTimerTick(object sender, ElapsedEventArgs e)
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

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
