using Jose;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using ToolBox.ExtensionMethods;
using BackEnd.OnActionHandle;
using BackEnd.Struct;
using System.Xml;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("Authorize")]
    [EnableCors("*", "*", "*")]
    [OpenApiTag("客戶端身分驗證", Description = "請登入取得授權驗證碼後，點擊界面上【Authorize】Button 進行設定")]
    [DomainFilter, Exception]
    public class AuthorizeController : ApiController
    {
        /// <summary>
        /// 取得伺服端公鑰
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetKey")]
        public object GetKey()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(RSA.PublicKey);
            return xml;
        }

        /// <summary>
        /// 登入，取得授權碼 (雙向加密)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("VerifyID")]
        public object VerifyID([FromBody] Packet obj)
        {
            if (RSA.Decrypt(obj, out string str))
            {
                var Info = JsonConvert.DeserializeObject<User_Info>(str);

                // 先透過 DB 去驗證人員 ID & Password
                // 驗證過後再給授權碼

                return new Response
                {
                    Message = "登入成功",
                    Token = new JWTToken().Create(
                        Info.ID,
                        DateTime.Now.ToCommonly(),
                        Request.GetUserIP()
                    ),
                    Data = true,
                };
            }
            else
            {
                throw new HttpException(401, "Verify failed");
            }
        }
    }



    /// <summary>
    /// 客戶端授權碼
    /// </summary>
    internal class JWTToken
    {
        #region 屬性
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User/Client IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 認證有效時間
        /// </summary>
        public DateTime Exp { get; set; }
        /// <summary>
        /// 登入時間
        /// </summary>
        public DateTime Login_Time { get => Login_Time_; }
        private DateTime Login_Time_;

        /// <summary>
        /// 對稱加密的固定加密 Key 值，建議把它建立在外部程序可修改的地方
        /// </summary>
        private static readonly string secretKey = RSA.PublicKey;
        /// <summary>
        /// 有效時間
        /// </summary>
        private int ExpMinutes = 10;
        #endregion 屬性


        #region 行為
        /// <summary>
        /// 建立授權碼
        /// </summary>
        /// <returns></returns>
        public string Create(string ID, string Name, string IP)
        {
            this.ID = ID;
            this.Name = Name;
            this.IP = IP;
            this.Exp = DateTime.Now.AddMinutes(ExpMinutes);
            this.Login_Time_ = DateTime.Now.Clone();

            var token = JWT.Encode(this.ToDictionary(), Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 解碼
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public JWTToken Decrypt(string token)
        {
            try
            {
                return JWT.Decode<JWTToken>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 更新認證時間
        /// </summary>
        /// <returns></returns>
        public string Refresh()
        {
            this.Exp = DateTime.Now.AddMinutes(ExpMinutes);
            return JWT.Encode(this.ToDictionary(), Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }


        private Dictionary<string, string> ToDictionary()
        {
            var data = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(this, null).ToString());

            return data;
        }
        #endregion 行為

    }

    /// <summary>
    /// 非對稱加密
    /// </summary>
    internal static class RSA
    {
        #region 屬性
        /// <summary>
        /// 伺服端公鑰
        /// </summary>
        public static string PublicKey
        {
            get
            {
                return rsa.ToXmlString(false);
            }
        }

        private static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
        #endregion 屬性


        #region 加密/解密
        /// <summary>
        /// 訊息加密
        /// </summary>
        /// <param name="Receiver_PublicKey"></param>
        /// <param name="Content">本文</param>
        /// <returns></returns>
        public static Content Encrypt(string Receiver_PublicKey, string Content)
        {
            Content safe = new Content();
            // 加密簽章
            using (var stream = Content.ToStream())
            {
                safe.Signature = rsa.SignData(stream, new MD5CryptoServiceProvider());
            }

            // 分段加密本文
            using (var stream = Content.ToStream())
            {
                using (var receiver = new RSACryptoServiceProvider())
                {
                    receiver.FromXmlString(Receiver_PublicKey);

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
        /// <param name="Packet">加密結構</param>
        /// <param name="Content">解密後本文</param>
        /// <returns></returns>
        public static bool Decrypt(Packet Packet, out string Content)
        {
            Content = null;
            int size = rsa.KeySize / 8;
            byte[] buffer = new byte[size];

            using (MemoryStream inputStream = new MemoryStream(Packet.Content.Ciphertext), outputStream = new MemoryStream())
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
                    var encryByte = rsa.Decrypt(temp, false);
                    outputStream.Write(encryByte, 0, encryByte.Length);
                }
                byte[] data = outputStream.ToArray();

                // 驗證簽章
                using (var publisher = new RSACryptoServiceProvider())
                {
                    publisher.FromXmlString(Packet.PublicKey);
                    if (publisher.VerifyData(data, new MD5CryptoServiceProvider(), Packet.Content.Signature))
                    {
                        Content = Encoding.UTF8.GetString(data);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion 加密/解密
    }
}