using System;
using System.IO;
using CommandLine;

namespace BigFile.Merger
{
    [Verb("merge", HelpText = "Merge file partitions")]
    public sealed class MergeOptions
    {
        [Option("delete-partitions", Required = false, HelpText = "Delete partitions after merge", Default = false)]
        public bool DeletePartitions { get; set; }

        [Option('m', "mask", Required = false, HelpText = "Mask of partitions", Default = Constants.PartitionFileName)]
        public string FileMask { get; set; } = null!;

        [Option('d', "dir", Required = false, HelpText = "Files directory with partitions", Default = ".")]
        public string Folder { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output file path", Default = Constants.SortedFileName)]
        public string Output { get; set; } = null!;

        public bool Validate()
        {
            if (Folder == null!)
            {
                Console.WriteLine("Set directory of partitions");
                return false;
            }

            if (!Directory.Exists(Folder))
            {
                Console.WriteLine($"Directory '{Folder}' isn't found");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FileMask))
            {
                Console.WriteLine("Set mask of partitions");
                return false;
            }

            if (Directory.GetFiles(Folder, FileMask).Length == 0)
            {
                Console.WriteLine($"Files by mask '{FileMask}' isn't found in directory '{Folder}'");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Output))
            {
                Console.WriteLine($"Set output file path");
            }

            return true;
        }
    }
}