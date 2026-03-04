using System.Security.Cryptography;
using System.Text;

namespace SmartMix.Core.Common.Security
{
    public static class Cryptor
    {
        private static TripleDES Create3DES(string key)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key));
            des.IV = new byte[des.BlockSize / 8];
            return des;
        }

        /// <summary>
        /// Шифровка
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptTextTo3DES(string plainText, string key)
        {
            //if (string.IsNullOrEmpty(plainText))
            //    return string.Empty;

            TripleDES des = Create3DES(key);
            ICryptoTransform ct = des.CreateEncryptor();
            byte[] input = Encoding.Unicode.GetBytes(plainText);
            byte[] resArr = ct.TransformFinalBlock(input, 0, input.Length);
            string result = Convert.ToBase64String(resArr);
            return result;
        }

        /// <summary>
        /// Дешифровка
        /// </summary>
        /// <param name="cypherText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptTextFrom3DES(string cypherText, string key)
        {
            //if (string.IsNullOrEmpty(cypherText))
            //    return string.Empty;

            byte[] b = Convert.FromBase64String(cypherText);
            TripleDES des = Create3DES(key);
            ICryptoTransform ct = des.CreateDecryptor();
            byte[] output = ct.TransformFinalBlock(b, 0, b.Length);
            return Encoding.Unicode.GetString(output);
        }
    }
}
