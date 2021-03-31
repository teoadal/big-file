using System;

namespace BigFile.Generator.Chars
{
    internal sealed class DefaultCharGenerator : ICharGenerator
    {
        private readonly Random _random;

        public DefaultCharGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public char GetNextChar()
        {
            return Constants.Alphabet[_random.Next(0, Constants.AlphabetLength)];
        }
    }
}