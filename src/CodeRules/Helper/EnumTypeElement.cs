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
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The EnumTypeElement class.
    /// </summary>
    public class EnumTypeElement
    {
        /// <summary>
        /// The constructor of the EnumTypeElement class.
        /// </summary>
        /// <param name="name">The Name attribute.</param>
        /// <param name="underlingType">The UnderlingType attribute.</param>
        /// <param name="isFlags">The IsFlags attribute.</param>
        /// <param name="members">The members of the enumeration type.</param>
        public EnumTypeElement(string name, string underlingType, bool isFlags, List<KeyValuePair<string, int>> members)
        {
            this.name = name;
            this.underlyingType = underlingType;
            this.isFlags = isFlags;
            this.members = members;
        }

        /// <summary>
        /// Gets the value of the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the value of the underlingType.
        /// </summary>
        public string UnderlingType
        {
            get
            {
                return this.underlyingType;
            }
        }

        /// <summary>
        /// Gets the value of the isFlags.
        /// </summary>
        public bool IsFlags
        {
            get
            {
                return this.isFlags;
            }
        }

        /// <summary>
        /// Gets the members of the enumeration type.
        /// </summary>
        public List<KeyValuePair<string, int>> Members
        {
            get
            {
                return this.members;
            }
        }

        /// <summary>
        /// Parse a XElement type parameter to a EnumTypeElement instance.
        /// </summary>
        /// <param name="enumTypeElement">A EnumType element.</param>
        /// <returns>Returns a EnumTypeElement instance.</returns>
        public static EnumTypeElement Parse(XElement enumTypeElement)
        {
            if (null == enumTypeElement)
            {
                return null;
            }

            string name = null != enumTypeElement.Attribute("Name") ? enumTypeElement.Attribute("Name").Value : null;
            string underlyingType = null != enumTypeElement.Attribute("UnderlyingType") ? enumTypeElement.Attribute("UnderlyingType").Value : null;
            bool isFlags = null != enumTypeElement.Attribute("IsFlags") ? Convert.ToBoolean(enumTypeElement.Attribute("IsFlags").Value) : false;
            List<KeyValuePair<string, int>> members = new List<KeyValuePair<string, int>>();
            string xPath = "./*[local-name()='Member']";
            var memberElements = enumTypeElement.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            if (isFlags)
            {
                foreach (var m in memberElements)
                {
                    if (null == m.Attribute("Name") || null == m.Attribute("Value"))
                    {
                        return new EnumTypeElement(name, underlyingType, isFlags, new List<KeyValuePair<string, int>>());
                    }

                    int val = Convert.ToInt32(m.Attribute("Value").Value);

                    if (val < 0)
                    {
                        throw new FormatException(string.Format("The '{0}' element has some bad format children.", name));
                    }

                    members.Add(new KeyValuePair<string, int>(m.Attribute("Name").Value, val));
                }
            }
            else
            {
                int loop = 0;
                int counter = 0;

                foreach (var m in memberElements)
                {
                    if (null == m.Attribute("Name"))
                    {
                        return new EnumTypeElement(name, underlyingType, isFlags, new List<KeyValuePair<string, int>>());
                    }

                    if (null == m.Attribute("Value") && counter == loop)
                    {
                        members.Add(new KeyValuePair<string, int>(m.Attribute("Name").Value, loop++));

                    }
                    else if (null != m.Attribute("Value") && 0 == counter && 0 == loop)
                    {
                        members.Add(new KeyValuePair<string, int>(m.Attribute("Name").Value, Convert.ToInt32(m.Attribute("Value").Value)));
                    }
                    else
                    {
                        throw new FormatException(string.Format("The '{0}' element has some bad format children.", name));
                    }

                    counter++;
                }
            }

            return new EnumTypeElement(name, underlyingType, isFlags, members);
        }

        /// <summary>
        /// The Name attribute of the enumeration type.
        /// </summary>
        private string name;

        /// <summary>
        /// The UnderlyingType attribute of the enumeration type.
        /// </summary>
        private string underlyingType;

        /// <summary>
        /// The IsFlags attribute of the enumeration type.
        /// </summary>
        private bool isFlags;

        /// <summary>
        /// The members of the enumeration type.
        /// </summary>
        private List<KeyValuePair<string, int>> members;
    }
}
