using BackEnd.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Http;

namespace BackEnd
{
    /// <summary>
    /// 靜態擴充
    /// </summary>
    public static class ExtensionMethods
    {
        // 2024-08-28 未實用封存
        private static HttpResponseException HttpException(this Exception obj, HttpStatusCode Code, string Message)
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

        /// <summary>
        /// 取得用戶IP
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetUserIP(this HttpRequestMessage obj)
        {
            if (obj.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)obj.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (obj.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)obj.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }

    }
}