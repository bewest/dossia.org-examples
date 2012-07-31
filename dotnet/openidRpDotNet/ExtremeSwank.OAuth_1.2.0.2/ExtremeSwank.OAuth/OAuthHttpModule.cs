// Copyright (c) 2009 John Ehn, ExtremeSwank.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// OAuth-compatible IIdentity object.
    /// </summary>
    class OAuthIdentity : IIdentity
    {
        public OAuthIdentity(string name)
        {
            Name = name;
        }

        #region IIdentity Members

        public string AuthenticationType 
        {
            get { return "OAuth"; }
        }

        public bool IsAuthenticated 
        {
            get { return true; } 
        }

        public string Name { get; private set; }

        #endregion
    }

    /// <summary>
    /// OAuth-specific security principal, representing a single
    /// user and membership roles.
    /// </summary>
    public class OAuthPrincipal : IPrincipal
    {
        string[] _Scope;

        /// <summary>
        /// Creates a new instance of OAuthPrincipal.
        /// </summary>
        /// <param name="name">Name of the user account.</param>
        /// <param name="scope">Scope of the credentials.  A list of site-specific operations that these credentials can perform.</param>
        public OAuthPrincipal(string name, string[] scope)
        {
            Identity = new OAuthIdentity(name);
            _Scope = new string[scope.Length];
            scope.CopyTo(_Scope, 0);
        }

        /// <summary>
        /// The scope of the credentials. A list of site-specific operations that these credentials can perform.
        /// </summary>
        /// <returns>A string array of custom operations.</returns>
        public string[] Scope() 
        {
            string[] scope = new string[_Scope.Length];
            _Scope.CopyTo(scope, 0);
            return scope;
        }

        #region IPrincipal Members

        /// <summary>
        /// The identity object representing this user account.
        /// </summary>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// Checks the Roles provider to see if the account is in a given role.
        /// Always returns "true" if "OAuthUser" is specified.
        /// </summary>
        /// <param name="role">Role to check.</param>
        /// <returns>True if the account is in the role, false if not.</returns>
        public bool IsInRole(string role)
        {
            if (role.ToUpper(CultureInfo.InvariantCulture) == "OAUTHUSER")
            {
                return true;
            }
            if (Roles.Provider != null)
            {
                if (Roles.IsUserInRole(role)) return true;
            }
            return false;
        }

        #endregion
    }

    /// <summary>
    /// OAuth HttpModule for ASP.NET server-side authentication integration.
    /// </summary>
    public class OAuthHttpModule : IHttpModule
    {
        IServerTokenStore Store;

        #region IHttpModule Members

        /// <summary>
        /// Dispose any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes the HttpModule.
        /// </summary>
        /// <param name="context">The current ASP.NET application.</param>
        public void Init(HttpApplication context)
        {
            Store = OAuthServer.GetConfiguredStorageProvider();
            context.AuthenticateRequest += new EventHandler(context_AuthenticateRequest);
            context.PreSendRequestHeaders += new EventHandler(context_PreSendRequestHeaders);
        }

        /// <summary>
        /// If the current request results in an authentication failure, 
        /// send the OAuth authentication challenge in the HTTP headers.
        /// </summary>
        /// <param name="sender">The current HttpApplication.</param>
        /// <param name="e">Associated event arguments.</param>
        void context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;

            if (context.Response.StatusCode == 401)
            {
                string realm = OAuthUtility.Realm(context.Context.Request.Url);
                context.Response.AppendHeader("WWW-Authenticate", String.Format(CultureInfo.InvariantCulture, "OAuth realm=\"{0}\"", realm));
            }
        }

        /// <summary>
        /// Performs authentication on an OAuth authentication request, if the OAuth response
        /// is in the request's HTTP headers.
        /// </summary>
        /// <param name="sender">The current HttpApplication.</param>
        /// <param name="e">Associated event arguments.</param>
        void context_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;
            ServerAccessToken accessToken = OAuthServer.AuthenticateUser(context.Context.Request, Store);
            if (accessToken == null) return;

            string[] scope = null;
            if (accessToken.Parameters[OAuthArguments.Scope] != null)
            {
                scope = accessToken.Parameters[OAuthArguments.Scope].Split(' ');
            }
            context.Context.User = new OAuthPrincipal(accessToken.UserAccount, scope);
        }

        #endregion
    }
}
