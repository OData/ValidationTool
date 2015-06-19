// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Defines a alias/namespace pair that can be set or retrieved.
    /// </summary>
    public struct AliasNamespacePair
    {
        /// <summary>
        /// Initializes a new instance of the AliasNamespacePair structure with the specified _alias and _namespace.
        /// </summary>
        /// <param name="alias">The alias used in metadata.</param>
        /// <param name="_namespace">The namespace used in metadata.</param>
        public AliasNamespacePair(string alias, string _namespace)
        {
            this.alias = alias;
            this.nspace = _namespace;
        }

        /// <summary>
        /// Gets the value of private member '_alias'.
        /// </summary>
        public string Alias
        {
            get { return alias; }
        }

        /// <summary>
        /// Gets the value of private member '_namespace'.
        /// </summary>
        public string Namespace
        {
            get { return nspace; }
        }

        /// <summary>
        /// A string representation of the AliasNamespace structure, 
        /// using the string representations of the alias and namespace.
        /// </summary>
        /// <returns>
        /// Returns a string representation of the AliasNamespace structure, 
        /// using the string representations of the alias and namespace.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", this.alias, this.nspace);
        }

        /// <summary>
        /// The alias used in metadata.
        /// </summary>
        private string alias;

        /// <summary>
        /// The namespace used in metadata.
        /// </summary>
        private string nspace;
    }
}
