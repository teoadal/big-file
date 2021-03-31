using System;
using System.IO;

namespace BigFile.Extensions
{
    internal static class StreamReaderExtensions
    {
        public static long ReadLong(this StreamReader reader, char[] buffer, int? line = null)
        {
            var numberLength = 0;
            for (; numberLength < buffer.Length; numberLength++)
            {
                var ch = reader.Read();
                if (ch == -1 || ch == 46) break; // 46 is .

                buffer[numberLength] = (char) ch;
            }

            reader.Skip(32); // whitespace

            var number = buffer.AsSpan(0, numberLength);
            return long.TryParse(number, out var value)
                ? value
                : throw new InvalidDataException($"String '{new string(number)}' isn't a number in line {line}");
        }

        public static ReadOnlyMemory<char> ReadValueMemory(this StreamReader reader, char[] buffer, int? line = null)
        {
            var stringLength = 0;
            for (; stringLength < buffer.Length; stringLength++)
            {
                var ch = reader.Read();
                if (ch == -1 || ch == 13) break; // 13 is \r

                buffer[stringLength] = (char) ch;
            }

            reader.Skip(10, line); // \n

            return buffer.AsMemory(0, stringLength);
        }

        public static ReadOnlySpan<char> ReadValueSpan(this StreamReader reader, char[] buffer, int? line = null)
        {
            var stringLength = 0;
            for (; stringLength < buffer.Length; stringLength++)
            {
                var ch = reader.Read();
                if (ch == -1 || ch == 13) break; // 13 is \r

                buffer[stringLength] = (char) ch;
            }

            reader.Skip(10, line); // \n

            return buffer.AsSpan(0, stringLength);
        }

        public static void Skip(this StreamReader reader, int expected, int? line = null)
        {
            var actual = reader.Read();
            if (actual == -1) return; // EOF
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Expected '{(char) expected}', but '{(char) actual}' found in line {line}");
            }
        }
    }
}