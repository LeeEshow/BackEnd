using Jose;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using ToolBox.WEB;
using BackEnd.FilterAttribute;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [NotToken]
    [EnableCors("*", "*", "*")]
    [RoutePrefix("Authorize"), OpenApiTag("客戶端身分驗證", Description = "請登入取得授權驗證碼後，點擊界面上【Authorize】Button 進行設定")]
    public class AuthorizeController : ApiController
    {
        /// <summary>
        /// 取得伺服端公鑰
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetKey"), NotEncrypt]
        public object GetKey()
        {
            return Global.Hedgehog.PublicKey;
        }

        /// <summary>
        /// 登入，明碼驗證
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("VerifyID"), NotEncrypt]
        public object VerifyID([FromBody] User_Info obj)
        {
            try
            {
                // 取得 Info 後執行驗證
                var token = new JWTToken().Create(
                        obj.ID,
                        DateTime.Now.ToCommonly(),
                        Request.GetUserIP()
                    );

                return token;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.Message);
            }
        }

        /// <summary>
        /// 登入，加密驗證
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("EncryptVerifyID")]
        public object EncryptVerifyID([FromBody] User_Info obj)
        {
            try
            {
                // 取得 Info 後執行驗證
                var token = new JWTToken().Create(
                        obj.ID,
                        DateTime.Now.ToCommonly(),
                        Request.GetUserIP()
                    );

                return token;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.Message);
            }
        }

        #region struct
        /// <summary>
        /// 
        /// </summary>
        public class User_Info
        {
            /// <summary>
            /// 
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Password { get; set; }
        }
        #endregion struct

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
        /// 對稱加密的固定加密 Key 值
        /// </summary>
        private static readonly string secretKey = new Hedgehog().PublicKey.ToString();
        /// <summary>
        /// 有效時間
        /// </summary>
        internal static int ExpMinutes = 10;
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
}