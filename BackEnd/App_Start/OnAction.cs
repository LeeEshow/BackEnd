using BackEnd.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ToolBox.ExtensionMethods;

namespace BackEnd.OnActionHandle
{
    // 參考網址 https://ithelp.ithome.com.tw/articles/10198206
    // 參考網址 https://ronsun.github.io/content/20180923-filters-of-webapi2.html

    /// <summary>
    /// 網域檢查
    /// </summary>
    public class DomainFilter : ActionFilterAttribute
    {
        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            #region 網域過濾
            // 設定允許的網域清單
            List<string> strAllowDomain = new List<string>()
            {
                "localhost:44388",
                "localhost:3000",
            };

            // 取出來自呼叫端的網域
            //string strOrigin = actionContext.Request.Headers.GetValues("Origin").FirstOrDefault();
            string strOrigin = actionContext.Request.Headers.Host;

            // 確認呼叫端的網域是否存在於允許的清單中
            bool blCheckDomain = strAllowDomain.Contains(strOrigin);

            // 如果不存在允許的網域清單，就回傳自訂的錯誤訊息
            if (!blCheckDomain)
            {
                throw new Exception().HttpException(HttpStatusCode.Forbidden, "來源拒絕存取", "Domain is not allow");
            }
            #endregion 網域過濾
        }
    }

    /// <summary>
    /// 安全性驗證
    /// </summary>
    public class SecurityVerify : ActionFilterAttribute
    {
        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            #region JWT Token
            var request = actionContext.Request;
            // 有取到 JwtToken 後，判斷授權格式不存在且不正確時
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
            {
                throw new Exception().HttpException(HttpStatusCode.Unauthorized, "請重新登入", "Lost Token");
            }
            else
            {
                var Token = new ClientToken().Decrpt(request.Headers.Authorization.Parameter);
                if (Token == null)
                {
                    throw new Exception().HttpException(HttpStatusCode.Unauthorized, "請重新登入", "Token Not Match");
                }
                if (Token.Exp < DateTime.Now)
                {
                    throw new Exception().HttpException(HttpStatusCode.Unauthorized, "請重新登入", "Token Expired");
                }

                // 麻煩一點可以弄非對稱加密
                // 帳號登入時產生密鑰+公鑰 > 密鑰由Server端保留 >公鑰打包進去 Token
                // 在這邊驗證時使用 或 傳遞的資訊透過加密處理
            }
            #endregion JWT Token
        }

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            #region 排除例外
            if (actionExecutedContext.Exception != null)
            {
                return;
            }

            var ignoreResult1 = actionExecutedContext.ActionContext.ActionDescriptor.GetCustomAttributes<IgnoreResultAttribute>().FirstOrDefault();
            var ignoreResult2 = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<IgnoreResultAttribute>().FirstOrDefault();
            if (ignoreResult1 != null || ignoreResult2 != null)
            {
                return;
            }
            if (ignoreResult1 != null || ignoreResult2 != null)
            {
                return;
            }
            #endregion 排除例外

            #region 統一 Response 結構
            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            var data = objectContent?.Value;

            var Token = new ClientToken().Decrpt(actionExecutedContext.Request.Headers.Authorization.Parameter);
            var response = new Response
            {
                Token = Token.Refresh(),
                Data = data
            };

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(response);
            #endregion 統一 Response 結構
        }

        #region class/struct
        private class IgnoreResultAttribute : Attribute
        {
        }
        private struct Response
        {
            public string Token;
            public object Data;
        }
        #endregion class/struct

    }




    /// <summary>
    /// API呼叫紀錄
    /// </summary>
    public class Logging : ActionFilterAttribute
    {
        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

        }

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            
        }
    }

}