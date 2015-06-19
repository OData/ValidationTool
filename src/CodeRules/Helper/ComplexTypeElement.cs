// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The ComplexTypeElement class.
    /// </summary>
    public class ComplexTypeElement
    {
        /// <summary>
        /// The constructor of ComplexTypeElement class.
        /// </summary>
        /// <param name="name">The complex type name.</param>
        /// <param name="properties">The properties of the complex type.</param>
        public ComplexTypeElement(string name, List<PropertyElement> properties)
        {
            this.Name = name;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets or sets the complex type name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the complex type properties.
        /// </summary>
        public List<PropertyElement> Properties
        {
            get;
            set;
        }

        /// <summary>
        /// Parse a XElement type parameter to a ComplexTypeElement instance.
        /// </summary>
        /// <param name="complexTypeElement">A ComplexType element.</param>
        /// <returns>Returns a ComplexTypeElement instance.</returns>
        public static ComplexTypeElement Parse(XElement complexTypeElement)
        {
            if (null == complexTypeElement)
            {
                return null;
            }

            string name = null != complexTypeElement.Attribute("Name") ? complexTypeElement.Attribute("Name").Value : null;
            var props = new List<PropertyElement>();

            string xPath = "./*[local-name()='Property']";
            var propertyElements = complexTypeElement.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            foreach (var propEle in propertyElements)
            {
                if (null != propEle.Attribute("Name") && null != propEle.Attribute("Type"))
                {
                    AnnotationDVsManager accessor = AnnotationDVsManager.Instance();
                    string propertyName = string.Format("{0}_{1}", name, propEle.GetAttributeValue("Name"));
                    object defaultValue = accessor.GetDefaultValue(propertyName);
                    bool nullable = null != propEle.Attribute("Nullable") ? Convert.ToBoolean(propEle.Attribute("Nullable").Value) : true;
                    props.Add(new PropertyElement(propEle.Attribute("Name").Value.ToString(), propEle.Attribute("Type").Value.ToString(), defaultValue, nullable));
                }
            }

            return new ComplexTypeElement(name, props);
        }
    }
}
