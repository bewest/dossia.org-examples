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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Abstracted wrapper class
    /// </summary>
    abstract class AbstractWrapper
    {
        object Item;

        protected T GetValue<T>(string propName)
        {
            return (T)Item.GetType().GetProperty(propName).GetValue(Item, null);
        }

        protected void SetValue(string propName, object value)
        {
            Item.GetType().GetProperty(propName).SetValue(Item, value, null);
        }

        /* protected object Method(string methodName, params object[] args)
        {
            return Item.GetType().GetMethod(methodName).Invoke(Item, args);
        } */

        public AbstractWrapper(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// Abstracted HTTP Request object.
    /// </summary>
    /// <remarks>
    /// Since HttpListenerRequest and HttpRequest do not inherit from 
    /// the same base class, and there is no duck typing in C# 3.0, we need
    /// to create an abstraction so we don't have to write a separate code
    /// path for both options.
    /// </remarks>
    class AbstractRequest : AbstractWrapper
    {
        /// <summary>
        /// Create a new instance using an HttpRequest object.
        /// </summary>
        /// <param name="request">The current HTTP request.</param>
        public AbstractRequest(HttpRequest request) : base(request) { }
        /// <summary>
        /// Create a new instance using an HttpListenerRequest object.
        /// </summary>
        /// <param name="request">The current HTTP request.</param>
        public AbstractRequest(HttpListenerRequest request) : base(request) { }

        /// <summary>
        /// QueryString parameters in the request.
        /// </summary>
        public NameValueCollection QueryString
        {
            get 
            {
                return GetValue<NameValueCollection>("QueryString");
            }
        }
        /// <summary>
        /// Parameters in the POST body of the request.
        /// </summary>
        public NameValueCollection Form
        {
            get
            {
                return GetValue<NameValueCollection>("Form");
            }
        }
        /// <summary>
        /// HTTP Headers.
        /// </summary>
        public NameValueCollection Headers
        {
            get 
            {
                return GetValue<NameValueCollection>("Headers");
            }
        }

        /// <summary>
        /// HTTP request method.
        /// </summary>
        public string HttpMethod
        {
            get
            {
                return GetValue<string>("HttpMethod");
            }
        }

        /// <summary>
        /// Resource URI.
        /// </summary>
        public Uri Url
        {
            get
            {
                return GetValue<Uri>("Url");
            }
        }
    }

    /// <summary>
    /// Abstracted HTTP Response object.
    /// </summary>
    /// <remarks>
    /// Since HttpListenerResponse and HttpResponse do not inherit from 
    /// the same base class, and there is no duck typing in C# 3.0, we need
    /// to create an abstraction so we don't have to write a separate code
    /// path for both options.
    /// </remarks>
    class AbstractResponse : AbstractWrapper
    {
        /// <summary>
        /// Create a new instance using an HttpResponse.
        /// </summary>
        /// <param name="response">The current HTTP response.</param>
        public AbstractResponse(HttpResponse response) : base(response) { }
        /// <summary>
        /// Create a new instance using an HttpListenerResponse.
        /// </summary>
        /// <param name="response">The current HTTP response.</param>
        public AbstractResponse(HttpListenerResponse response) : base(response) { }

        /// <summary>
        /// Set the response status code.
        /// </summary>
        public int StatusCode
        {
            set { SetValue("StatusCode", value); }
        }

        /// <summary>
        /// Get the response output stream.
        /// </summary>
        public Stream OutputStream
        {
            get { return GetValue<Stream>("OutputStream"); }
        }
    }
}
