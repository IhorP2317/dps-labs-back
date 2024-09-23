
using dbs_labs_back.Models;

namespace dbs_labs_back.Utils;

using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class LinearCongruentialGenerator
{
    public async Task<LinearCongruentialGeneratorResult> GenerateRandomNumbersAndWriteToFile(int a,
        int X0,
        int c, 
        BigInteger m, 
        int sequenceLength)
    {
        BigInteger Xn = X0;
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
            BigInteger prev = Xn;
            Xn = (a * Xn + c) % m;

         
            if (!periodFound)
            {
                period++;
            }

           
            if (period == 1)
            {
                X1 = Xn;
            }

          
            if (Xn == prev || Xn == X0 || (X1.HasValue && period > 1 && Xn == X1.Value))
            {
                periodFound = true;
            }

          
            if (randomNumbersSequence.Count < sequenceLength)
            {
                randomNumbersSequence.Add(Xn.ToString());
            }

          
            if (!periodFound)
            {
                randomNumbersSequenceToBeWritten.Append(", " + Xn);
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
}

