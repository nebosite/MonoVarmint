using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoVarmint.Tools
{
    public class VarmintCrypto
    {
        /// <summary>
        /// 16 bytes to help randomize data.  The decryptor must have the same
        /// characters to decrypt the data.  By default, the bytes are pulled
        /// from VarmintId.GetLocalDeviceId()
        /// </summary>
        public byte[] InitialVector { get; set; }

        /// <summary>
        /// Override this to provide a more secure offering for the key.  If you 
        /// leave this alone, a default key will be provided, which will encrypt the
        /// content, but it won't be very secure against a determined attack.
        /// Should return 32 bytes
        /// </summary>
        public Func<byte[]> GetKeyBytes { get; set; }

        //--------------------------------------------------------------------------------------
        // ctor
        //--------------------------------------------------------------------------------------
        public VarmintCrypto()
        {
            InitialVector = VarmintId.GetLocalDeviceId().ToByteArray();
            GetKeyBytes = () =>
            {
                return Encoding.ASCII.GetBytes("bX1ABQklYtp+9eMc4kByd8OEaCMGiZc4");
            };
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        public string Encrypt(string PlainText)
        {
            if (string.IsNullOrEmpty(PlainText)) return "";

            byte[] PlainTextBytes = Encoding.UTF8.GetBytes(PlainText);
            var SymmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
            byte[] encryptedBytes = null;

            using (ICryptoTransform Encryptor = SymmetricKey.CreateEncryptor(GetKeyBytes(), InitialVector))
            {
                using (MemoryStream MemStream = new MemoryStream())
                {
                    using (CryptoStream CryptoStream = new CryptoStream(MemStream, Encryptor, CryptoStreamMode.Write))
                    {
                        CryptoStream.Write(PlainTextBytes, 0, PlainTextBytes.Length);
                        CryptoStream.FlushFinalBlock();
                        encryptedBytes = MemStream.ToArray();
                    }
                }
            }

            SymmetricKey.Clear();
            return Convert.ToBase64String(encryptedBytes);
        }

        //--------------------------------------------------------------------------------------
        // 
        //--------------------------------------------------------------------------------------
        public string Decrypt(string CipherText)
        {
            if (string.IsNullOrEmpty(CipherText)) return "";

            byte[] CipherTextBytes = Convert.FromBase64String(CipherText);
            var SymmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
            byte[] PlainTextBytes = new byte[CipherTextBytes.Length];
            int ByteCount = 0;

            var bytes = GetKeyBytes();
            using (ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(GetKeyBytes(), InitialVector))
            {
                using (MemoryStream MemStream = new MemoryStream(CipherTextBytes))
                {
                    using (CryptoStream CryptoStream = new CryptoStream(MemStream, Decryptor, CryptoStreamMode.Read))
                    {

                        ByteCount = CryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
                        MemStream.Close();
                        CryptoStream.Close();
                    }
                }
            }

            SymmetricKey.Clear();
            return Encoding.UTF8.GetString(PlainTextBytes, 0, ByteCount);
        }

    }
}
