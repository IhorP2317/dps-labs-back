using System.Text;
using dbs_labs_back.Constants;
using dbs_labs_back.Models;

namespace dbs_labs_back.Utils;

public class Md5Generator
{
    private const int OptimalChunkSizeMultiplier = 100_000; 
    private const uint OptimalChunkSize = MD5C.BytesCountPerBits512Block * OptimalChunkSizeMultiplier;
    public MessageDigest Hash { get; private set; }
    
    public string HashAsString => Hash.ToString();
    
    /// <summary>
    /// Elementary function F(B, C, D)
    /// </summary>
    public  uint FuncF(uint B, uint C, uint D) =>  (B & C) | (~B & D);

    /// <summary>
    /// Elementary function G(B, C, D)
    /// </summary>
    public  uint FuncG(uint B, uint C, uint D) => (D & B) | (C & ~D);

    /// <summary>
    /// Elementary function H(B, C, D)
    /// </summary>
    public  uint FuncH(uint B, uint C, uint D) => B ^ C ^ D;

    /// <summary>
    /// Elementary function I(B, C, D)
    /// </summary>
    public  uint FuncI(uint B, uint C, uint D) => C ^ (B | ~D);
    /// <summary>
    /// Computes MD5 hash of input message
    /// </summary>
    /// <param name="message">Input message</param>
    /// <returns>Returns hash as <see cref="MessageDigest"/> model</returns>
    public void ComputeHash(string message)
    {
        ComputeHash(Encoding.ASCII.GetBytes(message));
    }
    /// <summary>
    /// Computes MD5 hash of input message
    /// </summary>
    /// <param name="message">Input message, byte array</param>
    /// <returns>Returns hash as <see cref="MessageDigest"/> model</returns>
    public MessageDigest ComputeHash(byte[] message)
    {
        Hash = MessageDigest.InitialValue;

        var paddedMessage = message.Concat(GetMessagePadding((uint)message.Length)).ToArray();

        for (uint bNo = 0; bNo < paddedMessage.Length / MD5C.BytesCountPerBits512Block; ++bNo)
        {
            uint[] X = Extract32BitWords(
                paddedMessage,
                bNo,
                MD5C.Words32BitArraySize * MD5C.BytesPer32BitWord);

            FeedMessageBlockToBeHashed(X);
        }

        return Hash;
    }
    /// <summary>
        /// Computes MD5 hash of input file
        /// </summary>
        /// <param name="filePath">Path to file to be hashed</param>
        /// <param name="chunkSizeMultiplier"></param>
        /// <returns></returns>
    public async Task<MessageDigest> ComputeFileHashAsync(string filePath)
    {
        Hash = MessageDigest.InitialValue;

           using (var fs = File.OpenRead(filePath))
           {
               ulong totalBytesRead = 0; 
               int currentBytesRead = 0;
               bool isFileEnd = false;
               do
               {
                   var chunk = new byte[OptimalChunkSize];
                   
                   currentBytesRead = await fs.ReadAsync(chunk, 0, chunk.Length);
                   totalBytesRead += (ulong)currentBytesRead;


                    if (currentBytesRead < chunk.Length)
                    {
                        byte[] lastChunk;

                        if (currentBytesRead == 0)
                        {
                            lastChunk = GetMessagePadding(totalBytesRead);
                        }
                        else
                        {
                            lastChunk = new byte[currentBytesRead];
                            Array.Copy(chunk, lastChunk, currentBytesRead);

                            lastChunk = lastChunk.Concat(GetMessagePadding(totalBytesRead)).ToArray();
                        }

                        chunk = lastChunk;
                        isFileEnd = true;
                    }

                    for (uint bNo = 0; bNo < chunk.Length / MD5C.BytesCountPerBits512Block; ++bNo)
                    {
                        uint[] X = Extract32BitWords(
                            chunk,
                            bNo,
                            MD5C.Words32BitArraySize * MD5C.BytesPer32BitWord);

                        FeedMessageBlockToBeHashed(X);
                    }
               } 
               while (isFileEnd == false);
           }

        return Hash;
        }
    private void FeedMessageBlockToBeHashed(uint[] X)
    {
        uint F, i, k;
        var blockSize = MD5C.BytesCountPerBits512Block;
        var MDq = Hash.Clone();

        // first round
        for (i = 0; i < blockSize / 4; ++i)
        {
            k = i;
            F = FuncF(MDq.B, MDq.C, MDq.D);

            MDq.Md5IterationSwap(F, X, i, k);
        }
        // second round
        for (; i < blockSize / 2; ++i)
        {
            k = (1 + (5 * i)) % (blockSize / 4);
            F = FuncG(MDq.B, MDq.C, MDq.D);

            MDq.Md5IterationSwap(F, X, i, k);
        }
        // third round
        for (; i < blockSize / 4 * 3; ++i)
        {
            k = (5 + (3 * i)) % (blockSize / 4);
            F = FuncH(MDq.B, MDq.C, MDq.D);

            MDq.Md5IterationSwap(F, X, i, k);
        }
        // fourth round
        for (; i < blockSize; ++i)
        {
            k = 7 * i % (blockSize / 4);
            F = FuncI(MDq.B, MDq.C, MDq.D);

            MDq.Md5IterationSwap(F, X, i, k);
        }

        Hash += MDq;
    }
    private  byte[] GetMessagePadding(ulong messageLength)
    {
        uint paddingLengthInBytes = default;
        var mod = (uint)(messageLength * MD5C.BitsPerByte % MD5C.Bits512BlockSize);

        // Append Padding Bits
        if (mod == MD5C.BITS_448)
        {
            paddingLengthInBytes = MD5C.Bits512BlockSize / MD5C.BitsPerByte;
        }
        else if (mod > MD5C.BITS_448)
        {
            paddingLengthInBytes = (MD5C.Bits512BlockSize - mod + MD5C.BITS_448) / MD5C.BitsPerByte;
        }
        else if (mod < MD5C.BITS_448)
        {
            paddingLengthInBytes = (MD5C.BITS_448 - mod) / MD5C.BitsPerByte;
        }

        var padding = new byte[paddingLengthInBytes + MD5C.BitsPerByte];
        padding[0] = MD5C.BITS_128;

        // Append Length
        var messageLength64bit = messageLength * MD5C.BitsPerByte;

        for (var i = 0; i < MD5C.BitsPerByte; ++i)
        {
            padding[paddingLengthInBytes + i] = (byte)(messageLength64bit
                                                       >> (int)(i * MD5C.BitsPerByte)
                                                       & MD5C.BITS_255);
        }

        return padding;
    }
    public  uint[] Extract32BitWords(byte[] message, uint blockNo, uint blockSizeInBytes)
    {
        var messageStartIndex = blockNo * blockSizeInBytes;
        var extractedArray = new uint[blockSizeInBytes / MD5C.BytesPer32BitWord];

        for (uint i = 0; i < blockSizeInBytes; i += MD5C.BytesPer32BitWord)
        {
            var j = messageStartIndex + i;

            extractedArray[i / MD5C.BytesPer32BitWord] = // form 32-bit word from four bytes
                message[j]                                                   // first byte
                | (((uint)message[j + 1]) << ((int)MD5C.BitsPerByte * 1))  // second byte
                | (((uint)message[j + 2]) << ((int)MD5C.BitsPerByte * 2))  // third byte
                | (((uint)message[j + 3]) << ((int)MD5C.BitsPerByte * 3)); // fourth byte
        }

        return extractedArray;
    }
}