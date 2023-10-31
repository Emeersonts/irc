using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Utilities
{
    public class RijndaelCryptographyStrategy : ICryptographyStrategy { 
        private const string BaseKey = "7hp8OgbIYCO/i7sZYd56D0G8Nl+vwLhAvcgoUqf52Qw="; 
        private const string BaseIv = "qRPwZuY5Vj28A3A//wboJXDoX4mhbwFndHcR/f44u9c="; 
        public string Encrypt(string clearText) 
        { 
            var rj = new RijndaelManaged() { Padding = PaddingMode.PKCS7, Mode = CipherMode.CBC, KeySize = 256, BlockSize = 256 };
            var key = Convert.FromBase64String(BaseKey); var iv = Convert.FromBase64String(BaseIv); 
            var encryptor = rj.CreateEncryptor(key, iv); var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write); 
            var toEncrypt = Encoding.ASCII.GetBytes(clearText);
            cryptoStream.Write(toEncrypt, 0, toEncrypt.Length); cryptoStream.FlushFinalBlock();
            var encrypted = memoryStream.ToArray(); return Convert.ToBase64String(encrypted); 
        }
        public string Decrypt(string encryptedText) 
        { 
           var rj = new RijndaelManaged() { Padding = PaddingMode.PKCS7, Mode = CipherMode.CBC, KeySize = 256, BlockSize = 256 };
           var encrypted = Convert.FromBase64String(encryptedText); var key = Convert.FromBase64String(BaseKey);
           var iv = Convert.FromBase64String(BaseIv); 
           var decryptor = rj.CreateDecryptor(key, iv); 
           using (var msDecrypt = new MemoryStream(encrypted)) 
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt)) { return srDecrypt.ReadToEnd(); 
                } 
        } 
    }

}
