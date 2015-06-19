// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The Property class.
    /// </summary>
    public struct ODataProperty
    {
        /// <summary>
        /// The constructor of the Property class.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="propertyValueType">The type of a property value.</param>
        public ODataProperty(string propertyName, object propertyValue, string propertyValueType)
        {
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
            this.propertyValueType = propertyValueType;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        public object PropertyValue
        {
            get
            {
                return this.propertyValue;
            }
        }

        /// <summary>
        /// Gets the type of a property value.
        /// </summary>
        public string PropertyValueType
        {
            get
            {
                return this.propertyValueType;
            }
        }

        /// <summary>
        /// Parse a XElement type parameter to a Property type instance.
        /// </summary>
        /// <param name="propertyValueElement">A PropertyValue element in metadata document.</param>
        /// <param name="complexTypeElement">A related ComplexType element in metadata document.</param>
        /// <returns>Return a Property type instance.</returns>
        public static ODataProperty Parse(XElement propertyValueElement, ComplexTypeElement complexTypeElement)
        {
            if (null == propertyValueElement || null == complexTypeElement)
            {
                throw new ArgumentNullException("The value of parameter 'propertyValueElement' or 'complexTypeElement' cannot be null.");
            }

            // Set the property name.
            string propName = null != propertyValueElement.Attribute("Property") ?
                propertyValueElement.Attribute("Property").Value :
                null;

            // Set the property type.
            string propType = null;

            foreach (var prop in complexTypeElement.Properties)
            {
                if (prop.Name == propName)
                {
                    propType = prop.Type;
                    break;
                }
            }

            if (null == propType)
            {
                throw new NullReferenceException("The property type does not contain in the input parameter 'complexTypeElement'.");
            }

            // Set the property value.
            object propVal = null;

            if (propType.StartsWith("Collection("))
            {
                string subType = propType.RemoveCollectionFlag().RemoveEdmDotPrefix();

                if (string.Empty != propertyValueElement.Value)
                {
                    string xPath = string.Format("./*[local-name()='Collection']/*[local-name()='{0}']", subType);
                    var elements = propertyValueElement.XPathSelectElements(xPath, ODataNamespaceManager.Instance).ToList();
                    List<string> vals = new List<string>();
                    elements.ForEach(e =>
                    {
                        vals.Add(e.Value);
                    });
                    propVal = vals;
                }
            }
            else
            {
                propVal = ODataProperty.GetPropertyValue(propertyValueElement, propType);
            }

            return new ODataProperty(propName, propVal, propType);
        }

        /// <summary>
        /// The property name.
        /// </summary>
        private string propertyName;

        /// <summary>
        /// The property value.
        /// </summary>
        private object propertyValue;

        /// <summary>
        /// The type of property value.
        /// </summary>
        private string propertyValueType;

        /// <summary>
        /// Get property value.
        /// </summary>
        /// <param name="propertyValueElement">The property value element.</param>
        /// <param name="propertyType">The property type.</param>
        /// <returns>Returns the property value.</returns>
        private static string GetPropertyValue(XElement propertyValueElement, string propertyType)
        {
            string propVal = string.Empty;

            switch (propertyType)
            {
                case "Edm.Binary":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ? 
                        propertyValueElement.GetAttributeValue("Binary") : 
                        propertyValueElement.Value;
                    break;
                case "Edm.Boolean":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Bool") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Date":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Date") :
                        propertyValueElement.Value;
                    break;
                case "Edm.DateTimeOffset":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("DateTimeOffset") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Decimal":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Decimal") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Duration":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Duration") :
                        propertyValueElement.Value;
                    break;
                case "Edm.EnumMember":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("EnumMember") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Float":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Float") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Guid":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Guid") :
                        propertyValueElement.Value;
                    break;
                case "Edm.Int":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("Int") :
                        propertyValueElement.Value;
                    break;
                case "Edm.String":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("String") :
                        propertyValueElement.Value;
                    break;
                case "Edm.TimeOfDay":
                    propVal = string.IsNullOrEmpty(propertyValueElement.Value) ?
                        propertyValueElement.GetAttributeValue("TimeOfDay") :
                        propertyValueElement.Value;
                    break;
            }

            return propVal;
        }
    }
}
