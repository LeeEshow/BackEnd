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
using System.Web.Http;
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
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            // 預設 500 錯誤
            var code = HttpStatusCode.InternalServerError;
            var msg = "Server Error";

            if (context.Exception is HttpException httpEx)
            {
                code = (HttpStatusCode)httpEx.GetHttpCode();
                msg = httpEx.Message;
            }
            else if (context.Exception is HttpResponseException respEx)
            {
                code = respEx.Response.StatusCode;
                msg = respEx.Response.ReasonPhrase;
            }

            // 建議改用 CreateErrorResponse 可支援更多格式
            context.Response = context.Request.CreateErrorResponse(code, msg);
            return Task.CompletedTask;
        }
    }
    #endregion 統一例外處理


    #region 網域檢查
    /// <summary>
    /// 網域檢查
    /// </summary>
    public class DomainFilter : ActionFilterAttribute
    {
        private static readonly HashSet<string> AllowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "https://localhost:44388",
            "localhost:44388",
            "localhost:3000",
            // 未來可改從 ConfigurationManager.AppSettings["AllowedDomains"] 讀取
        };

        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override Task OnActionExecutingAsync(HttpActionContext context, CancellationToken cancellationToken)
        {
            // 取自 Origin header（較符合 CORS)
            string originOrHost = null;

            // 1. 優先嘗試從 Origin header 取值
            if (context.Request.Headers.TryGetValues("Origin", out var originValues))
            {
                originOrHost = originValues.FirstOrDefault();
            }
            // 2. 若無 Origin，再嘗試從 Host header 取值
            else if (!string.IsNullOrEmpty(context.Request.Headers.Host))
            {
                originOrHost = context.Request.Headers.Host;
            }
            // 3. 最後 fallback 到 RequestUri.Authority
            else
            {
                originOrHost = context.Request.RequestUri.Authority;
            }

            if (string.IsNullOrEmpty(originOrHost) || !AllowedDomains.Contains(originOrHost))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "Domain denied access");
            }
            return Task.CompletedTask;
        }
    }
    #endregion 網域檢查


    #region Token 認證
    /// <summary>
    /// Token 驗證
    /// </summary>
    public class TokenVerify : ActionFilterAttribute
    {
        private bool HasAttribute<T>(HttpActionContext ctx) where T : Attribute
            => ctx.ActionDescriptor.GetCustomAttributes<T>().Any()
            || ctx.ControllerContext.ControllerDescriptor.GetCustomAttributes<T>().Any();

        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnActionExecutingAsync(HttpActionContext context, CancellationToken cancellationToken)
        {
            // 標記 NotToken 就跳過
            if (HasAttribute<NotTokenAttribute>(context)) 
            {
                await base.OnActionExecutingAsync(context, cancellationToken);
                return;
            };

            var req = context.Request;
            if (req.Headers.Authorization == null || req.Headers.Authorization.Scheme != "Token")
                throw new HttpException(401, "Loss token");

            if(string.IsNullOrEmpty(req.Headers.Authorization.Parameter))
                throw new HttpException(401, "Loss token");

            var jwt = new Client().Decrypt(req.Headers.Authorization.Parameter);
            if (jwt == null)
                throw new HttpException(401, "Authorization is Invalid, please login again");

            var dpNow = req.GetDeviceFingerprint();
            if (jwt.Fingerprint != dpNow)
                throw new HttpException(401, "Device-Fingerprint not match, please login again");

            if (jwt.Exp < DateTime.UtcNow)
                throw new HttpException(401, "Authorization expired, please login again");
        }

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            await base.OnActionExecutedAsync(context, cancellationToken);

            // 成功回應才刷新 Token
            var req = context.Request;
            if (context.Response.IsSuccessStatusCode && req.Headers.Authorization != null)
            {
                var jwt = new Client().Decrypt(req.Headers.Authorization.Parameter);
                var newToken = jwt.Refresh();

                context.Response.Headers.Add("Token", newToken);
                context.Response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromMinutes(Client.ExpMinutes),
                    MustRevalidate = true
                };
            }
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
        private bool HasAttribute<T>(HttpActionContext ctx) where T : Attribute
            => ctx.ActionDescriptor.GetCustomAttributes<T>().Any()
            || ctx.ControllerContext.ControllerDescriptor.GetCustomAttributes<T>().Any();

        /// <summary>
        /// API 調用前觸發
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnActionExecutingAsync(HttpActionContext context, CancellationToken cancellationToken)
        {
            try
            {
                if (HasAttribute<NotEncryptAttribute>(context) || context.Request.Method == HttpMethod.Get)
                    return;

                // 1. 讀取 Packet
                var raw = await context.Request.Content.ReadAsStringAsync();
                var packet = JsonConvert.DeserializeObject<Packet>(raw);
                if (packet?.Ciphertext == null)
                    throw new InvalidOperationException("Missing ciphertext");

                // 2. 解密並反序列化 DTO
                var json = Global.Hedgehog.Decrypt(packet);
                var param = context.ActionDescriptor.ActionBinding.ParameterBindings.First();
                var dto = JsonConvert.DeserializeObject(json, param.Descriptor.ParameterType);
                context.ActionArguments[param.Descriptor.ParameterName] = dto;
            }
            catch
            {
                // **短路**：解密任何例外都當 403 回
                context.Response = context.Request.CreateErrorResponse(
                        HttpStatusCode.Forbidden, "Decryption verification failed, This channel is two-way encrypted");
                return;  // 跳過後續 action 和 ExceptionFilter
            }
        }

        /// <summary>
        /// API 調用後觸發
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            if (HasAttribute<NotEncryptAttribute>(context.ActionContext) || context.Request.Method == HttpMethod.Get)
                return;

            if (context.Response != null && context.Response.IsSuccessStatusCode)
            {
                // 解出原始物件
                var original = await context.Response.Content.ReadAsAsync<object>(new[] { new JsonMediaTypeFormatter() });
                // 加密為 Packet
                var rawReq = await context.Request.Content.ReadAsStringAsync();
                var reqPkt = JsonConvert.DeserializeObject<Packet>(rawReq);
                var encPkt = Global.Hedgehog.Encrypt(reqPkt.PublicKey, original);

                context.Response.Content = new ObjectContent<Packet>(encPkt, new JsonMediaTypeFormatter());
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
    /// 緩衝讀取處理器（保留原本實作即可）
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
            return await base.SendAsync(request, cancellationToken);
        }
    }

}