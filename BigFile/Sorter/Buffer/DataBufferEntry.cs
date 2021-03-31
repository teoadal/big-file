using System;

namespace BigFile.Sorter.Buffer
{
    internal struct DataBufferEntry : IComparable<DataBufferEntry>
    {
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public int NumbersCount => _length;

        public uint HashCode;
        public int Next;
        public ReadOnlyMemory<char> Key;

        private long[]? _items;
        private int _length;

        public void Add(long number)
        {
            long[] items = _items ?? new long[16];

            if (items.Length == _length)
            {
                Array.Resize(ref items, items.Length * 2);
            }

            items[_length++] = number;
            _items = items;
        }

        public readonly bool Contains(long number)
        {
            if (_items == null) return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _items.Length; i++)
            {
                if (i == _length) break;
                if (_items[i] == number)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _length = 0;
            _items = null;
        }

        public readonly int CompareTo(DataBufferEntry other)
        {
            return Key.Span.CompareTo(other.Key.Span, Constants.ValueComparison);
        }

        public readonly Enumerator GetEnumerator() => new(_length, _items ?? Array.Empty<long>());

        public readonly bool Equals(in uint hash, in ReadOnlySpan<char> key)
        {
            return HashCode == hash && Key.Span.Equals(key, Constants.ValueComparison);
        }

        public readonly void Sort()
        {
            if (_items == null) return;
            Array.Sort(_items, 0, _length);
        }

        public ref struct Enumerator
        {
            public ref long Current => ref _items[_index];

            private readonly long[] _items;
            private readonly int _length;

            private int _index;

            public Enumerator(int length, long[] items)
            {
                _length = length;
                _items = items;

                _index = -1;
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _length;
            }
        }
    }
}