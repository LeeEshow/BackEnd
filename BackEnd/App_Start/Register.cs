using BackEnd.Controllers;
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
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ToolBox.WEB.Struct;

namespace BackEnd.FilterAttribute
{
    // 參考網址 https://ithelp.ithome.com.tw/articles/10198206
    // 參考網址 https://ronsun.github.io/content/20180923-filters-of-webapi2.html

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
            string message = "Server Error";
            if (actionExecutedContext.Exception is HttpException)
            {
                StatusCode = (actionExecutedContext.Exception as HttpException).GetHttpCode();
                message = actionExecutedContext.Exception.Message;
            }

            actionExecutedContext.Response = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)StatusCode,
                //ReasonPhrase = actionExecutedContext.Exception.Message,
                Content = new StringContent
                (
                    JsonConvert.SerializeObject(message),
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }
    }
    #endregion 統一例外處理


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


    #region Token 認證
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
            if (actionContext.ActionDescriptor.GetCustomAttributes<NotTokenAttribute>().Any() || 
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<NotTokenAttribute>().Any())
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

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            #region 授權碼更新
            if (actionExecutedContext.Response.IsSuccessStatusCode)
            {
                if (actionExecutedContext.Request.Headers.Authorization != null)
                {
                    var Token = new JWTToken().Decrypt(actionExecutedContext.Request.Headers.Authorization.Parameter);

                    actionExecutedContext.Response.Headers.Add("Token", Token.Refresh());
                    actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(JWTToken.ExpMinutes),
                        MustRevalidate = true
                    };
                }
            }
            #endregion 授權碼更新
        }
    }

    /// <summary>
    /// 不執行 雙向非對稱性加密
    /// </summary>
    public class NotTokenAttribute : Attribute
    {

    }
    #endregion Token 認證


    #region 雙向加密/解密
    /// <summary>
    /// 雙向加密/解密
    /// </summary>
    public class TWEncryptVerify : ActionFilterAttribute
    {
        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.ActionDescriptor.GetCustomAttributes<NotEncryptAttribute>().Any() ||
                    actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<NotEncryptAttribute>().Any() ||
                    actionContext.Request.Method.ToString() == "GET")
                {
                    return;
                }

                // 1. 讀取整個 Request Body（JSON 格式的 Packet）
                var raw = actionContext.Request.Content.ReadAsStringAsync().Result;
                var packet = JsonConvert.DeserializeObject<Packet>(raw);

                // 2. 解密：取得原始的 JSON 字串
                var data = Global.TwoWayCryp.Decrypt(packet);

                // 3. 反序列化成 Action 的 DTO 參數
                //    這裡假設 Action 只有一個參數，且名稱與型別可動態取得
                var binding = actionContext.ActionDescriptor.ActionBinding.ParameterBindings[0];
                var paramType = binding.Descriptor.ParameterType;
                var dto = JsonConvert.DeserializeObject(data, paramType);

                // 4. 用解好的 DTO 覆寫原本的 ActionArguments
                actionContext.ActionArguments[binding.Descriptor.ParameterName] = dto;
            }
            catch
            {
                throw new HttpException(403, "TWEncrypt Error");
            }
        }

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                if (actionExecutedContext.ActionContext.ActionDescriptor.GetCustomAttributes<NotEncryptAttribute>().Any() ||
                    actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<NotEncryptAttribute>().Any() ||
                    actionExecutedContext.Request.Method.ToString() == "GET")
                {
                    return;
                }

                var raw = actionExecutedContext.Request.Content.ReadAsStringAsync().Result;
                var encrypt_packet = JsonConvert.DeserializeObject<Packet>(raw);

                var response = actionExecutedContext.Response;
                if (response != null && response.IsSuccessStatusCode)
                {
                    // 1. 讀取 Action 回傳的物件（已被 Web API 序列化成物件）
                    //    這裡用 ReadAsAsync<object>，也可以改成具體型別
                    var originalObj = response.Content
                                              .ReadAsAsync<object>(new[] { new JsonMediaTypeFormatter() })
                                              .Result;

                    // 2. 加密成 Packet
                    var packet = Global.TwoWayCryp.Encrypt(encrypt_packet.PublicKey, originalObj);

                    // 3. 將 Response.Content 換成新的 Packet JSON
                    actionExecutedContext.Response.Content =
                        new ObjectContent<Packet>(
                            packet,
                            new JsonMediaTypeFormatter()
                        );
                }
            }
            catch
            {
                throw new HttpException(403, "TWEncrypt Error");
            }
        }
    }

    /// <summary>
    /// 不執行 雙向非對稱性加密
    /// </summary>
    public class NotEncryptAttribute : Attribute
    {

    }
    #endregion 雙向加密/解密


}

namespace BackEnd.Handler
{
    /// <summary>
    /// 
    /// </summary>
    public class BufferHandler : DelegatingHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await request.Content.LoadIntoBufferAsync();
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }

}