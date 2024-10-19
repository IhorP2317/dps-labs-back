using dbs_labs_back.Constants;
using dbs_labs_back.Models;
using dbs_labs_back.Models.abstractions;
using dbs_labs_back.Utils.abstractions;

namespace dbs_labs_back.Utils ;

    public class Word64BitFactory : IWordFactory
    {
        public int BytesPerWord => Word64Bit.BytesPerWord;

        public int BytesPerBlock => BytesPerWord * 2;

        public IWord Create()
        {
            return CreateConcrete();
        }

        public IWord CreateP()
        {
            return CreateConcrete(RC5Constants.P64);
        }

        public IWord CreateQ()
        {
            return CreateConcrete(RC5Constants.Q64);
        }

        public IWord CreateFromBytes(byte[] bytes, int startFromIndex)
        {
            var word = Create();
            word.CreateFromBytes(bytes, startFromIndex);

            return word;
        }

        private static Word64Bit CreateConcrete(ulong value = 0)
        {
            return new Word64Bit
            {
                WordValue = value
            };
        }
    }