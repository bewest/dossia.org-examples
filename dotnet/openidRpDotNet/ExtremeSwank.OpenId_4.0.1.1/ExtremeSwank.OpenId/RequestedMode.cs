namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// OpenID modes that can be remotely requested.
    /// </summary>
    public enum RequestedMode
    {
        /// <summary>
        /// No OpenID mode was requested.
        /// </summary>
        None,
        /// <summary>
        /// ID Resolution mode was requested.
        /// </summary>
        IdResolution,
        /// <summary>
        /// Operation was cancelled by user.
        /// </summary>
        CanceledByUser,
        /// <summary>
        /// Immediate request determined that user agent is not logged into the OpenID Provider.
        /// Must run a standard authentication request.
        /// </summary>
        SetupNeeded,
        /// <summary>
        /// Error message received from the OpenID Provider.
        /// </summary>
        Error
    }
}
