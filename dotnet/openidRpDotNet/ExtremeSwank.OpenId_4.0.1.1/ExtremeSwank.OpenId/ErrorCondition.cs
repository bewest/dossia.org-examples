using System;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Possible error conditions.
    /// </summary>
    public enum ErrorCondition
    {
        /// <summary>
        /// No errors have occurred.
        /// </summary>
        NoErrors,
        /// <summary>
        /// No servers were found during the discovery process.
        /// </summary>
        NoServersFound,
        /// <summary>
        /// An HTTP error occurred when attempting to contact the IdP.
        /// </summary>
        HttpError,
        /// <summary>
        /// The window to complete the authentication request has expired.
        /// User should try the request again.
        /// </summary>
        SessionTimeout,
        /// <summary>
        /// Request has been actively refused by the IdP.
        /// </summary>
        RequestRefused,
        /// <summary>
        /// No ID was specified.
        /// </summary>
        NoIdSpecified,
        /// <summary>
        /// User cancelled the OpenID authentication request at the Identity Provider.
        /// </summary>
        RequestCanceled
    }

}
