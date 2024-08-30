using BackEnd.OnActionHandle;
using BackEnd.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using ToolBox.ExtensionMethods;
using ToolBox.Common;
using System.Xml;
using System.IO;
using System.Text;

namespace BackEnd
{
    /// <summary>
    /// 想試試看 加密傳輸
    /// </summary>
    internal class Client
    {
        #region 屬性
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// User/Client IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 客戶端提供的公鑰
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// 伺服端公鑰
        /// </summary>
        public string Server_Key 
        {
            get
            {
                return RSA.ToXmlString(false);
            }
        }

        private RSACryptoServiceProvider RSA = null;

        #endregion 屬性


        #region 行為
        public Client(string PublicKey)
        {
            Key = PublicKey;
            new RSACryptoServiceProvider(2048);
        }

        #region 加密/解密
        /// <summary>
        /// 訊息加密
        /// </summary>
        /// <param name="Content">本文</param>
        /// <returns></returns>
        public Safety Encrypt(string Content)
        {
            Safety safe = new Safety();
            // 加密簽章
            using (var stream = Content.ToStream())
            {
                safe.Signature = RSA.SignData(stream, new MD5CryptoServiceProvider());
            }

            // 分段加密本文
            using (var stream = Content.ToStream())
            {
                using (var receiver = new RSACryptoServiceProvider())
                {
                    receiver.FromXmlString(this.Key);

                    byte[] inputByte = null;
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        inputByte = br.ReadBytes((int)stream.Length);
                    }

                    int size = receiver.KeySize / 8 - 11;
                    byte[] buffer = new byte[size];
                    using (MemoryStream inputStream = new MemoryStream(inputByte), outputStream = new MemoryStream())
                    {
                        while (true)
                        {
                            var readsize = inputStream.Read(buffer, 0, size);
                            if (readsize <= 0)
                            {
                                break;
                            }

                            var temp = new byte[readsize];
                            Array.Copy(buffer, 0, temp, 0, readsize);
                            var encryByte = receiver.Encrypt(temp, false);
                            outputStream.Write(encryByte, 0, encryByte.Length);
                        }
                        safe.Ciphertext = outputStream.ToArray();
                    }
                }
            }
            return safe;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Safety_Obj">加密結構</param>
        /// <param name="Content">解密後本文</param>
        /// <returns></returns>
        public bool Decrypt(Safety Safety_Obj, out string Content)
        {
            Content = null;
            int size = RSA.KeySize / 8;
            byte[] buffer = new byte[size];

            using (MemoryStream inputStream = new MemoryStream(Safety_Obj.Ciphertext), outputStream = new MemoryStream())
            {

                while (true)
                {
                    var readsize = inputStream.Read(buffer, 0, size);
                    if (readsize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readsize];
                    Array.Copy(buffer, 0, temp, 0, readsize);
                    var encryByte = RSA.Decrypt(temp, false);
                    outputStream.Write(encryByte, 0, encryByte.Length);
                }
                byte[] data = outputStream.ToArray();

                // 驗證簽章
                using (var publisher = new RSACryptoServiceProvider())
                {
                    publisher.FromXmlString(this.Key);
                    if (publisher.VerifyData(data, new MD5CryptoServiceProvider(), Safety_Obj.Signature))
                    {
                        Content = Encoding.UTF8.GetString(data);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion 加密/解密




        #endregion 行為
    }




}