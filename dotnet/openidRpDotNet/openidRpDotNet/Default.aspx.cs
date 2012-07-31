using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ExtremeSwank.OpenId;
using ExtremeSwank.OpenId.PlugIns.Extensions;
using openidrp.Code;
using ExtremeSwank.OAuth;

namespace openidrp
{
    public partial class _Default : System.Web.UI.Page
    {
        public String openID = null;

        public NameValueCollection extensionData = null;
        public AccessToken accessToken = null;

        protected void Page_Load(object sender, EventArgs eventArgs)
        {
            LoginInfo loginInfo = (LoginInfo)Session["loginInfo"];
            if (loginInfo != null)
            {
                openID = loginInfo.openID;
                extensionData = loginInfo.extensionData;
                accessToken = loginInfo.accessToken;
            }
            else
            {
                openID = null;
                extensionData = null;
                accessToken = null;
            }
            if (Request["logout"] == "true")
            {
                Session.RemoveAll();
            }
        }
    }
}
