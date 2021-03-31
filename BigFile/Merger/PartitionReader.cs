using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BigFile.Extensions;

namespace BigFile.Merger
{
    [DebuggerDisplay("{" + nameof(_filePath) + "}")]
    internal sealed class PartitionReader : IComparable<PartitionReader>, IDisposable
    {
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public ReadOnlyMemory<char> Value => _value;

        public bool End { get; private set; }

        public long Length => _reader.BaseStream.Length;

        private readonly char[] _buffer;
        private readonly string _filePath;
        private readonly StreamReader _reader;

        private int _line;
        private ReadOnlyMemory<char> _value;

        public PartitionReader(string filePath)
        {
            _buffer = new char[Constants.ValueMaxLength];
            _filePath = filePath;
            _line = 1;
            _reader = new StreamReader(filePath, Encoding.UTF8, false, Constants.StreamReaderBuffer);

            ReadNextValue();
        }

        public int CompareTo(PartitionReader? other)
        {
            ref var currentRecord = ref _value;
            ref var otherRecord = ref other!._value;

            return currentRecord.Span.CompareTo(otherRecord.Span, Constants.ValueComparison);
        }

        public void DeleteSource() => File.Delete(_filePath);

        public IEnumerable<long> ReadValueNumbers()
        {
            return new Enumerator(_reader, _line++, _filePath);
        }

        public bool ReadNextValue()
        {
            if (_reader.EndOfStream)
            {
                _value = default;
                End = true;

                return false;
            }

            _value = _reader.ReadValueMemory(_buffer, _line);
            _line++;

            return true;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        private sealed class Enumerator : IEnumerable<long>, IEnumerator<long>
        {
            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public long Current => _current;

            private long _current;
            private bool _disposed;
            private readonly int _line;
            private readonly string _fileName;
            private readonly StreamReader _reader;

            public Enumerator(StreamReader reader, int line, string fileName)
            {
                _line = line;
                _fileName = fileName;
                _reader = reader;
            }

            public IEnumerator<long> GetEnumerator() => this;

            public bool MoveNext()
            {
                if (_disposed) return false;

                Span<char> buffer = stackalloc char[Constants.NumberLength];
                var reader = _reader;

                var numberLength = 0;
                for (; numberLength < buffer.Length; numberLength++)
                {
                    var ch = reader.Read();
                    if (ch == -1 || ch == 124) break; // 124 is |
                    if (ch == 13) // 13 is \r
                    {
                        reader.Skip(10, _line); // \n
                        _disposed = true;
                        break;
                    }

                    buffer[numberLength] = (char) ch;
                }

                if (numberLength == 0)
                {
                    _disposed = true;
                    return false;
                }

                var number = buffer.Slice(0, numberLength);
                _current = long.TryParse(number, out var value)
                    ? value
                    : throw new InvalidOperationException($"String '{new string(number)}' isn't a number in line {_line} of file {_fileName}");
                
                return true;
            }

            public void Dispose()
            {
                _disposed = true;
            }

            #region Interfaces

            object IEnumerator.Current => Current;

            IEnumerator IEnumerable.GetEnumerator() => this;

            void IEnumerator.Reset()
            {
            }

            #endregion
        }
    }
}