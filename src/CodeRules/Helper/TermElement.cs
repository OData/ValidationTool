// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using RuleEngine.Common;
    #endregion

    /// <summary>
    /// The Term element class.
    /// </summary>
    public class TermElement
    {
        /// <summary>
        /// Initialize the Term class.
        /// </summary>
        /// <param name="aliasNamespace">The alias and namespace pair.</param>
        /// <param name="name">The name of the term.</param>
        /// <param name="type">The type of the term.</param>
        /// <param name="appliesTo">The element is applied by the term.</param>
        /// <param name="defaultValue">The default value of the term.</param>
        public TermElement(AliasNamespacePair aliasNamespace, string name, string type, string[] appliesTo, object defaultValue)
        {
            this.Name = name;
            this.Alias = aliasNamespace.Alias;
            this.Namespace = aliasNamespace.Namespace;
            this.Type = type;
            this.AppliesTo = appliesTo;
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets or sets the name attribute of a term element.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alias of a term element.
        /// </summary>
        public string Alias
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the namespace of a term element.
        /// </summary>
        public string Namespace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type attribute of a term element.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the applies-to attribute of a term element.
        /// </summary>
        public string[] AppliesTo
        {
            get;
            set;
        }

        #region Term facet properties.
        /// <summary>
        /// Gets or sets the facet property 'default-value' of a term element.
        /// </summary>
        public object DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// Gets or sets the facet property 'nullable' of a term element.
        /// </summary>
        public bool Nullable
        {
            get { return nullable; }
            set { nullable = value; }
        }

        /// <summary>
        /// Gets or sets the facet property 'max-length' of a term element.
        /// </summary>
        public int MaxLength
        {
            get { return maxLength; }
            set { maxLength = value; }
        }

        /// <summary>
        /// Gets or sets the facet property 'precision' of a term element.
        /// </summary>
        public uint Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        /// <summary>
        /// Gets or sets the facet property 'scale' of a term element. 
        /// </summary>
        public uint Scale
        {
            get { return scale; }
            set
            {
                scale = value <= this.precision ? value : this.precision;
            }
        }

        /// <summary>
        /// Gets or sets the facet property 'unicode' of a term element.
        /// </summary>
        public bool Unicode
        {
            get { return unicode; }
            set { unicode = value; }
        }

        /// <summary>
        /// Gets or sets the facet property 'SRID' of a term element. 
        /// </summary>
        public uint SRID
        {
            get { return srid; }
            set { srid = value; }
        }

        /// <summary>
        /// Parse a XElement type parameter to a TermElement type instance.
        /// </summary>
        /// <param name="termElement">The term element in metadata document.</param>
        /// <param name="getAliasAndNamespace">The delegate method which is used to get the alias and namespace of the term.</param>
        /// <returns>Return a TermElement type instance.</returns>
        public static TermElement Parse(XElement termElement, Func<XElement, AliasNamespacePair> getAliasAndNamespace)
        {
            var aliasNamespace = getAliasAndNamespace(termElement);
            string name = null != termElement.Attribute("Name") ? termElement.Attribute("Name").Value : null;
            string alias = aliasNamespace.Alias;
            string nspace = aliasNamespace.Namespace;
            string type = null != termElement.Attribute("Type") ? termElement.Attribute("Type").Value : null;
            string[] appliesTo = null != termElement.Attribute("AppliesTo") ? termElement.Attribute("AppliesTo").Value.Split(' ') : null;
            AnnotationDVsManager accessor = AnnotationDVsManager.Instance();
            object defaultValue = accessor.GetDefaultValue(name);

            return new TermElement(aliasNamespace, name, type, appliesTo, defaultValue);
        }

        #region Private members.
        // All facet properties.
        private object defaultValue = null;
        private bool nullable = true;
        private int maxLength;
        private uint precision;
        // Type: uint/variable
        private uint scale = 0;
        private bool unicode = true;
        // Type: uint/variable
        private uint srid = 0;
        #endregion
        #endregion
    }
}
