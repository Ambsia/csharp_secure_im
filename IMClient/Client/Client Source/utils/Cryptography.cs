using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMClient.utils
{
    public static class Cryptography
    {
        private readonly static char[] SupportedCharacterArray = @"abcdefghijklmnopqrstuvwxyz.',-()?/><\|¬`}]{[&^%$£!\* ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray(); //supported characters that can be ciphered




        //store the positions of the characters of the message in an array
        //store the positions of the characters of the key in an array
        //iterate through, creating a new character array, at each position do char[i] = LowerAlphaCharacters[(keyPositions[i] + messagePositions[i]) % 26]
        //call the character array as a string and return it as the encrypted text
        public static string EncryptMessageWithPad(string key, string unencryptedMessage)
        {
            int[] positionOfCharInKey = FindCharacterPositionsOfString(key);
            int[] positionOfCharInMessage = FindCharacterPositionsOfString(unencryptedMessage);

            char[] cipherText = new char[positionOfCharInMessage.Length];

            for (int posInCipherText = 0; posInCipherText < cipherText.Length; posInCipherText++)
            {
                int pos = positionOfCharInKey[posInCipherText];
                int pos2 = positionOfCharInMessage[posInCipherText];
                int pos3 = (pos + pos2) % SupportedCharacterArray.Length;
                cipherText[posInCipherText] = SupportedCharacterArray[pos3];
            }
            return new string(cipherText);
        }


        public static string DecryptMessageWithPad(string key, string encryptedMessage)
        {
            int[] positionOfCharInKey = FindCharacterPositionsOfString(key);
            int[] positionOfCharInMessage = FindCharacterPositionsOfString(encryptedMessage);

            char[] unCipheredText = new char[positionOfCharInMessage.Length];

            for (int posInCipherText = 0; posInCipherText < unCipheredText.Length; posInCipherText++)
            {
                int pos = positionOfCharInKey[posInCipherText];
                int pos2 = positionOfCharInMessage[posInCipherText];
                int pos3 = (pos2 - pos)% SupportedCharacterArray.Length; //c# mod works a little different, and can return negative integers

                if (pos3 < 0)
                    pos3 += SupportedCharacterArray.Length;

                unCipheredText[posInCipherText] = SupportedCharacterArray[pos3];
            }

            return new string(unCipheredText);
        }

        private static int[] FindCharacterPositionsOfString(string value)
        {
            int[] positionOfCharsIntArray = new int[value.ToCharArray().Length];
            int charPosition = 0;
            foreach (char character in value)
            {
                positionOfCharsIntArray[charPosition++] = Array.IndexOf(SupportedCharacterArray, character);
            }
            return positionOfCharsIntArray;
        }



        private const string KeyContainerName = "RSAKeyContainer";


        public static string Sha1(string unHashedString)
        {

            SHA1 sha1 = new SHA1CryptoServiceProvider();
            Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(unHashedString);

            var encodedBytes = sha1.ComputeHash(originalBytes);

            return Regex.Replace(BitConverter.ToString(encodedBytes), "-", "").ToLower();
        }


        public static void GenerateKeys(out RSAParameters publicKey)
        {
            CspParameters cp = new CspParameters {KeyContainerName = KeyContainerName};

            using (var cryptoService = new RSACryptoServiceProvider(3072, cp))
            {
                publicKey = cryptoService.ExportParameters(false);
            }
        }


        public static string EncryptData(string unencryptedData, RSAParameters key)
        {

            using (var cryptoService = new RSACryptoServiceProvider())
            {
                cryptoService.ImportParameters(key);

                //get bytes of string
                byte[] bytesOfMessage = System.Text.Encoding.Unicode.GetBytes(unencryptedData);

                byte[] bytesCipher = cryptoService.Encrypt(bytesOfMessage, false);

                //return a string representation of the ciph
                return Convert.ToBase64String(bytesCipher);
            }
        }

        public static string DecryptData(string encryptedData)
        {
            CspParameters cp = new CspParameters {KeyContainerName = KeyContainerName};
            using (var cryptoService = new RSACryptoServiceProvider(cp))
            {
                byte[] bytesCipher = Convert.FromBase64String(encryptedData);


                byte[] bytesOfMessage = cryptoService.Decrypt(bytesCipher, false);

                return System.Text.Encoding.Unicode.GetString(bytesOfMessage);

            }
        }


        public static void DeleteKeyFromContainer()
        {
            CspParameters cp = new CspParameters {KeyContainerName = KeyContainerName};

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp) {PersistKeyInCsp = false};


            rsa.Clear();
        }

        public static string Encrypt<T>(string value, byte[] password, string salt)
     where T : SymmetricAlgorithm, new()
        {
            string pass = System.Text.Encoding.Unicode.GetString(password);
            DeriveBytes rgb = new Rfc2898DeriveBytes(pass, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }

                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        public static string Decrypt<T>(string text, byte[] password, string salt)
           where T : SymmetricAlgorithm, new()
        {
            string pass = System.Text.Encoding.Unicode.GetString(password);

            DeriveBytes rgb = new Rfc2898DeriveBytes(pass, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}

