using System.Web;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Writes tracing information to the current HttpContext.
    /// </summary>
    internal static class Tracer
    {
        /// <summary>
        /// Writes a message to the current Trace context.
        /// </summary>
        /// <remarks>
        /// If running under ASP.NET, will write to the current HttpContext's Trace
        /// object.  For other environments, will write to System.Diagnostics.Trace.
        /// </remarks>
        /// <param name="message">Message to write.</param>
        internal static void Write(string message)
        {
#if TRACE
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Trace.Write("openid", message);
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation("OpenID: " + message);
            }
#endif
        }
        /// <summary>
        /// Writes a warning to the current Trace context.
        /// </summary>
        /// <remarks>
        /// If running under ASP.NET, will write to the current HttpContext's Trace
        /// object.  For other environments, will write to System.Diagnostics.Trace.
        /// </remarks>
        /// <param name="message">Warning to write.</param>
        internal static void Warn(string message)
        {
#if TRACE
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Trace.Warn("openid", message);
            }
            else
            {
                System.Diagnostics.Trace.TraceWarning("OpenID: " + message);
            }
#endif
        }
    }
}
