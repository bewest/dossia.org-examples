using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Specialized;
using ExtremeSwank.OAuth;

namespace openidrp.Code
{
    public class LoginInfo
    {
        public String openID;
        public NameValueCollection extensionData;
        public AccessToken accessToken;
    }
}
