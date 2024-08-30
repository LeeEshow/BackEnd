using Microsoft.Owin;
using NSwag;
using NSwag.AspNet.Owin;
using NSwag.Generation.Processors.Security;
using Owin;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Web.Http;

[assembly: OwinStartup(typeof(Swagger.Startup))]

namespace Swagger
{
    /// <summary>
    /// 參考網址 https://blog.darkthread.net/blog/use-nswag-on-aspnet-webapi2/
    /// </summary>
    public class Startup
    {
        private string Version 
        { 
            get 
            {
                try
                {
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                catch
                {
                    return "Debug model";
                }
            } 
        }

        /// <summary>
        /// 應用程式配置
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            app.UseSwaggerUi(typeof(Startup).Assembly, settings =>
            {
                // 針對 WebAPI，指定路由包含 Action 名稱
                settings.GeneratorSettings.DefaultUrlTemplate = "api/{controller}/{action}/{id?}";

                // 加入客製化調整邏輯名稱版本等
                settings.PostProcess = document =>
                {
                    document.Info.Title = "WEB API Sample";
                    //document.Info.Description = "RESTful API + Swagger 範例";
                    document.Info.Version = Version;
                };

                // 加入 Authorization JWT 定義
                settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("Token", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Description = "Type into the textbox value: Token {Authorization Code}.",
                    Scheme = "Token", // 不填寫會影響 Filter 判斷錯誤
                    BearerFormat = "JWT",
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    In = OpenApiSecurityApiKeyLocation.Header,
                }));

                settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor("Token"));
            });

            app.UseWebApi(config);
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
        }
    }
}