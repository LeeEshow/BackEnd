﻿using Jose;
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
        /// 登入，取得授權碼
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("VerifyID")]
        public object VerifyID([FromUri] string ID, string Name ,string Password)
        {
            // 先透過 DB 去驗證人員 ID & Password
            // 驗證過後再給授權碼

            return new Response
            {
                Message = "登入成功",
                Token = new JWTToken().Create(
                    ID, 
                    Name, 
                    Request.GetUserIP()
                    ),
                Data = true,
            };
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
        public string Create(string ID, string Name, string IP)
        {
            this.ID = ID;
            this.Name = Name;
            this.IP = IP;
            this.Exp = DateTime.Now.AddMinutes(ExpMinutes);

            var token = JWT.Encode(this.ToDictionary(), Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 解碼
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public JWTToken Decrpt(string token)
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