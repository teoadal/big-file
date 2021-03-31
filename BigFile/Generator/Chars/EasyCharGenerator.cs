using System;

namespace BigFile.Generator.Chars
{
    internal sealed class EasyCharGenerator : ICharGenerator
    {
        private int _alphabetPosition;
        private readonly Random _random;

        public EasyCharGenerator(int seed)
        {
            _alphabetPosition = 0;
            _random = new Random(seed);
        }

        public char GetNextChar()
        {
            var position = _alphabetPosition;
            if (position == Constants.AlphabetLength)
            {
                position = _random.Next(0, Constants.AlphabetLength - 1);
            }

            var result = Constants.Alphabet[position];

            position++;
            _alphabetPosition = position;

            return result;
        }
    }
}