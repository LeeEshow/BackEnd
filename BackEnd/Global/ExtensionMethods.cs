using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using ToolBox.WEB;

/// <summary>
/// 靜態擴充
/// </summary>
public static class Global
{
    /// <summary>
    /// 雙向加密物件
    /// </summary>
    public static WebCryp WebCryp = new WebCryp(2048);

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