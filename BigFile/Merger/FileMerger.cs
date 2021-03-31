using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BigFile.Extensions;

namespace BigFile.Merger
{
    public sealed class FileMerger
    {
        private readonly TextWriter _consoleWriter;
        private readonly MergeOptions _options;

        public FileMerger(MergeOptions options)
        {
            _options = options;
            _consoleWriter = Console.Out;
        }

        public void Merge()
        {
            var timer = Stopwatch.StartNew();

            var availableReaders = BuildReaders();

            var writer = new StreamWriter(_options.Output, false, Encoding.UTF8, Constants.StreamWriterBuffer);
            var writerStream = writer.BaseStream;
            var writtenBytesPrev = 0L;

            var currentReaders = new List<PartitionReader>(availableReaders.Length);
            while (true)
            {
                var writtenBytes = writerStream.Length;
                if (writtenBytes - writtenBytesPrev > Constants.HundredMegabytes)
                {
                    ShowProgress(writtenBytes, timer.Elapsed.TotalSeconds);
                    writtenBytesPrev = writtenBytes;
                }

                PartitionReader? currentReader = null;
                foreach (var availableReader in availableReaders)
                {
                    if (availableReader.End) continue;

                    if (currentReader == null)
                    {
                        currentReader = availableReader;
                        currentReaders.Add(availableReader);
                        continue;
                    }

                    var compareResult = currentReader.CompareTo(availableReader);
                    switch (compareResult)
                    {
                        case -1:
                            continue;
                        case 0:
                            currentReaders.Add(availableReader);
                            continue;
                        case 1:
                            currentReader = availableReader;
                            currentReaders.Clear();
                            currentReaders.Add(availableReader);
                            break;
                    }
                }

                if (currentReader == null)
                {
                    ShowProgress(writtenBytes, timer.Elapsed.TotalSeconds);
                    break;
                }
                
                var sortedNumbers = currentReaders.Count == 1
                    ? currentReaders[0].ReadValueNumbers()
                    : currentReaders
                        .AsParallel()
                        .SelectMany(r => r.ReadValueNumbers())
                        .OrderBy(n => n);

                var currentValue = currentReader.Value.Span;
                foreach (var sortedNumber in sortedNumbers)
                {
                    writer.WriteLong(sortedNumber);
                    writer.Write(Constants.NumberPostfix);
                    writer.WriteLine(currentValue);
                }

                foreach (var reader in currentReaders)
                {
                    reader.ReadNextValue();
                }

                currentReaders.Clear();
            }

            writer.Flush();
            writer.Dispose();

            DisposeReaders(availableReaders);

            Console.WriteLine();
        }

        private PartitionReader[] BuildReaders()
        {
            var files = Directory.GetFiles(_options.Folder, _options.FileMask);

            var readers = new PartitionReader[files.Length];
            for (var i = files.Length - 1; i >= 0; i--)
            {
                readers[i] = new PartitionReader(files[i]);
            }

            return readers.ToArray();
        }

        private void DisposeReaders(PartitionReader[] readers)
        {
            var deletePartitionFile = _options.DeletePartitions;
            foreach (var reader in readers)
            {
                reader.Dispose();

                if (deletePartitionFile)
                {
                    reader.DeleteSource();
                }
            }

            Array.Clear(readers, 0, readers.Length);
        }

        private void ShowProgress(long readerPosition, double elapsedSeconds)
        {
            var readMegabytes = Math.Round(readerPosition / Constants.Megabyte);

            Console.CursorLeft = 0;

            var writer = _consoleWriter;
            writer.Write("Merged ");
            writer.Write(readMegabytes);
            writer.Write(" MB, per second ");
            writer.Write(Math.Round(readMegabytes / elapsedSeconds));
            writer.Write(" MB...");
        }
    }

    // public sealed class FileMerger
    // {
    //     private readonly TextWriter _consoleWriter;
    //     private readonly MergeOptions _options;
    //
    //     public FileMerger(MergeOptions options)
    //     {
    //         _options = options;
    //         _consoleWriter = Console.Out;
    //     }
    //
    //     public void Merge()
    //     {
    //         var timer = Stopwatch.StartNew();
    //
    //         var availableReaders = BuildReaders();
    //         var partitionsLength = availableReaders.Sum(reader => reader.Length);
    //
    //         var writer = new StreamWriter(_options.Output, false, Encoding.UTF8, Constants.StreamWriterBuffer);
    //         var writerStream = writer.BaseStream;
    //         var writtenBytesPrev = 0L;
    //
    //         var activeReaders = new List<FilePartitionReader>(availableReaders.Length);
    //         var equalsReaders = new List<FilePartitionReader>(availableReaders.Length);
    //         while (true)
    //         {
    //             var writtenBytes = writerStream.Length;
    //             if (writtenBytes - writtenBytesPrev > Constants.HundredMegabytes)
    //             {
    //                 ShowProgress(writtenBytes, partitionsLength, timer.Elapsed.TotalSeconds);
    //                 writtenBytesPrev = writtenBytes;
    //             }
    //
    //             // ReSharper disable once LoopCanBeConvertedToQuery
    //             foreach (var availableReader in availableReaders)
    //             {
    //                 if (availableReader.End) continue;
    //                 activeReaders.Add(availableReader);
    //             }
    //
    //             if (activeReaders.Count == 0)
    //             {
    //                 ShowProgress(writtenBytes, partitionsLength, timer.Elapsed.TotalSeconds);
    //                 break;
    //             }
    //
    //             activeReaders.Sort();
    //
    //             var currentValue = activeReaders[0].Value.Span;
    //             // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
    //             foreach (var activeReader in activeReaders)
    //             {
    //                 if (currentValue.Equals(activeReader.Value.Span, StringComparison.Ordinal))
    //                 {
    //                     equalsReaders.Add(activeReader);
    //                 }
    //             }
    //
    //             var sortedNumbers = equalsReaders.Count == 1
    //                 ? equalsReaders[0].ReadValueNumbers()
    //                 : equalsReaders
    //                     .SelectMany(r => r.ReadValueNumbers())
    //                     .OrderBy(n => n);
    //
    //             foreach (var sortedNumber in sortedNumbers)
    //             {
    //                 writer.WriteLong(sortedNumber);
    //                 writer.Write(Constants.NumberPostfix);
    //                 writer.WriteLine(currentValue);
    //             }
    //
    //             foreach (var equalsReader in equalsReaders)
    //             {
    //                 equalsReader.ReadNextValue();
    //             }
    //
    //             activeReaders.Clear();
    //             equalsReaders.Clear();
    //         }
    //
    //         writer.Flush();
    //         writer.Dispose();
    //
    //         DisposeReaders(availableReaders);
    //
    //         Console.WriteLine();
    //     }
    //
    //     private FilePartitionReader[] BuildReaders()
    //     {
    //         var files = Directory.GetFiles(_options.Folder, _options.FileMask);
    //
    //         var readers = new FilePartitionReader[files.Length];
    //         for (var i = files.Length - 1; i >= 0; i--)
    //         {
    //             readers[i] = new FilePartitionReader(files[i]);
    //         }
    //
    //         return readers.ToArray();
    //     }
    //
    //     private void DisposeReaders(FilePartitionReader[] readers)
    //     {
    //         var deletePartitionFile = _options.DeletePartitions;
    //         foreach (var reader in readers)
    //         {
    //             reader.Dispose();
    //
    //             if (deletePartitionFile)
    //             {
    //                 reader.DeleteSource();
    //             }
    //         }
    //
    //         Array.Clear(readers, 0, readers.Length);
    //     }
    //
    //     private void ShowProgress(long readerPosition, long partitionSize, double elapsedSeconds)
    //     {
    //         var readMegabytes = Math.Round(readerPosition / Constants.Megabyte);
    //
    //         Console.CursorLeft = 0;
    //
    //         var writer = _consoleWriter;
    //         writer.Write("Merging ");
    //         writer.Write(Math.Round((double) readerPosition / partitionSize * 100d));
    //         writer.Write("% (total ");
    //         writer.Write(readMegabytes);
    //         writer.Write(" MB, per second ");
    //         writer.Write(Math.Round(readMegabytes / elapsedSeconds));
    //         writer.Write(" MB)...");
    //     }
    // }
}