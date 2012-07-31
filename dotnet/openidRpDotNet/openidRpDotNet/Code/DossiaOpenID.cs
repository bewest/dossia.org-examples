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
using ExtremeSwank.OpenId;
using ExtremeSwank.OpenId.PlugIns.Extensions;
using ExtremeSwank.OpenId.Persistence;
using System.Web.SessionState;

namespace openidrp.Code
{

    public class DossiaOpenID
    {
        private SingularAssociationManager associationManager;
        private SingularSessionManager sessionManager;
        public DossiaOpenID(HttpApplicationState application, HttpSessionState session)
        {
            associationManager = initAssociationManager(application);
            sessionManager = initSessionManager(session);
        }

        // todo: serialize access to this method
        private SingularAssociationManager initAssociationManager(HttpApplicationState application)
        {
            SingularAssociationManager returnValue = (SingularAssociationManager)application["dossia.openid.associationManager"];
            if (returnValue == null)
            {
                returnValue = new SingularAssociationManager();
                application["dossia.openid.associationManager"] = returnValue;
            }
            return returnValue;
        }

        private SingularSessionManager initSessionManager(HttpSessionState session)
        {
            SingularSessionManager returnValue = (SingularSessionManager)session["dossia.openid.sessionManager"];
            if (returnValue == null)
            {
                returnValue = new SingularSessionManager();
                session["dossia.openid.sessionManager"] = returnValue;
            }
            return returnValue;
        }

        public OpenIdClient CreateOpenIdClient()
        {
            OpenIdClient returnValue = new OpenIdClient();
            
            returnValue.EnableStatefulMode(associationManager, sessionManager);
            // todo: make this a property
            returnValue.Identity = System.Configuration.ConfigurationManager.AppSettings["openid.default.openid"];
            return returnValue;
        }

        public void InitializeForSending(OpenIdClient openIdClient)
        {
            openIdClient.TrustRoot = System.Configuration.ConfigurationManager.AppSettings["openid.realm"];
        }
    }
}
