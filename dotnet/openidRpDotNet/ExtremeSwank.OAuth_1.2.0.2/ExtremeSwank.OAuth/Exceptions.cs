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
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// The requested token could not be found.
    /// </summary>
    [Serializable]
    public class TokenNotFoundException : Exception
    {
        /// <summary>
        /// The requested OAuth token could not be found.
        /// </summary>
        public TokenNotFoundException() : base() { }
        /// <summary>
        /// The requested OAuth token could not be found.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public TokenNotFoundException(string message) : base(message) { }
        /// <summary>
        /// The requested OAuth token could not be found.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="exception">Inner exception.</param>
        public TokenNotFoundException(string message, Exception exception) : base(message, exception) { }
        /// <summary>
        /// The requested OAuth token could not be found.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected TokenNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// The token authorization request failed because the token
    /// has already been authorized.
    /// </summary>
    [Serializable]
    public class TokenAlreadyAuthorizedException : Exception
    {
        /// <summary>
        /// The token authorization request failed because the token
        /// has already been authorized.
        /// </summary>
        public TokenAlreadyAuthorizedException() : base() { }
        /// <summary>
        /// The token authorization request failed because the token
        /// has already been authorized.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public TokenAlreadyAuthorizedException(string message) : base(message) { }
        /// <summary>
        /// The token authorization request failed because the token
        /// has already been authorized.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="exception">Inner exception.</param>
        public TokenAlreadyAuthorizedException(string message, Exception exception) : base(message, exception) { }
        /// <summary>
        /// The token authorization request failed because the token
        /// has already been authorized.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected TokenAlreadyAuthorizedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// A required property has not been set.
    /// </summary>
    [Serializable]
    public class RequiredPropertyNotSetException : Exception
    {
        /// <summary>
        /// A required property has not been set.
        /// </summary>
        public RequiredPropertyNotSetException() : base() { }
        /// <summary>
        /// A required property has not been set.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RequiredPropertyNotSetException(string message) : base(message) { }
        /// <summary>
        /// A required property has not been set.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="exception">Inner exception.</param>
        public RequiredPropertyNotSetException(string message, Exception exception) : base(message, exception) { }
        /// <summary>
        /// A required property has not been set.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected RequiredPropertyNotSetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
