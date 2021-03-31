using System;
using System.IO;
using CommandLine;

namespace BigFile.Tester
{
    [Verb("test", HelpText = "Test exists file")]
    public class TestOptions
    {
        [Option("compare", Required = false, HelpText = "Compare withs source file path", Default = false)]
        public bool CompareWithSource { get; set; }

        [Option("source", Required = false, HelpText = "Source file path", Default = Constants.RandomFileName)]
        public string Source { get; set; } = null!;

        [Option('s', "sorted", Required = false, HelpText = "Sorted file path", Default = Constants.SortedFileName)]
        public string Sorted { get; set; } = null!;

        public bool Validate()
        {
            if (CompareWithSource)
            {
                if (string.IsNullOrWhiteSpace(Source))
                {
                    Console.WriteLine("Set source file path");
                    return false;
                }

                if (!File.Exists(Source))
                {
                    Console.WriteLine($"File by path '{Source}' isn't found ");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(Sorted))
            {
                Console.WriteLine("Set sorted file path");
                return false;
            }

            if (!File.Exists(Sorted))
            {
                Console.WriteLine($"File by path '{Sorted}' isn't found ");
                return false;
            }

            return true;
        }
    }
}