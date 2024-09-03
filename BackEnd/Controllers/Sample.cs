using System.Web.Http;
using NSwag.Annotations;
using System;
using ToolBox.ExtensionMethods;
using BackEnd.Struct;
using System.Net;
using System.Net.Http;
using BackEnd.OnActionHandle;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 測試
    /// </summary>
    [RoutePrefix("Sample"), OpenApiTag("Sample", Description = "功能測試中")]
    public class SampleController : BaseController
    {
        /// <summary>
        /// GET 測試
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        [Exception]
        [HttpGet, Route("GET")]
        public object GET([FromUri] double Value)
        {
            return Value * 2;
        }

        /// <summary>
        /// POST 測試
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("POST")]
        public object POST([FromBody] Response obj)
        {
            return obj.Message + ", " + DateTime.Now.ToCommonly();
        }
    }
}
