using System.Web.Http;
using System.Web.Http.Cors;
using NSwag.Annotations;
using BackEnd.OnActionHandle;

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
    }
}
