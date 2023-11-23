using System.Web.Http;

namespace Web_API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 設定和服務
            config.EnableCors();
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Setting DB Connection
            Advantech.Database.MSSQL.Setting("172.22.246.152", "MES", "@Mes9527@", "M9_MESDB");

        }
    }
}
