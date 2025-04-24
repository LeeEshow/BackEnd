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
using BackEnd.OnActionHandle;
using BackEnd.Struct;
using System.Xml;
using ToolBox.WEB;
using ToolBox.WEB.Struct;

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
        private static WebCryp WebCryp = new WebCryp(2048);

        /// <summary>
        /// 取得伺服端公鑰
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetKey")]
        public object GetKey()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(WebCryp.PublicKey);
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
            try
            {
                var Info = WebCryp.Decrypt<User_Info>(obj);
                // 取得 Info 後執行驗證

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
            catch (Exception ex)
            {
                throw new HttpException(500, ex.Message);
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
        private static readonly string secretKey = new WebCryp().PublicKey;
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
}