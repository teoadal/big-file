using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BigFile.Extensions;
using BigFile.Merger;
using BigFile.Models;
using BigFile.Sorter.Buffer;

namespace BigFile.Sorter.Strategies
{
    internal sealed class DefaultStrategy : ISortStrategy
    {
        private DataBuffer _buffer;
        private int _nextPartitionNumber;

        public DefaultStrategy()
        {
            _buffer = new DataBuffer();
        }

        public void Aggregate(DataRecord dataRecord)
        {
            if (_buffer.TryAdd(dataRecord)) return;

            WritePartition();

            _buffer.TryAdd(dataRecord);
        }

        public void WriteResult(string outputFile)
        {
            WritePartition(false); // write last partition

            _buffer.Dispose();
            _buffer = null!;

            GC.Collect();

            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
            if (string.IsNullOrWhiteSpace(outputDirectory)) outputDirectory = ".";
            var merger = new FileMerger(new MergeOptions
            {
                DeletePartitions = true,
                FileMask = Constants.PartitionFileName,
                Folder = outputDirectory,
                Output = outputFile
            });

            merger.Merge();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CreatePartitionName(int partitionNumber)
        {
            return Constants.PartitionFileName.Replace("*", partitionNumber.ToString("000"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WritePartition(bool collectGarbage = true)
        {
            var partitionName = CreatePartitionName(_nextPartitionNumber++);
            using (var writer = new StreamWriter(partitionName, false, Encoding.UTF8, Constants.StreamWriterBuffer))
            {
                var orderedEntries = _buffer
                    .AsParallel()
                    .OrderBy(entry => entry);

                var firstEntry = true;
                foreach (var entry in orderedEntries)
                {
                    if (firstEntry) firstEntry = false;
                    else writer.WriteLine();

                    writer.WriteLine(entry.Key.Span);

                    entry.Sort();
                    var firstNumber = true;
                    foreach (var number in entry)
                    {
                        if (firstNumber) firstNumber = false;
                        else writer.Write('|');

                        writer.WriteLong(number);
                    }
                }
            }

            _buffer.Clear();

            if (collectGarbage)
            {
                GC.Collect();
            }
        }
    }
}