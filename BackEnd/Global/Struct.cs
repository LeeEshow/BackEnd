using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 更新授權碼
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
    /// 加密傳輸
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// 簽章
        /// </summary>
        public byte[] Signature { get; set; }
        /// <summary>
        /// 密文
        /// </summary>
        public byte[] Ciphertext { get; set; }
    }
    /// <summary>
    /// 身分驗證
    /// </summary>
    public class VerifyID 
    {
        /// <summary>
        /// 客戶端公鑰
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// 登入資訊
        /// </summary>
        public Packet Packet { get; set; }
    }


    internal class User_Info
    {
        public string ID;
        public string Password;
    }
    #endregion Authorize
}