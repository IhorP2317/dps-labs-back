using dbs_labs_back.Models.abstractions;
using RC5C = dbs_labs_back.Constants.RC5Constants;

namespace dbs_labs_back.Models ;

    public class Word16Bit : IWord
    {
        public const int WordSizeInBits = BytesPerWord * RC5C.BitsPerByte;
        public const int BytesPerWord = sizeof(ushort);

        public ushort WordValue { get; set; }

        public void CreateFromBytes(byte[] bytes, int startFrom)
        {
            WordValue = 0;

            for (var i = startFrom + BytesPerWord - 1; i > startFrom; --i)
            {
                WordValue = (ushort)(WordValue | bytes[i]);
                WordValue = (ushort)(WordValue << RC5C.BitsPerByte);
            }

            WordValue = (ushort)(WordValue | bytes[startFrom]);
        }

        public byte[] FillBytesArray(byte[] bytesToFill, int startFrom)
        {
            var i = 0;
            for (; i < BytesPerWord - 1; ++i)
            {
                bytesToFill[startFrom + i] = (byte)(WordValue & RC5C.ByteMask);
                WordValue = (ushort)(WordValue >>  RC5C.BitsPerByte);
            }

            bytesToFill[startFrom + i] = (byte)(WordValue & RC5C.ByteMask);

            return bytesToFill;
        }

        public IWord ROL(int offset)
        {
            offset %= BytesPerWord;
            WordValue = (ushort)((WordValue << offset) | (WordValue >>  (WordSizeInBits - offset)));

            return this;
        }

        public IWord ROR(int offset)
        {
            offset %= BytesPerWord;
            WordValue = (ushort)((WordValue >>  offset) | (WordValue << (WordSizeInBits - offset)));

            return this;
        }

        public IWord Add(IWord word)
        {
            WordValue = (ushort)(WordValue + (word as Word16Bit).WordValue);

            return this;
        }

        public IWord Add(byte value)
        {
            WordValue = (ushort)(WordValue + value);

            return this;
        }

        public IWord Sub(IWord word)
        {
            WordValue = (ushort)(WordValue - (word as Word16Bit).WordValue);

            return this;
        }

        public IWord XorWith(IWord word)
        {
            WordValue = (ushort)(WordValue ^ (word as Word16Bit).WordValue);

            return this;
        }

        public IWord Clone()
        {
            return (IWord)MemberwiseClone();
        }

        public int ToInt32()
        {
            return WordValue;
        }
    }