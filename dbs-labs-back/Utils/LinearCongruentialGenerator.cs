using dbs_labs_back.Models;

namespace dbs_labs_back.Utils ;

using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

    public class LinearCongruentialGenerator
    {
        private readonly int a;
        private readonly int c;
        private readonly BigInteger m;
        private readonly int sequenceLength;
        private readonly BigInteger X0;
        private BigInteger _Xn;

        public LinearCongruentialGenerator(int a, int X0, int c, BigInteger m, int sequenceLength = 0)
        {
            this.a = a;
            this.X0 = X0;
            this.c = c;
            this.m = m;
            this.sequenceLength = sequenceLength;
           _Xn = X0;
        }


        public async Task<LinearCongruentialGeneratorResult> GenerateRandomNumbersAndWriteToFile()
        {
           BigInteger currentNumber = X0;
            BigInteger? X1 = null;
            var randomNumbersSequence = new List<string> { X0.ToString() };
            int period = 0;
            bool periodFound = false;

            await using var writer = new StreamWriter("result.txt", false);
            var randomNumbersSequenceToBeWritten = new StringBuilder();
            const int batchSize = 1_000_000;
            randomNumbersSequenceToBeWritten.Append(X0.ToString());

            while (true)
            {
                BigInteger prev = currentNumber;
                currentNumber = SequenceFormula(currentNumber);


                if (!periodFound)
                {
                    period++;
                }


                if (period == 1)
                {
                    X1 = currentNumber;
                }


                if (currentNumber == prev || currentNumber == X0 || (X1.HasValue && period > 1 && currentNumber == X1.Value))
                {
                    periodFound = true;
                }


                if (randomNumbersSequence.Count < sequenceLength)
                {
                    randomNumbersSequence.Add(currentNumber.ToString());
                }


                if (!periodFound)
                {
                    randomNumbersSequenceToBeWritten.Append(", " + currentNumber);
                    if (period % batchSize == 0)
                    {
                        await writer.WriteAsync(randomNumbersSequenceToBeWritten.ToString());
                        randomNumbersSequenceToBeWritten.Clear();
                        await writer.FlushAsync();
                    }
                }


                if (periodFound && randomNumbersSequence.Count >= sequenceLength)
                {
                    break;
                }
            }

            if (randomNumbersSequenceToBeWritten.Length > 0)
            {
                await writer.WriteAsync(randomNumbersSequenceToBeWritten.ToString());
                await writer.FlushAsync();
            }

            return new LinearCongruentialGeneratorResult(randomNumbersSequence, period);
        }

        public BigInteger NextNumber()
        {
            BigInteger currentNumber = this._Xn;
            _Xn = SequenceFormula(currentNumber);
            return currentNumber;
        }
        


        public void Reset() => _Xn = X0;

        private BigInteger SequenceFormula(BigInteger Xn) => (a * Xn + c) % m;
    }