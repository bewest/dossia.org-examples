using System;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Primitive used with Extension plugins. Links aliases to namespace URIs.
    /// </summary>
    public class SchemaDef
    {
        private string x;
        private Uri y;

        /// <summary>
        /// Create a new SchemaDef.
        /// </summary>
        /// <param name="alias">Alias to link.</param>
        /// <param name="uri">Namespace URI.</param>
        public SchemaDef(string alias, string uri)
        {
            x = alias;
            y = new Uri(uri);
        }

        /// <summary>
        /// Create a new SchemaDef.
        /// </summary>
        /// <param name="alias">Alias to link.</param>
        /// <param name="uri">Namespace URI.</param>
        public SchemaDef(string alias, Uri uri)
        {
            x = alias;
            y = uri;
        }

        /// <summary>
        /// Gets the defined alias.
        /// </summary>
        public string Alias { get { return x; } }
        /// <summary>
        /// Gets the defined namespace URI.
        /// </summary>
        public Uri Uri { get { return y; } }

        /// <summary>
        /// Returns the defined namespace URI.
        /// </summary>
        /// <returns>A string representing the defined namespace URI.</returns>
        public override string ToString()
        {
            return Uri.AbsoluteUri;
        }
    }
}
