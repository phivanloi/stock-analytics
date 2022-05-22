using Ardalis.GuardClauses;
using System.Security.Cryptography;
using System.Text;

namespace Pl.Sas.Core.Entities.Security
{
    public static class Cryptography
    {
        /// <summary>
        /// Mã hóa một chuỗi với 
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] AesEncryptString(string plainText, string key, string iv)
        {
            Guard.Against.NullOrWhiteSpace(plainText, nameof(plainText));
            Guard.Against.NullOrWhiteSpace(key, nameof(key));
            Guard.Against.NullOrWhiteSpace(iv, nameof(iv));

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using MemoryStream msEncrypt = new();
                using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }
            return encrypted;
        }

        /// <summary>
        /// Giải mã chuỗi mã hóa
        /// </summary>
        /// <param name="cipherText">Mảng dữ liệu mã hóa</param>
        /// <param name="key">Key mã hóa</param>
        /// <param name="iv">Vector mã hóa</param>
        /// <returns></returns>
        public static string AesDecryptString(byte[] cipherText, string key, string iv)
        {
            Guard.Against.NullOrEmpty(cipherText, nameof(cipherText));
            Guard.Against.NullOrWhiteSpace(key, nameof(key));
            Guard.Against.NullOrWhiteSpace(iv, nameof(iv));

            string? plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new(cipherText);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new(csDecrypt);
                plaintext = srDecrypt.ReadToEnd();

            }
            return plaintext;
        }

        /// <summary>
        /// Tạo key và iv mã hóa
        /// </summary>
        /// <returns>string Key, string Iv</returns>
        public static (string Key, string Iv) GenerateAesKey()
        {
            using Aes aesAlg = Aes.Create();
            return (Convert.ToBase64String(aesAlg.Key), Convert.ToBase64String(aesAlg.IV));
        }

        /// <summary>
        /// Hàm tạo mật khẩu để lưu trữ
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string CreateMd5Password(string password)
        {
            Guard.Against.NullOrEmpty(password, nameof(password));
            using var mD5 = MD5.Create();
            byte[] array = mD5.ComputeHash(Encoding.ASCII.GetBytes(password));
            StringBuilder stringBuilder = new();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.AppendFormat("{0:x2}", array[i]);
            }
            return stringBuilder.ToString().ToUpper();
        }
    }
}
