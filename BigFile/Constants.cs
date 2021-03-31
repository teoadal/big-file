using System;

namespace BigFile
{
    internal static class Constants
    {
        public const string Alphabet = "QWERTY UIOPASD FGHJKLZXCVBNM qwert yuiopasdfg hjklzx cvbnm йцукенгшщзхъфывапролджэячсмитьбю";

        public static readonly int AlphabetLength = Alphabet.Length;

        public const long HundredMegabytes = 104_857_600;

        public const double Megabyte = 1_048_576d;

        public static readonly int NumberLength = long.MinValue.ToString().Length;

        public const string NumberPostfix = ". ";

        public const string PartitionFileName = "partition-*.tmp";

        public const string RandomFileName = "randomData.txt";

        public const string SortedFileName = "sortedData.txt";

        /// <summary>
        /// Реомендуемая величина для компенсирования стоимости смещения читающей головки HDD.
        /// </summary>
        public const int StreamReaderBuffer = (int) Megabyte * 8;

        public const int StreamWriterBuffer = ushort.MaxValue;

        public const StringComparison ValueComparison = StringComparison.Ordinal;

        public const int ValueMaxLength = 1024;
    }
}