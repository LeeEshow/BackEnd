using BackEnd.FilterAttribute;
using BackEnd.Handler;
using System.Web.Http;
using static API.Server;

namespace Web_API
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // 1. 全域註冊
            config.MessageHandlers.Insert(0, new ResponseHandler());
            config.Filters.Add(new DomainFilter());
            config.Filters.Add(new TokenVerify());
            config.Filters.Add(new ExceptionFilter());

            // Web API 設定和服務
            config.EnableCors();
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
