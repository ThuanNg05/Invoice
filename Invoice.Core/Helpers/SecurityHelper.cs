using System.Security.Cryptography;
using System.Text;

namespace Invoice.Core.Helpers;

public static class SecurityHelper
{
    private const string DefaultKey = "InvoiceApp_SecretKey_2024!@#"; // Recommendation: Change this and keep it secure
    private const string Prefix = "enc:";

    public static string Encrypt(string plainText, string? key = null)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        key ??= DefaultKey;
        var keyBytes = GetKeyBytes(key);

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.GenerateIV();
            var iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine IV and CipherText
                byte[] combinedBytes = new byte[iv.Length + encryptedBytes.Length];
                Buffer.BlockCopy(iv, 0, combinedBytes, 0, iv.Length);
                Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, iv.Length, encryptedBytes.Length);

                return Prefix + Convert.ToBase64String(combinedBytes);
            }
        }
    }

    public static string Decrypt(string encryptedTextWithPrefix, string? key = null)
    {
        if (string.IsNullOrEmpty(encryptedTextWithPrefix) || !encryptedTextWithPrefix.StartsWith(Prefix))
        {
            return encryptedTextWithPrefix;
        }

        string encryptedText = encryptedTextWithPrefix.Substring(Prefix.Length);
        key ??= DefaultKey;
        var keyBytes = GetKeyBytes(key);

        try
        {
            byte[] combinedBytes = Convert.FromBase64String(encryptedText);
            
            using (var aes = Aes.Create())
            {
                int ivLength = aes.BlockSize / 8;
                byte[] iv = new byte[ivLength];
                byte[] cipherText = new byte[combinedBytes.Length - ivLength];

                Buffer.BlockCopy(combinedBytes, 0, iv, 0, ivLength);
                Buffer.BlockCopy(combinedBytes, ivLength, cipherText, 0, cipherText.Length);

                aes.Key = keyBytes;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch
        {
            // If decryption fails, it might not be encrypted correctly, return original (without prefix if possible, or as is)
            return encryptedTextWithPrefix;
        }
    }

    private static byte[] GetKeyBytes(string key)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }
}
