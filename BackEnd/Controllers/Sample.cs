using System.Web.Http;
using NSwag.Annotations;
using System;
using BackEnd.Struct;
using System.Net;
using System.Net.Http;
using ToolBox.WEB.Struct;
using ToolBox.WEB;
using Newtonsoft.Json;
using System.Web.Http.Cors;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 測試
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("Sample"), OpenApiTag("Sample", Description = "功能測試中")]
    public class SampleController : ApiController
    {
        /// <summary>
        /// GET 測試
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
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
            return obj.Data + ", " + DateTime.Now.ToCommonly();
        }

        /// <summary>
        /// 雙向加密
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("EncryptPOST")]
        public object EncryptPOST([FromBody] Packet obj)
        {
            var value = Global.TwoWayCryp.Decrypt(obj);
            return Global.TwoWayCryp.Encrypt(obj.PublicKey, new 
            {
                Value = value,
                Text = "被你找到秘密了"
            });
        }

        private void EncryptPOST_Client_Sample()
        {
            TwoWayCryp Client = new TwoWayCryp();
            var server_key = RESTful.Get<Response>(@"https://localhost:44388/Authorize/GetKey");
            var Info = new { ID = "Eshow", Password = "A123456" };

            var res = RESTful.Post<Response>("https://localhost:44388/Authorize/VerifyID", Info);
            if (!RESTful.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                RESTful.Client.DefaultRequestHeaders.Add("Authorization", "Token " + res.Token);
            }

            //var data = RESTful.Get(@"https://localhost:44388/Sample/GET?Value=444");

            var packet = Client.Encrypt(server_key.Data.ToString(), new { A = "Test", B = 123456789 });
            var data = RESTful.Post<Response>("https://localhost:44388/Sample/EncryptPOST", packet);
            packet = JsonConvert.DeserializeObject<Packet>(data.Data.ToString());

            var str = Client.Decrypt(packet);
        }
    }
}
