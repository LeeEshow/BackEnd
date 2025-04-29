using BackEnd.Controllers;
using BackEnd.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BackEnd.FilterAttribute
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
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;

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
            }
            #endregion 授權碼驗證        
        }
    }
    #endregion Token 驗證


    #region 統一例外處理
    /// <summary>
    /// Back-End 例外統一處理特性
    /// </summary>
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// 方法例外時
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            int StatusCode = 500;
            if (actionExecutedContext.Exception is HttpException)
            {
                StatusCode = (actionExecutedContext.Exception as HttpException).GetHttpCode();
            }

            actionExecutedContext.Response = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)StatusCode,
                ReasonPhrase = actionExecutedContext.Exception.Message,
                Content = new StringContent
                (
                    JsonConvert.SerializeObject(new Response 
                    { 
                        Token = null, 
                        Data = actionExecutedContext.Exception.Message
                    }),
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }
    }
    #endregion 統一例外處理

}

namespace BackEnd.Handler
{
    /// <summary>
    /// 統一 Response 結構
    /// </summary>
    public class ResponseHandler : DelegatingHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 先呼叫內層管線（Controller）
            var response = await base.SendAsync(request, cancellationToken);

            // 只包 2xx 
            if (response.IsSuccessStatusCode)
            {
                string token_str = null;
                if (request.Headers.Authorization != null) 
                {
                    var Token = new JWTToken().Decrypt(request.Headers.Authorization.Parameter);
                    token_str = Token.Refresh();
                }

                // 包裝成統一格式
                var wrapper = new Response
                {
                    Token = token_str,
                    Data = await response.Content.ReadAsAsync<object>(cancellationToken)
                };

                // 設定新 Content
                response.Content = new ObjectContent<Response>(
                    wrapper,
                    new JsonMediaTypeFormatter());

                // **在這裡設定 Cache-Control**
                response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromMinutes(10),
                    MustRevalidate = true
                };
            }

            return response;
        }
    }

}