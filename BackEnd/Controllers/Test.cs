using System.Web.Http;
using System.Web.Http.Cors;
using NSwag.Annotations;
using BackEnd.OnActionHandle;
using System;
using ToolBox.ExtensionMethods;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 測試
    /// </summary>
    [RoutePrefix("Test")]
    [EnableCors("*", "*", "*")]
    [OpenApiTag("Test", Description = "功能測試中")]
    [DomainFilter, Logging, SecurityVerify]
    public class TestController : ApiController
    {
        /// <summary>
        /// GET 測試
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GET")]
        public object GET([FromUri] int Value)
        {
            return Value * 2;
        }

        /// <summary>
        /// POST 測試
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("POST")]
        public object POST([FromBody] Response obj)
        {
            return obj.Message + ", " + DateTime.Now.ToCommonly();
        }
    }
}
