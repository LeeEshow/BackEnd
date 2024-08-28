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
        public static HttpResponseException HttpException(this Exception obj, HttpStatusCode Code, string Message, string Content)
        {
            return new HttpResponseException(new HttpResponseMessage()
            {
                StatusCode = Code,
                ReasonPhrase = Message,
                Content = new StringContent
                (
                    JsonConvert.SerializeObject
                    (
                        new
                        {
                            Message = Message,
                            Content = Content
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                )
            });
        }
    }
}