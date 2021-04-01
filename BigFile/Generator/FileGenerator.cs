using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using BigFile.Extensions;
using BigFile.Generator.Chars;

namespace BigFile.Generator
{
    public sealed class FileGenerator
    {
        private readonly int _averageLineSize;

        private readonly ICharGenerator _charGenerator;
        private readonly char[] _charBuffer;
        private readonly TextWriter _consoleWriter;
        private readonly GenerateOptions _options;
        private readonly Random _random;

        private int _repeatPosition;
        private long _repeatsCount;

        public FileGenerator(GenerateOptions options)
        {
            var randomSeed = options.Seed ?? Environment.TickCount;

            _charBuffer = new char[options.MaxStringLength];
            _charGenerator = options.WellGenerator
                ? new DefaultCharGenerator(randomSeed)
                : new EasyCharGenerator(randomSeed);
            _consoleWriter = Console.Out;
            _options = options;
            _random = new Random(randomSeed);

            _averageLineSize = Constants.NumberLength +
                               Constants.NumberPostfix.Length +
                               (int) ((options.MaxStringLength - options.MinStringLength) / 1.5);
        }

        public void Generate()
        {
            var timer = Stopwatch.StartNew();
            var repeats = GenerateRepeats();

            var fileSize = _options.Size;
            var writer = new StreamWriter(_options.Output, false, Encoding.UTF8, ushort.MaxValue);
            var writerFileStream = writer.BaseStream;
            var writtenBytes = 0L;

            const int blockSize = 100;

            var currentNumber = _options.FromNumber;
            var firstBlock = true;
            var writtenBytesPrev = 0L;
            while (writtenBytes < fileSize)
            {
                if (writtenBytes - writtenBytesPrev > Constants.HundredMegabytes)
                {
                    ShowProgress(writtenBytes, fileSize, timer.Elapsed.TotalSeconds);
                    writtenBytesPrev = writtenBytes;
                }

                if (firstBlock) firstBlock = false;
                else writer.WriteLine();

                WriteBlock(writer, currentNumber, blockSize, repeats);
                currentNumber += blockSize;
                writtenBytes = writerFileStream.Length;
            }

            EnsureRepeatsExists(writer, repeats);

            writer.Flush();
            writer.Dispose();

            ShowProgress(writtenBytes, fileSize, timer.Elapsed.TotalSeconds);

            Console.WriteLine();
            Console.WriteLine($"Created {_repeatsCount} repeats");
        }

        private void EnsureRepeatsExists(StreamWriter writer, string[] repeats)
        {
            if (_repeatsCount >= repeats.Length && _repeatsCount >= 10) return;

            writer.WriteLine();
            for (var i = 0; i < 10; i++)
            {
                writer.WriteLong(_random.Next());
                writer.Write(Constants.NumberPostfix);
                WriteNextRepeat(writer, repeats);
                writer.WriteLine();
            }
        }

        private ReadOnlySpan<char> GenerateChars()
        {
            var length = _random.Next(_options.MinStringLength, _options.MaxStringLength);

            if (length == 0) return ReadOnlySpan<char>.Empty;

            var buffer = _charBuffer;
            for (var i = 0; i < length; i++)
            {
                buffer[i] = _charGenerator.GetNextChar();
            }

            return buffer.AsSpan(0, length);
        }

        private string[] GenerateRepeats()
        {
            var timer = Stopwatch.StartNew();

            var repeatsCount = _options.Size / 100 * _options.RepeatPercent / _averageLineSize;
            if (repeatsCount == 0) repeatsCount = 1;

            var repeats = new string[repeatsCount];

            for (var repeatIndex = 0; repeatIndex < repeats.Length; repeatIndex++)
            {
                var chars = GenerateChars();
                repeats[repeatIndex] = chars.Length == 0 ? string.Empty : new string(chars);
            }

            Console.WriteLine($"Generated {repeats.Length} repeats at {timer.Elapsed}");

            return repeats;
        }

        private void ShowProgress(long writtenBytes, long fileSize, double elapsedSeconds)
        {
            var progress = Math.Round((double) writtenBytes / fileSize * 100d);
            var writtenMegabytes = Math.Round(writtenBytes / Constants.Megabyte);

            Console.CursorLeft = 0;

            var writer = _consoleWriter;
            writer.Write("Progress ");
            writer.Write(progress > 100d ? 100d : progress);
            writer.Write("% (total ");
            writer.Write(writtenMegabytes);
            writer.Write(" MB, per second ");
            writer.Write(Math.Round(writtenMegabytes / elapsedSeconds));
            writer.Write(" MB)...");
        }

        private void WriteBlock(StreamWriter writer, long startId, int length, string[] repeats)
        {
            Span<long> numbers = stackalloc long[length];

            // fill
            for (var i = 0; i < numbers.Length; i++)
            {
                numbers[i] = startId++;
            }

            // shuffle
            var random = _random;
            for (var i = numbers.Length - 1; i > 0; i--)
            {
                var randomIndex = random.Next(0, i + 1);

                var current = numbers[randomIndex];
                var temp = numbers[i];

                numbers[i] = current;
                numbers[randomIndex] = temp;
            }

            // write
            var firstNumber = true;
            var repeatPercent = _options.RepeatPercent;
            foreach (var number in numbers)
            {
                if (firstNumber) firstNumber = false;
                else writer.WriteLine();

                writer.WriteLong(number);
                writer.Write(Constants.NumberPostfix);

                if (random.Next(0, 100) < repeatPercent)
                {
                    WriteNextRepeat(writer, repeats);
                }
                else
                {
                    writer.Write(GenerateChars());
                }
            }
        }

        private void WriteNextRepeat(StreamWriter writer, string[] repeats)
        {
            string repeat;
            if (_options.WellGenerator)
            {
                repeat = repeats[_random.Next(0, repeats.Length - 1)];
            }
            else
            {
                var position = _repeatPosition++;
                if (position == repeats.Length) position = 0;
                _repeatPosition = position;

                repeat = repeats[position];
            }

            writer.Write(repeat);
            _repeatsCount++;
        }
    }
}