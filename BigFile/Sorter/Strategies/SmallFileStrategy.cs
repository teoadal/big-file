using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BigFile.Extensions;
using BigFile.Models;

namespace BigFile.Sorter.Strategies
{
    internal sealed class SmallFileStrategy : ISortStrategy
    {
        private readonly Dictionary<string, List<long>> _data;

        public SmallFileStrategy()
        {
            _data = new Dictionary<string, List<long>>(2048);
        }

        public void Aggregate(DataRecord dataRecord)
        {
            var key = new string(dataRecord.Value);
            var number = dataRecord.Number;

            if (!_data.TryGetValue(key, out var numbers))
            {
                numbers = new List<long>(16);
                _data.Add(key, numbers);
            }

            numbers.Add(number);
        }

        public void Checkout()
        {
        }

        public void WriteResult(string outputFile)
        {
            GC.Collect();

            using var writer = new StreamWriter(outputFile, false, Encoding.UTF8, ushort.MaxValue);

            var sortedRecords = _data.AsParallel().OrderBy(pair => pair.Key);
            foreach (var (key, numbers) in sortedRecords)
            {
                numbers.Sort();
                foreach (var number in numbers)
                {
                    writer.WriteLong(number);
                    writer.Write(Constants.NumberPostfix);
                    writer.WriteLine(key);
                }

                numbers.Clear();
            }
        }
    }
}