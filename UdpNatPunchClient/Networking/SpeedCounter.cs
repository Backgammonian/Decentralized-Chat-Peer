using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Networking.Utils;

namespace Networking
{
    public sealed class SpeedCounter
    {
        private readonly DispatcherTimer _timer;
        private readonly Queue<double> _speedValues;
        private long _oldAmountOfBytes, _newAmountOfBytes;
        private long _currentAmountOfBytes;
        private DateTime _oldTimeStamp, _newTimeStamp;
        private double _speed;

        public SpeedCounter()
        {
            _speedValues = new Queue<double>();
            _timer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / NetworkingConstants.SpeedTimerFrequency);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public event EventHandler? Updated;

        public double Speed => _speed;
        public long Bytes => _currentAmountOfBytes;

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _oldAmountOfBytes = _newAmountOfBytes;
            _newAmountOfBytes = _currentAmountOfBytes;
            _oldTimeStamp = _newTimeStamp;
            _newTimeStamp = DateTime.Now;

            var value = (_newAmountOfBytes - _oldAmountOfBytes) / (_newTimeStamp - _oldTimeStamp).TotalSeconds;
            _speedValues.Enqueue(value);
            if (_speedValues.Count > 20)
            {
                _speedValues.Dequeue();
            }
            _speed = _speedValues.CalculateAverageValue();

            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void AddBytes(long newBytes)
        {
            _currentAmountOfBytes += newBytes;
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
