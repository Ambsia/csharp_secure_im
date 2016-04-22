using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;
namespace IMServer.utils
{
    public class Cryptography
    {
        public static string Sha1(string unHashedString)
        {
            //create an instance of sha1 cryptio provider
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            //use UTF-16 codec and extract bytes from the string
            byte[] originalBytes = Encoding.Unicode.GetBytes(unHashedString);
            //compute a hash with the bytes
            var encodedBytes = sha1.ComputeHash(originalBytes);
            //run a regular expression replacing all '-' characters with 
            //a colon and return as a lowercase string
            return Regex.Replace(BitConverter.ToString(encodedBytes), "-", ":").ToLower();
        }

        public static string HashPassword(string unhashedPassword, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(unhashedPassword,salt);
        }

        public static string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public static bool CompareValues(string compared, string actual)
        {
            return BCrypt.Net.BCrypt.Verify(compared, actual);
        }
    }

}
