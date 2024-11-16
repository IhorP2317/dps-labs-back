namespace dbs_labs_back.Utils ;
using System;
using System.Security.Cryptography;

    public class DSAUtil
    {
        private readonly DSACryptoServiceProvider _dsa = new();
        private readonly SHA1 _sha1 = SHA1.Create();
        
        public string ProcessSignature(byte[] message) => Convert.ToHexString(_dsa.CreateSignature(_sha1.ComputeHash(message)));
        public bool VerifySignature(byte[] message, string hexSignature)
        {
            try
            {
                var verified = _dsa.VerifySignature(_sha1.ComputeHash(message), Convert.FromHexString(hexSignature));
                return verified;
            }
            catch
            {
                return false;
            }
        }
    }