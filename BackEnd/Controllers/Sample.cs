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
        public object POST([FromBody] Struct obj)
        {
            return obj.Message + ", " + DateTime.Now.yyyyMMddHHmmss();
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
            Hedgehog Client = new Hedgehog();

            // 1. 取得 Server Key
            string serverKey = RESTful.GetAsync("https://localhost:44388/Authorize/GetKey").Result;

            // 2. 登入以取得 Token
            var credentials = new { ID = "Eshow", Password = "A123456" };
            string token = RESTful.PostAsync<string>("https://localhost:44388/Authorize/VerifyID", credentials).Result;

            // 3. 設定 Authorization Header
            if (!RESTful.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                RESTful.Client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");
            }

            // 4. 加密封包
            var packet = Client.Encrypt(serverKey, new { ID = "123", Name = "456" });

            // 5. 呼叫加密後的 POST API，並反序列化為 Packet
            Packet data = RESTful.PostAsync<Packet>("https://localhost:44388/Sample/EncryptPOST", packet).Result;

            // 6. 解密回傳的 Packet
            var result = Client.Decrypt(data);
            Console.WriteLine(result.ToJsonString());
        }

        #region struct
        /// <summary>
        /// 
        /// </summary>
        public struct Struct
        {
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
        }
        #endregion struct

    }
}
