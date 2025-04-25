using System.Web.Http;
using NSwag.Annotations;
using System;
using BackEnd.Struct;
using System.Net;
using System.Net.Http;
using BackEnd.OnActionHandle;
using ToolBox.WEB.Struct;
using ToolBox.WEB;
using Newtonsoft.Json;

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

        /// <summary>
        /// 雙向加密
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Exception]
        [HttpPost, Route("EncryptPOST")]
        public object EncryptPOST([FromBody] Packet obj)
        {
            var value = Global.WebCryp.Decrypt(obj);
            return Global.WebCryp.Encrypt(obj.PublicKey, new 
            {
                Value = value,
                Text = "被你找到秘密了"
            });
        }

        private void EncryptPOST_Client_Sample()
        {
            // Client Sample

            WebCryp Client = new WebCryp();
            // 取得 Server 公鑰
            var server_key = RESTful.Get(@"https://localhost:44388/Authorize/GetKey");

            // 取得 Token
            var packet = Client.Encrypt(server_key, new { ID = "Eshow", Password = "A123456" });
            var res = RESTful.Post<Response>("https://localhost:44388/Authorize/VerifyID", packet);
            if (!RESTful.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                RESTful.Client.DefaultRequestHeaders.Add("Authorization", "Token " + res.Token);
            }

            // 加密傳輸
            packet = Client.Encrypt(server_key, new { ABC = DateTime.Now });
            var data = RESTful.Post<Response>("https://localhost:44388/Sample/EncryptPOST", packet);
            packet = JsonConvert.DeserializeObject<Packet>(data.Data.ToString());

            // 解密 Response
            var str = Client.Decrypt(packet);
        }
    }
}
