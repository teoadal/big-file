using System;
using System.Diagnostics;
using System.IO;
using BigFile.Sorter.Strategies;

namespace BigFile.Sorter
{
    public sealed class FileSorter
    {
        private readonly TextWriter _consoleWriter;
        private readonly SortOptions _options;

        public FileSorter(SortOptions options)
        {
            _consoleWriter = Console.Out;
            _options = options;
        }

        public void Sort()
        {
            ISortStrategy sortStrategy;
            using (var reader = new DataRecordReader(_options.Input))
            {
                var fileLength = reader.Length;

                sortStrategy = fileLength < 1_000_000_000 // 1GB
                    ? (ISortStrategy) new SmallFileStrategy()
                    : new DefaultStrategy();

                SplitData(sortStrategy, reader, fileLength);
            }

            WriteResults(sortStrategy);
        }

        private void ShowProgress(long readerPosition, long fileSize, double elapsedSeconds)
        {
            var readMegabytes = Math.Round(readerPosition / Constants.Megabyte);

            Console.CursorLeft = 0;

            var writer = _consoleWriter;
            writer.Write("Splitting ");
            writer.Write(Math.Round((double) readerPosition / fileSize * 100d));
            writer.Write("% (total ");
            writer.Write(readMegabytes);
            writer.Write(" MB, per second ");
            writer.Write(Math.Round(readMegabytes / elapsedSeconds));
            writer.Write(" MB)...");
        }

        private void SplitData(ISortStrategy sortStrategy, DataRecordReader reader, long fileLength)
        {
            var timer = Stopwatch.StartNew();

            var readerPositionPrev = 0L;
            foreach (var dataRecord in reader)
            {
                var readerPosition = reader.Position;
                if (readerPosition - readerPositionPrev > Constants.HundredMegabytes)
                {
                    ShowProgress(readerPosition, fileLength, timer.Elapsed.TotalSeconds);
                    readerPositionPrev = readerPosition;
                }

                sortStrategy.Aggregate(dataRecord);
            }

            ShowProgress(reader.Position, fileLength, timer.Elapsed.TotalSeconds);

            Console.WriteLine();
            Console.WriteLine($"Splitting executed at {timer.Elapsed}");
        }

        private void WriteResults(ISortStrategy sortStrategy)
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine($"Write results to file {_options.Output}...");

            sortStrategy.WriteResult(_options.Output);

            Console.WriteLine($"Results written at {timer.Elapsed}");
        }
    }
}