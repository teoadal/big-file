using System;
using System.Collections;
using System.Collections.Generic;
using BigFile.Models;

namespace BigFile.Sorter.Buffer
{
    internal sealed class DataBuffer : IDisposable, IEnumerable<DataBufferEntry>
    {
        private char[] _charBuffer;
        private int _charBufferOffset;

        private int[] _buckets;
        private int _count;
        private readonly uint _capacity;
        private bool _disposed;
        private DataBufferEntry[] _entries;

        public DataBuffer(uint capacity = 1_500_000u)
        {
            _buckets = new int[capacity];
            _capacity = capacity;
            _entries = new DataBufferEntry[capacity];

            _charBuffer = new char[capacity / 2 * Constants.ValueMaxLength];
            _charBufferOffset = 0;
        }

        public void Clear()
        {
            if (_count == 0) return;

            _charBufferOffset = 0;
            _count = 0;

            foreach (var entry in _entries)
            {
                entry.Clear();
            }

            var capacity = (int) _capacity;
            Array.Clear(_buckets, 0, capacity);
            Array.Clear(_entries, 0, capacity);
        }

        public IEnumerator<DataBufferEntry> GetEnumerator()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataBuffer));
            return new Enumerator(this);
        }

        public void Dispose()
        {
            if (_disposed) return;

            Clear();

            _charBuffer = null!;
            _buckets = null!;
            _entries = null!;

            _disposed = true;
        }

        public bool TryAdd(DataRecord dataRecord)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataBuffer));

            var capacity = _capacity;
            var (number, key) = dataRecord;
            var bufferExcepted = _charBuffer.Length - _charBufferOffset;
            if (_count == capacity || bufferExcepted < key.Length)
            {
                return false;
            }

            var entries = _entries;
            var hashCode = (uint) string.GetHashCode(key, Constants.ValueComparison);

            var collisionCount = 0;
            ref var bucket = ref _buckets[hashCode % capacity];
            var i = bucket - 1;

            do
            {
                if ((uint) i >= (uint) entries.Length)
                {
                    break;
                }

                ref var existEntry = ref entries[i];
                if (existEntry.Equals(in hashCode, in key))
                {
                    existEntry.Add(number);
                    return true;
                }

                i = existEntry.Next;

                if (collisionCount >= _count) throw new InvalidOperationException("Many collisions");
                collisionCount++;
            } while (true);

            var count = _count;
            var index = count;
            _count = count + 1;

            ref var entry = ref entries[index];

            entry.HashCode = hashCode;
            entry.Next = bucket - 1;
            entry.Key = CreateMemory(key);
            entry.Add(number);
            bucket = index + 1;

            return true;
        }

        public bool Contains(ReadOnlySpan<char> key, long number)
        {
            var capacity = _capacity;
            var entries = _entries;
            var hashCode = (uint) string.GetHashCode(key, Constants.ValueComparison);

            var collisionCount = 0;
            ref var bucket = ref _buckets[hashCode % capacity];
            var i = bucket - 1;

            do
            {
                if ((uint) i >= (uint) entries.Length)
                {
                    break;
                }

                ref var existEntry = ref entries[i];
                if (existEntry.Equals(in hashCode, in key))
                {
                    return existEntry.Contains(number);
                }

                i = existEntry.Next;

                if (collisionCount >= _count) throw new InvalidOperationException("Many collisions");
                collisionCount++;
            } while (true);

            return false;
        }

        public override string ToString()
        {
            if (_disposed) return "Disposed";

            var chars = Math.Round(_charBufferOffset / (double) _charBuffer.Length * 100d, 1);
            var entries = Math.Round(_count / (double) _capacity * 100d, 1);
            return $"Chars {chars:0.0}%, Entries {entries:0.0}%";
        }

        private ReadOnlyMemory<char> CreateMemory(ReadOnlySpan<char> key)
        {
            var start = _charBufferOffset;
            var index = start;
            foreach (var ch in key)
            {
                _charBuffer[index++] = ch;
            }

            _charBufferOffset = index;
            return new ReadOnlyMemory<char>(_charBuffer, start, key.Length);
        }

        private struct Enumerator : IEnumerator<DataBufferEntry>
        {
            public DataBufferEntry Current => _current;

            private readonly int _count;
            private readonly DataBufferEntry[] _entries;

            private DataBufferEntry _current;
            private int _index;

            public Enumerator(DataBuffer dictionary)
            {
                _count = dictionary._count;
                _current = default;
                _entries = dictionary._entries;
                _index = 0;
            }

            public bool MoveNext()
            {
                while ((uint) _index < (uint) _count)
                {
                    ref var entry = ref _entries[_index++];
                    // ReSharper disable once InvertIf
                    if (entry.Next >= -1)
                    {
                        _current = entry;
                        return true;
                    }
                }

                _current = default;
                _index = _count + 1;
                return false;
            }

            #region Intefaces

            void IEnumerator.Reset()
            {
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator.Current => Current;

            #endregion
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}