namespace dbs_labs_back.Extensions ;

    public static class ByteArrayExtensions
    {
        public static void XorWith(
            this byte[] array,
            byte[] xorArray,
            int inStartIndex,
            int xorStartIndex,
            int length)
        {
            for (int i = 0; i < length; ++i)
            {
                array[i + inStartIndex] ^= xorArray[i + xorStartIndex];
            }
        }
    }