using System.Web.Http;
using System;
using BackEnd.Struct;
using ToolBox.WEB.Struct;
using ToolBox.WEB;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using Newtonsoft.Json.Linq;
using BackEnd.FilterAttribute;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 測試
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("Sample")]
    public class SampleController : ApiController
    {
        /// <summary>
        /// GET 測試
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        [HttpGet, Route("GET"), NotEncrypt]
        public object GET([FromUri] double Value)
        {
            return Value * 2;
        }

        /// <summary>
        /// POST 測試
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Route("POST"), NotEncrypt]
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
        public object EncryptPOST([FromBody] object obj)
        {
            return new
            {
                Value = obj,
                Text = "被你找到秘密了"
            };
        }

        private void EncryptPOST_Client_Sample()
        {
            TwoWayCryp Client = new TwoWayCryp();
            var server_key = RESTful.Get(@"https://localhost:44388/Authorize/GetKey");
            var Info = new { ID = "Eshow", Password = "A123456" };

            var token = RESTful.Post<string>("https://localhost:44388/Authorize/VerifyID", Info);
            if (!RESTful.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                RESTful.Client.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            }

            //var data = RESTful.Get(@"https://localhost:44388/Sample/GET?Value=444");

            var packet = Client.Encrypt(server_key, new { ID = "123", Name = "456" });
            Console.WriteLine(packet.ToJsonString());
            var data = RESTful.Post<Packet>("https://localhost:44388/Sample/EncryptPOST", packet);

            var str = Client.Decrypt(data);
        }
    }
}
