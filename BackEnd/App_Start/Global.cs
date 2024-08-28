using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace BackEnd
{
    /// <summary>
    /// 靜態擴充
    /// </summary>
    public static class Global
    {
        public static HttpResponseException HttpException(this Exception obj, HttpStatusCode Code, string Message)
        {
            return new HttpResponseException(new HttpResponseMessage()
            {
                StatusCode = Code,
                ReasonPhrase = Message,
                Content = new StringContent
                (
                    JsonConvert.SerializeObject
                    (
                        new Response
                        {
                            Message = Message,
                            Key = "",
                            Data = null
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                )
            });
        }
    }

    /// <summary>
    /// API 統一回復結構
    /// </summary>
    public struct Response
    {
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// API 授權碼
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        public object Data { get; set; }
    }
}