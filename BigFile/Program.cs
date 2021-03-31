using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BigFile.Generator;
using BigFile.Merger;
using BigFile.Sorter;
using BigFile.Tester;
using CommandLine;

namespace BigFile
{
    public static class Program
    {
        public static int Main(string[]? args)
        {
            var verbTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !type.IsAbstract && Attribute.IsDefined(type, typeof(VerbAttribute)))
                .ToArray();

            if (args?.Length > 0)
            {
                return Parser.Default.ParseArguments(args, verbTypes)
                    .MapResult(
                        (GenerateOptions generator) => ExecuteGenerator(generator),
                        (MergeOptions merge) => ExecuteMerge(merge),
                        (SortOptions sort) => ExecuteSort(sort),
                        (TestOptions test) => ExecuteTest(test),
                        _ => 1);
            }

            Console.WriteLine("Use --help for show commands");
            return 0;
        }

        private static int ExecuteGenerator(GenerateOptions options)
        {
            if (!options.Validate()) return 1;

            var generator = new FileGenerator(options);
            var timer = Stopwatch.StartNew();

            Console.WriteLine($"File '{options.Output}' creation started...");

            generator.Generate();

            Console.WriteLine($"File creation executed at {timer.Elapsed}");
            return 0;
        }

        private static int ExecuteMerge(MergeOptions options)
        {
            if (!options.Validate()) return 1;

            var merger = new FileMerger(options);
            var timer = Stopwatch.StartNew();

            var partitionCount = Directory.GetFiles(options.Folder, options.FileMask).Length;
            Console.WriteLine($"Merge {partitionCount} partition started...");

            merger.Merge();

            Console.WriteLine($"Partitions merge executed at {timer.Elapsed}");
            return 0;
        }

        private static int ExecuteSort(SortOptions options)
        {
            if (!options.Validate()) return 1;

            var sorter = new FileSorter(options);
            var timer = Stopwatch.StartNew();

            Console.WriteLine($"File '{options.Input}' sort started...");

            sorter.Sort();

            Console.WriteLine($"File sort executed at {timer.Elapsed}");
            return 0;
        }

        private static int ExecuteTest(TestOptions options)
        {
            if (!options.Validate()) return 1;

            var tester = new FileTester(options);
            var timer = Stopwatch.StartNew();

            Console.WriteLine($"File '{options.Sorted}' test started...");

            tester.Test();

            Console.WriteLine($"File test executed at {timer.Elapsed}");
            return 0;
        }
    }
}