using System.IO;
using System.Text;
using BigFile.Extensions;
using BigFile.Models;

namespace BigFile.Sorter
{
    internal readonly ref struct DataRecordReader
    {
        public long Length => _reader.BaseStream.Length;

        public long Position => _reader.BaseStream.Position;

        private readonly char[] _buffer;
        private readonly StreamReader _reader;

        public DataRecordReader(string filePath)
        {
            _buffer = new char[Constants.ValueMaxLength];
            _reader = new StreamReader(filePath, Encoding.UTF8, false, Constants.StreamReaderBuffer);
        }

        public void Dispose() => _reader.Dispose();

        public Enumerator GetEnumerator() => new(_reader, _buffer);

        public ref struct Enumerator
        {
            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public DataRecord Current => _current;

            private DataRecord _current;
            private readonly char[] _buffer;
            private readonly StreamReader _reader;

            private int _line;

            public Enumerator(StreamReader reader, char[] buffer)
            {
                _current = default;
                _reader = reader;
                _buffer = buffer;

                _line = 1;
            }

            public bool MoveNext()
            {
                if (_reader.EndOfStream) return false;

                _current = new DataRecord(
                    _reader.ReadLong(_buffer, _line), // first for reuse buffer 
                    _reader.ReadValueSpan(_buffer, _line));

                _line++;

                return true;
            }
        }
    }
}