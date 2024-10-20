using System.Numerics;
using System.Text;
using dbs_labs_back.Constants;
using dbs_labs_back.Extensions;
using dbs_labs_back.Models;
using dbs_labs_back.Models.abstractions;
using dbs_labs_back.Settings;
using dbs_labs_back.Utils.abstractions;

namespace dbs_labs_back.Utils ;

    public class RC5Util
    {
        private readonly LinearCongruentialGenerator  _numberGenerator;
        private readonly IWordFactory _wordsFactory;
        private readonly int _roundsCount;
        private readonly byte[] _password;
        
        public RC5Util(RC5Settings algorithmSettings, string password)
        {
            _numberGenerator = new LinearCongruentialGenerator(
                (int)Math.Pow(7, 5),
                64,
                17711,
                (BigInteger)Math.Pow(2, 31) - 1);

            _wordsFactory = algorithmSettings.WordLengthInBits.GetWordFactory();
            _roundsCount = (int)algorithmSettings.RoundCount;
            _password = GetMD5HashedKeyForRC5(Encoding.UTF8.GetBytes(password), algorithmSettings.KeyLengthInBytes);
        }
        
        public  byte[] GetMD5HashedKeyForRC5(
             byte[] key,
            KeyLengthInBytesEnum keyLengthInBytes)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var hasher = new Md5Generator();
            var bytesHash = hasher.ComputeHash(key).ToByteArray();

            if (keyLengthInBytes == KeyLengthInBytesEnum.Bytes_8)
            {
                bytesHash = bytesHash.Take(bytesHash.Length / 2).ToArray();
            }
            else if (keyLengthInBytes == KeyLengthInBytesEnum.Bytes_32)
            {
                bytesHash = bytesHash
                    .Concat(hasher.ComputeHash(bytesHash).ToByteArray())
                    .ToArray();
            }

            if (bytesHash.Length != (int)keyLengthInBytes)
            {
                throw new InvalidOperationException(
                    $"Internal error at {nameof(GetMD5HashedKeyForRC5)} method, " +
                    $"hash result is not equal to {(int)keyLengthInBytes}.");
            }

            return bytesHash;
        }
        public byte[] EncipherCBCPAD(byte[] input)
        {
            var paddedBytes = input.Concat( GetPadding(input)).ToArray();
            var bytesPerBlock = _wordsFactory.BytesPerBlock;
            var s = BuildExpandedKeyTable();
            var cnPrev = GetRandomBytesForInitVector();
            var encodedFileContent = new byte[cnPrev.Length + paddedBytes.Length];


            EncipherECB(cnPrev, encodedFileContent, inStart: 0, outStart: 0, s);

            for (int i = 0; i < paddedBytes.Length; i += bytesPerBlock)
            {
                var cn = new byte[bytesPerBlock];
                Array.Copy(paddedBytes, i, cn, 0, cn.Length);

                cn.XorWith(
                    xorArray: cnPrev,
                    inStartIndex: 0,
                    xorStartIndex: 0,
                    length: cn.Length);

                EncipherECB(
                    inBytes: cn,
                    outBytes: encodedFileContent,
                    inStart: 0,
                    outStart: i + bytesPerBlock,
                    s: s);

                Array.Copy(encodedFileContent, i + bytesPerBlock, cnPrev, 0, cn.Length);
            }

            return encodedFileContent;
        }
        
        public byte[] DecipherCBCPAD(byte[] input)
        {
            var bytesPerBlock = _wordsFactory.BytesPerBlock;
            var s = BuildExpandedKeyTable();
            var cnPrev = new byte[bytesPerBlock];
            Array.Copy(input, 0, cnPrev, 0, bytesPerBlock);

            var decodedFileContent = new byte[input.Length - bytesPerBlock];

            DecipherECB(
                inBuf: input,
                outBuf: cnPrev,
                inStart: 0,
                outStart: 0,
                s: s);

            for (int i = bytesPerBlock; i < input.Length; i += bytesPerBlock)
            {
                var cn = new byte[bytesPerBlock];
                Array.Copy(input, i, cn, 0, cn.Length);

                DecipherECB(
                    inBuf: cn,
                    outBuf: decodedFileContent,
                    inStart: 0,
                    outStart: i - bytesPerBlock,
                    s: s);

                decodedFileContent.XorWith(
                    xorArray: cnPrev,
                    inStartIndex: i - bytesPerBlock,
                    xorStartIndex: 0,
                    length: cn.Length);

                Array.Copy(input, i, cnPrev, 0, cnPrev.Length);
            }

            var decodedWithoutPadding = new byte[decodedFileContent.Length - decodedFileContent.Last()];
            Array.Copy(decodedFileContent, decodedWithoutPadding, decodedWithoutPadding.Length);

            return decodedWithoutPadding;
        }
        
        private void EncipherECB(byte[] inBytes, byte[] outBytes, int inStart, int outStart, IWord[] s)
        {
            var a = _wordsFactory.CreateFromBytes(inBytes, inStart);
            var b = _wordsFactory.CreateFromBytes(inBytes, inStart + _wordsFactory.BytesPerWord);

            a.Add(s[0]);
            b.Add(s[1]);

            for (var i = 1; i < _roundsCount + 1; ++i)
            {
                a.XorWith(b).ROL(b.ToInt32()).Add(s[2 * i]);
                b.XorWith(a).ROL(a.ToInt32()).Add(s[2 * i + 1]);
            }

            a.FillBytesArray(outBytes, outStart);
            b.FillBytesArray(outBytes, outStart + _wordsFactory.BytesPerWord);
        }

        private void DecipherECB(byte[] inBuf, byte[] outBuf, int inStart, int outStart, IWord[] s)
        {
            var a = _wordsFactory.CreateFromBytes(inBuf, inStart);
            var b = _wordsFactory.CreateFromBytes(inBuf, inStart + _wordsFactory.BytesPerWord);

            for (var i = _roundsCount; i > 0; --i)
            {
                b = b.Sub(s[2 * i + 1]).ROR(a.ToInt32()).XorWith(a);
                a = a.Sub(s[2 * i]).ROR(b.ToInt32()).XorWith(b);
            }

            a.Sub(s[0]);
            b.Sub(s[1]);

            a.FillBytesArray(outBuf, outStart);
            b.FillBytesArray(outBuf, outStart + _wordsFactory.BytesPerWord);
        }
        
        private byte[] GetPadding(byte[] inBytes)
        {
            var paddingLength = _wordsFactory.BytesPerBlock - inBytes.Length % (_wordsFactory.BytesPerBlock);

            var padding = new byte[paddingLength];

            for (int i = 0; i < padding.Length; ++i)
            {
                padding[i] = (byte)paddingLength;
            }

            return padding;
        }

        private byte[] GetRandomBytesForInitVector()
        {
            int ivLength = _wordsFactory.BytesPerBlock;
            var ivBytes = new byte[ivLength];
            int bytesFilled = 0;

            while (bytesFilled < ivLength)
            {
                ulong nextNumber = (ulong)_numberGenerator.NextNumber();
                byte[] numberBytes = BitConverter.GetBytes(nextNumber);

                int bytesToCopy = Math.Min(numberBytes.Length, ivLength - bytesFilled);
                Array.Copy(numberBytes, 0, ivBytes, bytesFilled, bytesToCopy);
                bytesFilled += bytesToCopy;
            }

            return ivBytes;
        }



        private IWord[] BuildExpandedKeyTable()
        {
            var keysWordArrLength = _password.Length % _wordsFactory.BytesPerWord > 0
                ? _password.Length / _wordsFactory.BytesPerWord + 1
                : _password.Length / _wordsFactory.BytesPerWord;

            var lArr = new IWord[keysWordArrLength];

            for (int i = 0; i < lArr.Length; i++)
            {
                lArr[i] = _wordsFactory.Create();
            }

            for (var i = _password.Length - 1; i >= 0; i--)
            {
                lArr[i / _wordsFactory.BytesPerWord].ROL(RC5Constants.BitsPerByte).Add(_password[i]);
            }

            var sArray = new IWord[2 * (_roundsCount + 1)];
            sArray[0] = _wordsFactory.CreateP();
            var q = _wordsFactory.CreateQ();

            for (var i = 1; i < sArray.Length; i++)
            {
                sArray[i] = sArray[i - 1].Clone();
                sArray[i].Add(q);
            }

            var x = _wordsFactory.Create();
            var y = _wordsFactory.Create();

            var n = 3 * Math.Max(sArray.Length, lArr.Length);

            for (int k = 0, i = 0, j = 0; k < n; ++k)
            {
                sArray[i].Add(x).Add(y).ROL(3);
                x = sArray[i].Clone();

                lArr[j].Add(x).Add(y).ROL(x.ToInt32() + y.ToInt32());
                y = lArr[j].Clone();

                i = (i + 1) % sArray.Length;
                j = (j + 1) % lArr.Length;
            }

            return sArray;
        }
        
    }