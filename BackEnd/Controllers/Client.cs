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

namespace BackEnd.Controllers
{
    /// <summary>
    /// 使用者
    /// </summary>
    [RoutePrefix("Client")]
    [EnableCors("*", "*", "*")]
    [DomainFilter, Logging]
    public class ClientController : ApiController
    {
        /// <summary>
        /// 驗證人員身分並取得API授權
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("VerifyID")]
        public string VerifyID([FromUri] string ID, string Name ,string Password)
        {
            // 先透過 DB 去驗證人員 ID & Password
            // 驗證過後再給授權碼

            return new ClientToken().Create(ID, Name);
        }

        /// <summary>
        /// 更新授權
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Update_Authorization")]
        [SecurityVerify]
        public ClientToken Update_Authorization([FromUri] string ID)
        {
            var Token = new ClientToken().Decrpt(Request.Headers.Authorization.Parameter);
            Token.Refresh();
            return Token;
        }
    }



    /// <summary>
    /// 客戶端授權
    /// </summary>
    public class ClientToken
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
        /// 認證有效時間
        /// </summary>
        public DateTime Exp { get; set; }

        /// <summary>
        /// 對稱加密的固定加密 Key 值，建議把它建立在外部程序可修改的地方
        /// </summary>
        private static readonly string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; // 從 appSettings 取出
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
        public string Create(string ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
            this.Exp = DateTime.Now.AddMinutes(ExpMinutes);

            var token = JWT.Encode(this.ToDictionary(), Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 解碼
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClientToken Decrpt(string token)
        {
            try
            {
                return JWT.Decode<ClientToken>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
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


        /// <summary>
        /// 轉換型態
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> ToDictionary()
        {
            var data = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(this, null).ToString());

            return data;
        }
        #endregion 行為

    }


}