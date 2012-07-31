namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Represents the mode used for authentication.
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// Represents Stateful (smart) authentication.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Stateful authentication, the client will first create a 
        /// cached shared secret with the Identity Provider. An authentication
        /// request will then be passed to the Identity Provider through the user's web browser.
        /// Once the user has been authenticated at the Identity Provider, the response
        /// is sent back to the client through the web browser. The client will then verify the
        /// validity of the response using the cached shared secret.
        /// </para>
        /// <para>
        /// Stateful mode requires less processing at the Identity Provider
        /// and gives faster response to the user.
        /// </para>
        /// </remarks>
        Stateful,
        /// <summary>
        /// Represents Stateless (dumb) authentication.
        /// </summary>
        /// <remarks>In Stateless authentication, the authentication request is
        /// sent immediately to the Identity Provider through the user's web browser.
        /// The Identity Provider will authenticate the user and will eventually
        /// respond with the requested information, also passing the data through the 
        /// web browser. The Consumer will then communicate with the Identity Provider 
        /// directly to confirm the validity of the data.</remarks>
        Stateless
    };
}
