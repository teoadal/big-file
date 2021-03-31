using System;
using System.IO;
using CommandLine;

namespace BigFile.Sorter
{
    [Verb("sort", HelpText = "Sort exists file")]
    public sealed class SortOptions
    {
        [Option('i', "input", Required = false, HelpText = "Input not sorted file path", Default = Constants.RandomFileName)]
        public string Input { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output sorted file path", Default = Constants.SortedFileName)]
        public string Output { get; set; } = null!;

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Input))
            {
                Console.WriteLine("Set input file path");
                return false;
            }

            if (!File.Exists(Input))
            {
                Console.WriteLine($"Input file by path '{Input}' isn't found");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Output))
            {
                Console.WriteLine("Set output file path");
                return false;
            }

            return true;
        }
    }
}