using System.Collections.Specialized;
using ExtremeSwank.OpenId.PlugIns.Extensions;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Receive and process unsolicited Attribute Exchange assertion messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Attribute Exchange specification supports subscribing to updates 
    /// whenever changes are made to values stored at the OpenID Provider.  
    /// Use AXReceiver to receive the updates whenever they are pushed 
    /// out from the Provider.
    /// </para>
    /// <para>
    /// Set AttributeExchange.UpdateUrl property during an Attribute Exchange
    /// Store request to subscribe to changes.  If supported by the OpenID
    /// Provider, the URL specified in the UpdateUrl property will be automatically
    /// notified whenever the subscribed values change.
    /// </para>
    /// <para>
    /// AttributeExchange.UpdateUrl should be set to the URL where AXReceiver
    /// is configured.
    /// </para>
    /// </remarks>
    /// <example>
    /// For ASP.NET:
    /// <code>
    /// using ExtremeSwank.OpenId;
    /// 
    /// public partial class _Default 
    /// {
    ///   protected void Page_Load(object sender, EventArgs e) 
    ///   {
    ///     if (!IsPostBack) 
    ///     {
    ///       AXReceiver axr = new AXReceiver();
    ///       OpenIdUser receivedData = axr.RetrieveUser();
    ///     }
    ///   }
    /// }
    /// </code>
    /// For other environments:
    /// <code>
    /// using ExtremeSwank.OpenId;
    /// 
    /// public static class MyClass 
    /// {
    ///   public static OpenIdUser ProcessAXResponse(NameValueCollection arguments) 
    ///   {
    ///     if (arguments == null) { throw new ArgumentNullException("arguments"); }
    ///     
    ///     // Create a new AXReceiver with the arguments
    ///     AXReceiver axr = new AXReceiver(arguments);
    ///     
    ///     // After processing, return the populated OpenIdUser object
    ///     // or null if response was invalid.
    ///     return axr.RetrieveUser();
    ///   }
    /// }
    /// </code>
    /// </example>
    public class AXReceiver
    {
        OpenIdClient openid;
        AttributeExchange ax;
        OpenIdUser user;

        /// <summary>
        /// Creates a new instance of AXReceiver.
        /// </summary>
        public AXReceiver()
        {
            openid = new OpenIdClient();
            ax = new AttributeExchange(openid);
        }

        /// <summary>
        /// Creates a new instance of AXReceiver.
        /// </summary>
        /// <param name="arguments">A collection of arguments received from a request.</param>
        public AXReceiver(NameValueCollection arguments)
        {
            openid = new OpenIdClient(arguments);
            ax = new AttributeExchange(openid);
        }

        /// <summary>
        /// Whether or not the request is an OpenID assertion.
        /// </summary>
        /// <returns>True if it is, false if not.</returns>
        private bool CheckMode()
        {
            if (Utility.GetRequestedMode(ax.Parent.RequestArguments) == RequestedMode.IdResolution)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check with the OpenID Provider to ensure the message is valid.
        /// </summary>
        /// <returns>True if the validation was successful, false if not.</returns>
        public bool ValidateResponse()
        {
            if (CheckMode())
            {
                return openid.ValidateResponse();
            }
            return false;
        }

        /// <summary>
        /// Validate the authentication response, if present.
        /// </summary>
        /// <returns>A populated OpenIdUser object, if the response is valid.  Null if the response
        /// is invalid.</returns>
        public OpenIdUser RetrieveUser()
        {
            if (!ValidateResponse()) { return null; }
            user = openid.RetrieveUser();
            return user;
        }
    }
}
