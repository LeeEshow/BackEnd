using System.Web.Http;
using System.Web.Http.Cors;
using System;
using Advantech.Sample;
using System.Timers;
using Web_API;

namespace Router.Controllers
{
    [RoutePrefix("Abject/Product")]
    [EnableCors("*", "*", "*")]
    [CorsOnActionHandle]
    public class ProductController : ApiController
    {
        [HttpPost]
        [Route("Find")]
        public object Find([FromBody] Product obj)
        {
            try
            {
                return obj.Find();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }


}