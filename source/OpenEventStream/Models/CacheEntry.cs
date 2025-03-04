using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OpenEventStream.Abstractions;
using OpenEventStream.Services;

namespace OpenEventStream.Models
{
    public sealed class CacheEntry<T> : INotifyPropertyChanged
    {
        private long _lastUsed;
        private long _lastUpdated;
        private int _accessCount;
        private T? _value;
        private readonly object _key;
        private readonly Func<T> _valueFactory;
        private readonly ITimestampProvider _timestampProvider;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CacheEntry(object key, Func<T> valueFactory, ITimestampProvider? timestampProvider = null)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            _timestampProvider = timestampProvider ?? new SystemUtcTicks();
            _lastUsed = _timestampProvider.Ticks;
        }

        public DateTime LastUsed => new DateTime(_lastUsed);

        public DateTime LastUpdated => new DateTime(_lastUsed);

        public int AccessCount => _accessCount;

        public object Key => _key;

        public T? Value
        {
            get
            {
                _accessCount++;
                _lastUsed = _timestampProvider.Ticks;
                OnPropertyChanged(nameof(LastUsed));
                lock (_key)
                {
                    return _value;
                }
            }
        }

        public bool TryUpdate([MaybeNullWhen(false)] out T value, TimeSpan? slidingExpiration = null)
        {
            var slidingLimit = _timestampProvider.Ticks - slidingExpiration?.Ticks ?? 0;
            if (slidingLimit > _lastUpdated)
            {
                _lastUpdated = _timestampProvider.Ticks;
                OnPropertyChanged(nameof(LastUpdated));
                value = _valueFactory();
                lock (_key)
                {
                    _value = value;
                }
                OnPropertyChanged(nameof(Value));
                return true;
            }
            value = default;
            return false;
        }

        public bool TryDispose(TimeSpan? slidingExpiration = null)
        {
            var slidingLimit = _timestampProvider.Ticks - slidingExpiration?.Ticks ?? 0;
            if (slidingLimit > _lastUsed)
            {
                lock (_key)
                {
                    _value = default;
                }
                OnPropertyChanged(nameof(Value));
                return true;
            }
            return false;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
