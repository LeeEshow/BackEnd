using BackEnd.OnActionHandle;
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
using System.Web.Http.Controllers;
using System.Web.Http.Cors;

namespace BackEnd.Controllers
{
    /// <summary>
    /// 基於 ApiController 提【抽象類別】為基礎以減少重複設定
    /// </summary>
    [EnableCors("*", "*", "*")]
    [DomainFilter, Logging, SecurityVerify, Exception]
    public abstract class BaseController : ApiController
    {
        
    }


}