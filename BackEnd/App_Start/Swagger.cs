using Microsoft.Owin;
using NSwag;
using NSwag.AspNet.Owin;
using NSwag.Generation.Processors.Security;
using Owin;
using System.Collections.Generic;
using System.Web.Http;

[assembly: OwinStartup(typeof(Swagger.Startup))]

namespace Swagger
{
    /// <summary>
    /// 參考網址 https://blog.darkthread.net/blog/use-nswag-on-aspnet-webapi2/
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 應用程式配置
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            // 如需如何設定應用程式的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkID=316888
            var config = new HttpConfiguration();

            app.UseSwaggerUi(typeof(Startup).Assembly, settings =>
            {
                // 針對 WebAPI，指定路由包含 Action 名稱
                settings.GeneratorSettings.DefaultUrlTemplate = "api/{controller}/{action}/{id?}";

                // 加入客製化調整邏輯名稱版本等
                settings.PostProcess = document =>
                {
                    document.Info.Title = "WEB API Sample";
                    //document.Info.Description = "123456";
                    //document.Info.Version = "1.0";
                };

                // 加入 Authorization JWT token 定義
                settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("Key", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Description = "Type into the textbox value: Key {Authorization Code}.",
                    Scheme = "Key", // 不填寫會影響 Filter 判斷錯誤
                    BearerFormat = "JWT",
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    In = OpenApiSecurityApiKeyLocation.Header,
                }));

                settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor("Key"));
            });

            app.UseWebApi(config);
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
        }
    }
}