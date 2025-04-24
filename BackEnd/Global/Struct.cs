using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace BackEnd.Struct
{
    #region Response
    /// <summary>
    /// API 統一回覆結構
    /// </summary>
    public struct Response
    {
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 授權
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        public object Data { get; set; }
    }
    #endregion Response


    internal class User_Info
    {
        public string ID;
        public string Password;
    }
}