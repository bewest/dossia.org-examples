<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="openidrp._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Home Page</title>
    <link type="text/css" rel="stylesheet" href="css/styles.css" />
</head>
<body>

    
    <div class="displayBlock">
        <p class="blockHeader">Login Info</p>
        <% if (Request["error"] != null)
           { %>
        <p class="error"><%=Request["error"]%></p>
        <% } %>
        
        <% if (openID == null)
           { %>
            You are not logged in.
        <% } else { %>
            <ul>
               <li><span class="dispLabel">OpenID : <span class="dispValue"><%=openID%></span></li>
            </ul>
        <% } // endif %>
        <p><strong><asp:Label ID="loginLabel" runat="server" /></strong></p>
        <p>Use the form below to log in. This will create an openid login request.</p>
        <ul>
            <li>If you select "<strong>Get Attributes</strong>", an attribute exchange extension will be included, requesting all of 
                the available attributes, as documented at 
                <a href="http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID" target="_blank">http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID</a>
            </li>
            <li>If you select "<strong>Get Oauth Request Token</strong>", an oauth exchange extension will be included, which will
                return a request token. With this request token, you can get an oauth access token. This will allow you to request or store data 
                using our REST API
                <a href="http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID" target="_blank">http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID</a>
            </li>
        </ul>
        <form action="OpenID.aspx" method="post">
            <input id="reqAttributes" type="checkbox" name="reqAttributes" value="1" />&nbsp;Get Attributes<br />
            <input id="reqOauthToken" type="checkbox" name="reqOauthToken" value="1" />&nbsp;Get Oauth Request Token<br />
            <input type="submit" value="Submit" />
        </form>
        <p>
            To log out, <a href="Default.aspx?logout=true">Click here</a>.
        </p>
    </div>
    <div class="displayBlock">
        <p class="blockHeader">Attribute Exchange responses</p>
        <% if (extensionData == null || extensionData.Count == 0)
           { %>
            <p>There are no attribute exchange attributes to display.</p>
        <% } else { %>
            <p>The following attributes were returned with the OpenID authentication response</p>
            <ul>
                <% foreach (String e in extensionData.AllKeys) { %>
                   <li><span class="dispLabel"><%=e%></span> : <span class="dispValue"><%=extensionData[e]%></span></li>
                <% } %>
            </ul>
        <% } // endif %>
    </div>
    <div class="displayBlock">
        <p class="blockHeader">Oauth Access Token</p>
        <% if (accessToken == null) { %>
        <p>There is no Oauth Access Token in your session.</p>
        <% } else { %>
        <ul>
               <li><span class="dispLabel">Key</span> : <span class="dispValue"><%=accessToken.Key%></span></li>
               <li><span class="dispLabel">Secret</span> : <span class="dispValue"><%=accessToken.Secret%></span></li>
               <% if (accessToken.Parameters != null) {
                      foreach (String e in accessToken.Parameters.AllKeys) { %>
                   <li><span class="dispLabel">Parameter <%=e%></span> : <span class="dispValue"><%=accessToken.Parameters[e]%></span></li>
                <%  }
                  } %>
        </ul>
        <p><a href="Oauth.aspx">Click here</a> to access the Oauth page. You can use this access token to get customer data, or to get an sso token.</p>
        <% } // endif %>
    </div>
    <div class="displayBlock">
        <p class="blockHeader">Configuration Settings</p>
        <p>The following settings are configured in the <strong>Web.config</strong> file.</p>
        <ul>
            <% foreach (String e in ConfigurationManager.AppSettings.AllKeys)
               { %>
               <li><span class="dispLabel"><%=e%></span> : <span class="dispValue"><%=ConfigurationManager.AppSettings[e]%></span></li>
            <% } %>
        </ul>
    </div>
</body>
</html>
