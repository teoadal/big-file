using BigFile.Models;

namespace BigFile.Sorter.Strategies
{
    internal interface ISortStrategy
    {
        void Aggregate(DataRecord dataRecord);

        void WriteResult(string outputFile);
    }
}