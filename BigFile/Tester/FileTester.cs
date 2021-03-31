using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using BigFile.Models;
using BigFile.Sorter;
using BigFile.Sorter.Buffer;

namespace BigFile.Tester
{
    public sealed class FileTester
    {
        private readonly TextWriter _consoleWriter;
        private readonly TestOptions _options;

        public FileTester(TestOptions options)
        {
            _consoleWriter = Console.Out;
            _options = options;
        }

        public void Test()
        {
            if (_options.CompareWithSource) CompareWithSource();

            EnsureSorted();
        }

        private void CompareWithSource()
        {
            var timer = Stopwatch.StartNew();
            var sortedData = new Dictionary<string, List<long>>();
            using (var sortedReader = new DataRecordReader(_options.Sorted))
            {
                foreach (var sortedRecord in sortedReader)
                {
                    var key = new string(sortedRecord.Value);
                    if (!sortedData.TryGetValue(key, out var collection))
                    {
                        collection = new List<long>();
                        sortedData.Add(key, collection);
                    }

                    collection.Add(sortedRecord.Number);
                }
            }

            Console.WriteLine("Read sorted records complete");

            using (var sourceReader = new DataRecordReader(_options.Source))
            {
                foreach (var (expectedNumber, value) in sourceReader)
                {
                    var expectedKey = new string(value);
                    if (!sortedData.TryGetValue(expectedKey, out var actualNumbers))
                    {
                        Console.WriteLine($"Not found value '{expectedKey}' in sorted file");
                        return;
                    }

                    if (actualNumbers.Contains(expectedNumber)) continue;

                    Console.WriteLine($"Not found number {expectedNumber} of value '{expectedKey}' in sorted file");
                    return;
                }
            }
            Console.WriteLine($"All lines from source file exists in sorted file");
            Console.WriteLine($"Compare complete at {timer.Elapsed}");
        }

        private void EnsureSorted()
        {
            var timer = Stopwatch.StartNew();
            using var reader = new DataRecordReader(_options.Sorted);

            var charBuffer = new char[Constants.ValueMaxLength];
            var counter = new Counter();
            var fileLength = reader.Length;
            var readerPositionPrev = 0L;

            var dataRecordPrev = new DataRecord(long.MinValue, ReadOnlySpan<char>.Empty);
            foreach (var dataRecord in reader)
            {
                if (dataRecordPrev.CompareTo(dataRecord) == 1)
                {
                    Console.WriteLine($"Not sorted data in line {counter.LinesCount}");
                    return;
                }

                var readerPosition = reader.Position;
                if (readerPosition - readerPositionPrev > Constants.HundredMegabytes)
                {
                    ShowProgress(readerPosition, fileLength, timer.Elapsed.TotalSeconds);
                    readerPositionPrev = readerPosition;
                }

                counter.SetMinMax(dataRecord.Number);
                counter.IncrementLineCount();

                dataRecordPrev = dataRecord.Copy(charBuffer);
            }

            Console.WriteLine();
            Console.WriteLine("Data is really sorted");
            Console.WriteLine($"Numbers between {counter.MinNumber} and {counter.MaxNumber}");
            Console.WriteLine($"Lines count {counter.LinesCount}");
        }

        private void ShowProgress(long readerPosition, long fileSize, double elapsedSeconds)
        {
            var readMegabytes = Math.Round(readerPosition / Constants.Megabyte);

            Console.CursorLeft = 0;

            var writer = _consoleWriter;
            writer.Write("Progress ");
            writer.Write(Math.Round((double) readerPosition / fileSize * 100d));
            writer.Write("% (total ");
            writer.Write(readMegabytes);
            writer.Write(" MB, per second ");
            writer.Write(Math.Round(readMegabytes / elapsedSeconds));
            writer.Write(" MB)...");
        }

        private ref struct Counter
        {
            public long LinesCount { get; private set; }

            public long MinNumber { get; private set; }

            public long MaxNumber { get; private set; }

            public void IncrementLineCount() => LinesCount++;

            public void SetMinMax(long number)
            {
                MaxNumber = Math.Max(MaxNumber, number);
                MinNumber = Math.Min(MinNumber, number);
            }
        }
    }
}