using dbs_labs_back.Constants;
using dbs_labs_back.Utils;

namespace dbs_labs_back.Models;

    public class MessageDigest
    {
        internal static MessageDigest InitialValue { get; }

        static MessageDigest()
        {
            InitialValue = new MessageDigest
            {
                A = MD5C.A_MD_BUFFER_INITIAL,
                B = MD5C.B_MD_BUFFER_INITIAL,
                C = MD5C.C_MD_BUFFER_INITIAL,
                D = MD5C.D_MD_BUFFER_INITIAL
            };
        }

        public uint A { get; set; }

        public uint B { get; set; }

        public uint C { get; set; }

        public uint D { get; set; }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(A)
                .Concat(BitConverter.GetBytes(B))
                .Concat(BitConverter.GetBytes(C))
                .Concat(BitConverter.GetBytes(D))
                .ToArray();
                
        }

        public MessageDigest Clone()
        {
            return MemberwiseClone() as MessageDigest;
        }

        internal void Md5IterationSwap(uint F, uint[] X, uint i, uint k)
        {
            var tempD = D;
            D = C;
            C = B;
            B += LeftRotate(A + F + X[k] + MD5C.T[i], MD5C.S[i]);
            A = tempD;
        }

        public override string ToString()
        {
            return $"{ToByteString(A)}{ToByteString(B)}{ToByteString(C)}{ToByteString(D)}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object value)
        {
            return value is MessageDigest md
                && (GetHashCode() == md.GetHashCode() || ToString() == md.ToString());
        }

        public static MessageDigest operator+(MessageDigest left, MessageDigest right)
        {
            return new()
            {
                A = left.A + right.A,
                B = left.B + right.B,
                C = left.C + right.C,
                D = left.D + right.D
            };
        } 
        private  uint LeftRotate(uint value, int shiftValue)
        {
            return (value << shiftValue)
                   | (value >> (int)(MD5C.BitsPerByte * MD5C.BytesPer32BitWord - shiftValue));
        }

        private  string ToByteString(uint x)
        {
            return string.Join(string.Empty, BitConverter.GetBytes(x).Select(y => y.ToString("x2")));
        }
    }