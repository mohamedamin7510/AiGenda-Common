using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AI_genda_API.Services.TokenManagement;

public class TokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;

    public TokenEncryptionService(IConfiguration configuration)
    {
        // سحب المفتاح الثابت الموحد المرفوع على مونستر
        var secretKeyStr = configuration["Encryption:SecretKey"] 
            ?? throw new InvalidOperationException("Encryption SecretKey is missing from configuration.");
        _key = Convert.FromBase64String(secretKeyStr);
    }

    public string EncryptToken(string plainToken)
    {
        if (string.IsNullOrEmpty(plainToken))
            return plainToken;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        
        // خزن الـ IV في أول المصفوفة عشان نستخدمه في فك التشفير لاحقاً
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainToken);
            cs.Write(plainBytes, 0, plainBytes.Length);
            cs.FlushFinalBlock();
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string DecryptToken(string encryptedToken)
    {
        if (string.IsNullOrEmpty(encryptedToken))
            return encryptedToken;

        try
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedToken);
            using var aes = Aes.Create();
            aes.Key = _key;

            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            byte[] cipherText = new byte[fullCipher.Length - ivLength];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, ivLength);
            Buffer.BlockCopy(fullCipher, ivLength, cipherText, 0, cipherText.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            
            return sr.ReadToEnd();
        }
        catch (Exception)
        {
            // Guardrail: لو قابل سطر قديم بايظ متشفر بالـ Data Protection القديمة رجع نص فارغ بدل ما توقع الـ GET ALL كاملة
            return string.Empty; 
        }
    }
}