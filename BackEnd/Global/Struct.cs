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


    #region Authorize
    /// <summary>
    /// 加密資訊
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// 公鑰
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// 內容
        /// </summary>
        public Content Content { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Content
    {
        /// <summary>
        /// 簽章
        /// </summary>
        public byte[] Signature { get; set; }
        /// <summary>
        /// 密文
        /// </summary>
        public byte[] Ciphertext { get; set; }
        /// <summary>
        /// 混淆視聽的欄位
        /// </summary>
        //public byte[] Text
        //{
        //    get
        //    {
        //        using (var crypto = new RNGCryptoServiceProvider())
        //        {
        //            var bytesarray = new byte[256];
        //            crypto.GetBytes(bytesarray);
        //            return bytesarray;
        //        }
        //    }
        //}
    }


    internal class User_Info
    {
        public string ID;
        public string Password;
    }
    #endregion Authorize
}