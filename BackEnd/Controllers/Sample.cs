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
        [HttpPost, Route("Encrypt/POST")]
        public object Encrypt_POST([FromBody] object obj)
        {
            return new
            {
                Value = obj,
                Text = "被你找到秘密了"
            };
        }

        private async void Encrypt_Sample()
        {
            Hedgehog Client = new Hedgehog();

            // 1. 取得 Server 公鑰
            var key = await RESTful.Get("https://localhost:44388/Authorize/GetKey");

            if (!RESTful.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                // 2. 加密登入訊息
                Packet credentials = Client.Encrypt(key, new { ID = "Eshow", Password = "A123456" });

                // 3. 呼叫登入 API，並反序列化為 Packet
                var packet = await RESTful.Post<Packet>("https://localhost:44388/Authorize/Encrypt/VerifyID", credentials);

                // 4. 解密回傳的 Packet，取得 Token
                string token = Client.Decrypt<string>(packet);

                RESTful.Client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");
            }


            // 5. 加密封包
            Packet pack = Client.Encrypt(key, DateTime.Now.yyyyMMddHHmmss());

            // 6. 呼叫加密後的 POST API，並反序列化為 Packet
            var packet_2 = await RESTful.Post<Packet>("https://localhost:44388/Sample/Encrypt/POST", pack);

            // 7. 解密回傳的 Packet
            var result = Client.Decrypt(packet_2);
            Console.WriteLine(result);
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
