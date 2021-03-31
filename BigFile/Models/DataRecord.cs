using System;
using System.Diagnostics;

namespace BigFile.Models
{
    [DebuggerDisplay("{Number}. {Value}")]
    public readonly ref struct DataRecord
    {
        public readonly long Number;
        public readonly ReadOnlySpan<char> Value;

        public DataRecord(in long number, ReadOnlySpan<char> value)
        {
            Number = number;
            Value = value;
        }

        public DataRecord Copy(char[] buffer)
        {
            var value = Value;
            for (var i = 0; i < value.Length; i++)
            {
                buffer[i] = value[i];
            }

            return new DataRecord(Number, buffer.AsSpan(0, Value.Length));
        }

        public int CompareTo(DataRecord other)
        {
            var valueCompare = Value.CompareTo(other.Value, Constants.ValueComparison);

            return valueCompare == 0
                ? Number.CompareTo(other.Number)
                : valueCompare;
        }

        public void Deconstruct(out long number, out ReadOnlySpan<char> value)
        {
            number = Number;
            value = Value;
        }
    }
}