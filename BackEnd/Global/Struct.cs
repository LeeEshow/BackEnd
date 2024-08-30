using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd.Struct
{
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
        /// 更新授權碼
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        public object Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Safety
    {
        /// <summary>
        /// 簽章
        /// </summary>
        public byte[] Signature;
        /// <summary>
        /// 密文
        /// </summary>
        public byte[] Ciphertext;
    }
}