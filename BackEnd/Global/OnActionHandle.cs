using BackEnd.Controllers;
using BackEnd.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BackEnd.OnActionHandle
{
    // 參考網址 https://ithelp.ithome.com.tw/articles/10198206
    // 參考網址 https://ronsun.github.io/content/20180923-filters-of-webapi2.html

    #region 網域檢查
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
                throw new HttpException(403, "Domain denied access");
            }
        }
    }
    #endregion 網域檢查


    #region Token 驗證
    /// <summary>
    /// Token 驗證
    /// </summary>
    public class TokenVerify : ActionFilterAttribute
    {
        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            #region 授權碼驗證
            var request = actionContext.Request;
            // 先判斷 Header 內容中包含 Authorization
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Token")
            {
                throw new HttpException(401, "Login first");
            }
            else
            {
                // 解析 Authorization.Parameter
                var Token = new JWTToken().Decrypt(request.Headers.Authorization.Parameter);

                // Token 失效
                if (Token == null)
                {
                    throw new HttpException(401, "Authorization is Invalid, please login again");
                }
                // Token IP不吻合
                if (Token.IP != actionContext.Request.GetUserIP())
                {
                    throw new HttpException(401, "Authorization's IP not match, please login again");
                }
                // Token 過期
                if (Token.Exp < DateTime.Now)
                {
                    throw new HttpException(401, "Authorization expired, please login again");
                }

                // 麻煩一點可以弄非對稱加密
                // 帳號登入時產生密鑰+公鑰 > 密鑰由Server端保留 >公鑰打包進去 Token
                // 在這邊驗證時使用 或 傳遞的資訊透過加密處理
            }
            #endregion 授權碼驗證        
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

            var Token = new JWTToken().Decrypt(actionExecutedContext.Request.Headers.Authorization.Parameter);
            var response = new Response
            {
                Message = "Over",
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
        #endregion class/struct
    }
    #endregion Token 驗證


    #region 統一例外處理
    /// <summary>
    /// Back-End 例外統一處理特性
    /// </summary>
    public class ExceptionAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// 方法例外時
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            int StatusCode = 500;
            Response response = new Response { Token = null, Data = null };
            if (actionExecutedContext.Exception is HttpException)
            {
                StatusCode = (actionExecutedContext.Exception as HttpException).GetHttpCode();
                response.Message = actionExecutedContext.Exception.Message;
            }
            else
            {
                response.Message = "Service error";
            }

            actionExecutedContext.Response = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)StatusCode,
                ReasonPhrase = actionExecutedContext.Exception.Message,
                Content = new StringContent
                (
                    JsonConvert.SerializeObject(response),
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }
    }
    #endregion 統一例外處理
}