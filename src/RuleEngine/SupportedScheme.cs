// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Singleton pattern Class of SupportedScheme.
    /// </summary>
    public sealed class SupportedScheme
    {
        /// <summary>
        /// The built-in supported URI HTTP scheme
        /// </summary>
        internal const string SchemeHttp = "http";

        /// <summary>
        /// The built-in supported URI HTTPS scheme
        /// </summary>
        internal const string SchemeHttps = "https";

        /// <summary>
        /// The singleton instance of SupportedScheme
        /// </summary>
        private static SupportedScheme instance = new SupportedScheme();

        /// <summary>
        /// Supported URI scheme collection.
        /// </summary>
        private HashSet<string> SupportedSchemes = new HashSet<string>(EqualityComparer<string>.Default) { SchemeHttp, SchemeHttps };

        /// <summary>
        /// Creates an instance of SupportedScheme.
        /// Marked as private to prevent any other instantiations other than the singleton instance.
        /// </summary>
        private SupportedScheme()
        {
        }

        /// <summary>
        /// Gets the singleton instance of SupportedScheme.
        /// </summary>
        public static SupportedScheme Instance
        {
            get { return SupportedScheme.instance; }
        }

        /// <summary>
        /// Adds a supported uri scheme. Don't add http since it is the default one.
        /// </summary>
        /// <param name="scheme">Uri scheme other than http to be added</param>
        /// <returns>true if the scheme has been added; false otherwise</returns>
        public bool Register(string scheme)
        {
            if (!string.IsNullOrEmpty(scheme) && !this.SupportedSchemes.Contains(scheme))
            {
                this.SupportedSchemes.Add(scheme);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an URI scheme from the supported collection
        /// </summary>
        /// <param name="scheme">Uri scheme to be removed from the supported collection</param>
        /// <returns>true if the scheme has been removed; false otherwise</returns>
        public bool Unregister(string scheme)
        {
            if (!string.IsNullOrEmpty(scheme) && this.SupportedSchemes.Contains(scheme))
            {
                this.SupportedSchemes.Remove(scheme);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a scheme is supported
        /// </summary>
        /// <param name="scheme">Scheme name</param>
        /// <returns>True if the scheme is a supported one; false otherwise</returns>
        public bool Contains(string scheme)
        {
            return this.SupportedSchemes.Contains(scheme);
        }
    }
}
