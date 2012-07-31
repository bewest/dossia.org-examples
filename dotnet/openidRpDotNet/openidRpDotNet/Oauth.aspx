<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oauth.aspx.cs" Inherits="openidrp.Oauth" %>
<head id="Head1" runat="server">
    <title>Home Page</title>
    <link type="text/css" rel="stylesheet" href="css/styles.css" />
</head>
<body>
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
        <% } // endif %>
    </div>
    
<% if (accessToken != null) { %>
    <div class="displayBlock">
        <p class="blockHeader">Make a call to the Dossia REST API</p>
        <form id="Oauth_rest" action="Oauth.aspx" method="post">
            <input id="Oauth_rest_url" type="text" name="restUrl" size="120" value="<%=restUrl %>" />
            <input id="Oauth_submit" name="OauthSubmit" type="submit" value="Submit" />
        </form>
<% if (restResponse != null) { %>
        <div class="highlightBlock">
            <p class="highlightHeader">Dossia REST Response</p>
            <%=restResponse %>
        </div>
<% } %>
    </div>
<% } // endif %>

<% if (accessToken != null) { %>
    <div class="displayBlock">
        <p class="blockHeader">Sso to the dossia dashboard</p>
        <form id="Form2" action="Oauth.aspx" method="post">
            <p>Click the button below to SSO over to the Dossia dashboard.</p>
            <input id="Sso_submit" name="SsoSubmit" type="submit" value="Submit" />
        </form>
    </div>
<% } // endif %>

</body>
</html>
