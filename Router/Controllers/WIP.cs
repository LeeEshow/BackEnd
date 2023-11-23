using System.Web.Http;
using System.Web.Http.Cors;
using System;
using Advantech.Sample;

namespace Router.Controllers
{
    [RoutePrefix("Abject/WIP")]
    [EnableCors("*", "*", "*")]
    [CorsOnActionHandle]
    public class WIPController : ApiController
    {
        [HttpPost]
        [Route("Find")]
        public object Find([FromBody] WIP obj)
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


        [HttpPost]
        [Route("GetProducts_")]
        public object GetProducts_([FromBody] WIP obj)
        {
            try
            {
                return obj.GetProducts();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        // Sample 預載方法
        #region Preload
        static Preload_Data preload = null;
        [HttpPost]
        [Route("GetProducts")]
        public object GetProducts([FromBody] WIP obj)
        {
            try
            {
                if (preload == null)
                {
                    preload = new Preload_Data
                    {
                        Log_Time = DateTime.Now,
                        WIP = obj,
                        Data = obj.GetProducts(),
                    };
                }
                lock (preload)
                {
                    if (preload.Log_Time.AddMinutes(1).Minute <= DateTime.Now.Minute ||
                        preload.WIP.ID != obj.ID)
                    {
                        preload = new Preload_Data
                        {
                            Log_Time = DateTime.Now,
                            WIP = obj,
                            Data = obj.GetProducts(),
                        };
                    }
                }

                return preload;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        #endregion Preload
    }

    public class Preload_Data
    {
        public DateTime Log_Time;

        public WIP WIP;

        public object Data;
    }
}