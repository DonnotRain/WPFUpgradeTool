using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Web;

namespace LaunchServer.Controllers
{
    public class ClientVersionController : ApiController
    {
        [Route("api/ClientVersion/LastestVersion")]
        public object GetVersionInfo()
        {
            var lastUpdateTime = ConfigurationManager.AppSettings["LastUpdateTime"].ToString();
            var currentVersion = ConfigurationManager.AppSettings["CurrentVersion"].ToString();
            return new { LastUpdateTime = lastUpdateTime, CurrentVersion = currentVersion };
        }

        [Route("api/ClientVersion")]
        public Dictionary<string, string> GetFilesInfo()
        {
            return WebHelpers.GetAllFilesInfo(HttpContext.Current.Server.MapPath("~/ClientBin"));
        }
    }
}
