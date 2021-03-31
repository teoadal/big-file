using System;
using System.Globalization;
using System.IO;

namespace BigFile.Extensions
{
    internal static class StreamWriterExtensions
    {
        private static readonly CultureInfo CurrentCulture = CultureInfo.CurrentCulture;

        public static void WriteLong(this StreamWriter writer, long value)
        {
            Span<char> intChars = stackalloc char[Constants.NumberLength];
            if (!value.TryFormat(intChars, out var written, ReadOnlySpan<char>.Empty, CurrentCulture))
            {
                throw new InvalidOperationException($"Can't format {value}");
            }

            writer.Write(intChars.Slice(0, written));
        }
    }
}