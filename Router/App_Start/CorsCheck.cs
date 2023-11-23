using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

public class CorsOnActionHandle : ActionFilterAttribute
{
    /// <summary>
    /// 進行專案使用時的授權驗證
    /// </summary>
    /// <param name="actionContext"></param>
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        // 設定允許的網域清單
        List<string> strAllowDomain = new List<string>()
        {
            "https://localhost:44388",
            "http://localhost:3000",
        };

        // 取出來自呼叫端的網域
        string strOrigin = actionContext.Request.Headers.GetValues("Origin").FirstOrDefault();

        // 確認呼叫端的網域是否存在於允許的清單中
        bool blCheckDomain = strAllowDomain.Contains(strOrigin);

        // 如果不存在允許的網域清單，就回傳自訂的錯誤訊息
        if (!blCheckDomain)
        {
            UnauthorizedObject result = new UnauthorizedObject()
            {
                code = "401",
                message = "domain is not allow"
            };
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, result);
        }
    }

    public class UnauthorizedObject
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}