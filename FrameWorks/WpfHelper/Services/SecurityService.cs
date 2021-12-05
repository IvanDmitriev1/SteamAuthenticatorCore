using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WpfHelper.Services
{
    public static class SecurityService
    {
        private static readonly Random Random = new();

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new())
                {
                    using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public static string RandomKey(int size)
        {
            StringBuilder passwordBuilder = new();

            passwordBuilder.Append(RandomString(size / 4, true));

            passwordBuilder.Append(RandomNumber(1000, 9999));

            passwordBuilder.Append(RandomString(size / 4, true));

            passwordBuilder.Append(RandomNumber(1000, 9999));

            passwordBuilder.Append(RandomString(size / 4, true));

            passwordBuilder.Append(RandomNumber(1000, 9999));

            return passwordBuilder.ToString();
        }

        private static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max);
        }

        private static string RandomString(int size, bool lowerCase = false)
        {
            StringBuilder builder = new(size);

            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < size; i++)
            {
                char @char = (char)Random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
