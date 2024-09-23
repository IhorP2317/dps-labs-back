using System.Security.Cryptography;
using System.Text;
using dbs_labs_back.Utils;

namespace dbs_labs_back.test;

 public class UnitTest1
    {
        [Theory]
        [InlineData("", "D41D8CD98F00B204E9800998ECF8427E")]
        [InlineData("a", "0CC175B9C0F1B6A831C399E269772661")]
        [InlineData("abc", "900150983CD24FB0D6963F7D28E17F72")]
        [InlineData("message digest", "F96B697D7CB7938D525A2F31AAF161D0")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "C3FCD3D76192E4007DFB496CCA67E13B")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "D174AB98D277D9F5A5611C2C9F419D9F")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "57EDF4A22BE3C955AC49DA2E2107B67A")]
        public void HashesString(string input, string expectedHash)
        {
            var hasher = new Md5Generator();
            hasher.ComputeHash(input);

            Assert.Equal(hasher.HashAsString.ToUpper(), expectedHash);
        }

        [Theory]
        [InlineData("", "D41D8CD98F00B204E9800998ECF8427E")]
        [InlineData("a", "0CC175B9C0F1B6A831C399E269772661")]
        [InlineData("abc", "900150983CD24FB0D6963F7D28E17F72")]
        [InlineData("message digest", "F96B697D7CB7938D525A2F31AAF161D0")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "C3FCD3D76192E4007DFB496CCA67E13B")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "D174AB98D277D9F5A5611C2C9F419D9F")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "57EDF4A22BE3C955AC49DA2E2107B67A")]
        public void HashesBytes(string input, string expectedHash)
        {
            var hasher = new Md5Generator();
            hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            Assert.Equal(expectedHash, hasher.HashAsString.ToUpper());
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("abc")]
        [InlineData("message digest")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890")]
        [InlineData("SYBi4V6eGlUlSLiwvDHCH7vGylhzX6s1ZcL4lCYk2eyNUIWQ9J3WWCA9CGfRMPHqtELFPxn8erNCAiHwTY5aHHhj53gPoYSKxqlXSHTy0wv28cZWvQZPy2QAKH1X7gUnuC9BjhBahfNNBhbRdETzFMlk93CU99fRohu9ZZnk2mTYesRUfwfdiImSYDa3XRYC2bNw6FNiOU2VhBAFgq9J6BOZ0g4PbRgeUy2rXZeEV8lqH1Wdvx5NXL9BZDevOyUuJQY2t4AvYTSusYIFEgyHOORUuV3eI79VwjCsxYSXvksPJNwhf26NvSwgW8QxuW3VvwQ8GhuSL4Qu4EtVN1O5hNWuaD0E88TzzrvdhH0yUobv1okztRS9KeJfnWtYz2NdB3iNReJug5J8GrJP9viKO09BIuBHtpzbvt1sBxVk3Pe4i2NiWWvCn7P3")]
        public void HashesSameAsCryptoImpl(string input)
        {
            var hasher = new Md5Generator();
            var myHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hash = CreateMD5Bytes(input);

            Assert.Equal(CreateMD5(input).ToUpper(), hasher.HashAsString.ToUpper());
            Assert.True(myHash.ToByteArray().SequenceEqual(hash));
        }

        [Theory]
        [InlineData("TestFile.txt")]
        [InlineData("TestFile_2.txt")]
        [InlineData("test_file")]
        public void HashesFileSameAsCryptoImpl(string filePath)
        {
            var message = File.ReadAllBytes(filePath);
            var hasher = new Md5Generator();
            hasher.ComputeFileHashAsync(filePath).Wait();

            Assert.Equal(CreateMD5(message).ToUpper(), hasher.HashAsString.ToUpper());
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("abc")]
        [InlineData("message digest")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        public void ComparesMessageDigests(string input)
        {
            var hasher = new Md5Generator();

            var firstMd = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            var secondMd = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            Assert.True(firstMd.Equals(secondMd));
        }

        public static string CreateMD5(string input)
        {
            return CreateMD5(Encoding.UTF8.GetBytes(input));
        }

        public static byte[] CreateMD5Bytes(string input)
        {
            using var md5 = MD5.Create();

            return md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string CreateMD5(byte[] inputBytes)
        {
            using var md5 = MD5.Create();
            byte[] hashedBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                sb.Append(hashedBytes[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }