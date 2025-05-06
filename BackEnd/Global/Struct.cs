using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace BackEnd.Struct
{
    /// <summary>
    /// API 統一回覆結構
    /// </summary>
    public struct Response
    {
        /// <summary>
        /// 授權
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        public object Data { get; set; }
    }

}