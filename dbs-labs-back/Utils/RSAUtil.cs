using System.Security.Cryptography;
using dbs_labs_back.Constants;
using dbs_labs_back.Models;

namespace dbs_labs_back.Utils ;

    public class RSAUtil
    {
        private  RSACryptoServiceProvider _rsa;
        private  int _encipherBlockSizeRsa;
        private  int _decipherBlockSizeRsa;

        public RSAUtil()
        {
            _rsa = new RSACryptoServiceProvider();
            InitializeBlockSizes();
        }
        
        public void GenerateNewKeyPair(KeyLengthInBytesRSAEnum keySizeInBytes)
        {
           
            var keySizeInBits = (int)keySizeInBytes * 8;
            _rsa = new RSACryptoServiceProvider(keySizeInBits);
            InitializeBlockSizes();
        }

        public KeyLengthInBytesRSAEnum KeySizeInBytes => (KeyLengthInBytesRSAEnum)(_rsa.KeySize / 8);


        public void ImportKey(string keyXml)
        {
            _rsa.FromXmlString(keyXml);
            InitializeBlockSizes();
        }
        

        public string ExportPublicKey()
        {
            return _rsa.ToXmlString(false);
        }
        public string ExportFullKey()
        {
            return _rsa.ToXmlString(true);
        }
        public bool HasPrivateKey
        {
            get
            {
                try
                {
                    var rsaParameters = _rsa.ExportParameters(true);
                    return rsaParameters.D is {Length: > 0 };
                }
                catch (CryptographicException)
                {
                    return false;
                }
            }
        }

        public bool HasPublicKey
        {
            get
            {
                try
                {

                    var parameters = _rsa.ExportParameters(false);


                    var modulusPresent = parameters.Modulus is {Length: > 0 };
                    var exponentPresent = parameters.Exponent is {Length: > 0 };

                    return modulusPresent && exponentPresent;
                }
                catch (CryptographicException)
                {

                    return false;
                }
            }
        }

        private byte[] EncryptData(byte[] dataToEncrypt)
        {
            return _rsa.Encrypt(dataToEncrypt, false);
        }
        private byte[] DecryptData(byte[] dataToDecrypt)
        {
            return _rsa.Decrypt(dataToDecrypt, false);
        }

        public byte[] EncryptDataPerBlock(byte[] dataToEncrypt)
        {
            var encipheredBytes = new List<byte>
            {
                Capacity = dataToEncrypt.Length * 2
            };
            for (var i = 0; i < dataToEncrypt.Length; i += _encipherBlockSizeRsa)
            {
                var inputBlock = dataToEncrypt.Skip(i).Take(_encipherBlockSizeRsa).ToArray();
                encipheredBytes.AddRange(EncryptData(inputBlock));
                
            }
            return encipheredBytes.ToArray();
        }
        public byte[] DecryptDataPerBlock(byte[] dataToDecrypt)
        {
            var decipheredBytes = new List<byte>
            {
                Capacity = dataToDecrypt.Length /2
            };
            for (var i = 0; i < dataToDecrypt.Length; i += _decipherBlockSizeRsa)
            {
                var inputBlock = dataToDecrypt.Skip(i).Take(_decipherBlockSizeRsa).ToArray();
                decipheredBytes.AddRange(DecryptData(inputBlock));
                
            }
            return decipheredBytes.ToArray();
        }
        private void InitializeBlockSizes()
        {
            var keySizeInBytes = _rsa.KeySize / 8;
            _decipherBlockSizeRsa = keySizeInBytes;
            _encipherBlockSizeRsa = _decipherBlockSizeRsa - RSAConstants.PaddingOverheadPkcs1;
        }
    }