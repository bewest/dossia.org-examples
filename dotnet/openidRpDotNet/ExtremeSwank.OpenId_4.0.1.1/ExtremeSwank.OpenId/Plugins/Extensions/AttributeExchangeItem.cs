using System;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Intermediate object used while performing Fetch or Store operations.
    /// Represents one data item with one or more values.
    /// </summary>
    internal class AttributeExchangeItem
    {
        private Uri _IdUri;
        private string _Alias;
        private int _Count;
        private string[] _Values;
        private bool _Required;

        /// <summary>
        /// Gets or sets the Identifier URI for this item.
        /// </summary>
        public Uri IdUri
        {
            get { return _IdUri; }
            set { _IdUri = value; }
        }
        /// <summary>
        /// Gets or sets short alias which references the supplied Identifier URI for this item.
        /// </summary>
        public string Alias
        {
            get { return _Alias; }
            set { _Alias = value; }
        }
        /// <summary>
        /// Gets or sets the number of values in this item.
        /// </summary>
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        /// <summary>
        /// Gets or sets the values for this item.  Used only during Store requests.
        /// </summary>
        public string[] Values
        {
            get { return _Values; }
            set { _Values = value; }
        }
        /// <summary>
        /// Gets or sets whether or not a response for this item is required.  Used only during Fetch requests.
        /// </summary>
        public bool Required
        {
            get { return _Required; }
            set { _Required = value; }
        }
    }
}
