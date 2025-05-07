using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using ToolBox.WEB;

/// <summary>
/// 靜態擴充
/// </summary>
internal static class Global
{
    /// <summary>
    /// 雙向加密物件
    /// </summary>
    internal static Hedgehog Hedgehog = new Hedgehog(2048);

    /// <summary>
    /// 取得用戶IP
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    internal static string GetUserIP(this HttpRequestMessage obj)
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

    /// <summary>
    /// 從 HttpRequestMessage 組出一個簡易的「裝置指紋」
    /// </summary>
    internal static string GetDeviceFingerprint(this HttpRequestMessage request)
    {
        // 1. IP
        string ip = request.GetUserIP();  // 請自行實作 Extension 取得真實 client IP

        // 2. User-Agent
        string ua = request.Headers.UserAgent.ToString();

        // 3. Accept-Language
        string acceptLang = request.Headers.AcceptLanguage?
            .Select(h => h.ToString()).DefaultIfEmpty().Aggregate((a, b) => a + "," + b)?? "";

        // 4. Accept-Encoding
        string acceptEnc = request.Headers.AcceptEncoding?
            .Select(h => h.ToString()).DefaultIfEmpty().Aggregate((a, b) => a + "," + b)?? "";

        // 5. 合併成一個原始字串
        string raw = $"{ip}|{ua}|{acceptLang}|{acceptEnc}";

        // 6. SHA-256 雜湊後再轉 16 進位
        using (var sha = SHA256.Create())
        {
            byte[] data = Encoding.UTF8.GetBytes(raw);
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}