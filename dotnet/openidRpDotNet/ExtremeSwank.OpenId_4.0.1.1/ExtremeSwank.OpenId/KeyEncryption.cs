namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Represents the encryption type used during key exchange.
    /// </summary>
    internal enum KeyEncryption
    {
        /// <summary>
        /// No encryption used during key exchange operation.
        /// </summary>
        None,
        /// <summary>
        /// Diffie-Hellman key exchange will be used, and data will be
        /// signed using SHA1.
        /// </summary>
        DHSHA1,
        /// <summary>
        /// Diffie-Hellman key exchange will be used, and data will be
        /// signed using SHA256.
        /// </summary>
        DHSHA256
    };
}
