using System;
using CommandLine;

namespace BigFile.Generator
{
    [Verb("create", HelpText = "Create file with random data")]
    public sealed class GenerateOptions
    {
        [Option('f', "from-number", Required = false, HelpText = "Start number of data", Default = -100_000L)]
        public long FromNumber { get; set; }

        [Option("max-string-length", Required = false, HelpText = "Output file path", Default = 1024)]
        public int MaxStringLength { get; set; }

        [Option("min-string-length", Required = false, HelpText = "Output file path", Default = 0)]
        public int MinStringLength { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file path", Default = Constants.RandomFileName)]
        public string Output { get; set; } = null!;

        [Option('r', "repeat-percent", Required = false, HelpText = "Number repetitions of string", Default = 1)]
        public int RepeatPercent { get; set; }

        [Option("seed", Required = false, HelpText = "Randomizer seed")]
        public int? Seed { get; set; }

        [Option('s', "size", Required = false, HelpText = "Output file size in bytes", Default = 10_737_418_240)]
        public long Size { get; set; }

        [Option("well", Required = false, HelpText = "Use well but not fast string generator", Default = false)]
        public bool WellGenerator { get; set; }

        public bool Validate()
        {
            if (MinStringLength < 0 || MinStringLength > MaxStringLength)
            {
                Console.WriteLine("Min string length should be greater 0 and less that max string length");
                return false;
            }

            if (MaxStringLength < MinStringLength || MaxStringLength > Constants.ValueMaxLength)
            {
                Console.WriteLine(
                    $"Max string length must be between 0 and {Constants.ValueMaxLength} and greater that min string length");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Output))
            {
                Console.WriteLine("Set output file path");
                return false;
            }

            if (RepeatPercent < 0 || RepeatPercent > 90)
            {
                Console.WriteLine("Repeat percent must be between 0 and 90");
                return false;
            }

            if (Size < 0)
            {
                Console.WriteLine("Size should be greater that 0");
                return false;
            }

            return true;
        }
    }
}