// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4163
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4163 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Metadata.Core.4163";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"An entity type MUST NOT introduce an inheritance cycle via the base type attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "8.1.2";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Metadata;
            }
        }

        /// <summary>
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Gets the OData version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4163
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            XmlDocument metadata = new XmlDocument();
            metadata.LoadXml(context.MetadataDocument);
            string xpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and @Namespace]/*[local-name()='EntityType' and @BaseType and @Name]";
            XmlNodeList entityTypes = metadata.SelectNodes(xpath);

            // Get all the type names in current schema that need to check.
            List<string> typesToCheck=new List<string>();
            foreach (XmlNode type in entityTypes)
            {
                typesToCheck.Add(type.ParentNode.Attributes["Namespace"].Value + "." + type.Attributes["Name"].Value);
            }

            // Get the complete CSDL.
            if (context.ContainsExternalSchema)
            {
                metadata.LoadXml(context.MergedMetadataDocument);
            }
            
            // Get Alias Namespace map.
            Dictionary<string, string> aliasNamespaceMap = new Dictionary<string, string>();
            XmlNodeList schemasWithAlias = metadata.SelectNodes(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and @Namespace and @Alias]");
            foreach (XmlNode schema in schemasWithAlias)
            {
                aliasNamespaceMap.Add(schema.Attributes["Alias"].Value, schema.Attributes["Namespace"].Value);
            }

            // Create a list that contains the type -> basetype relationship. And convert all the alias to full namespace.
            List<Item> typeInheritTable = new List<Item>();
            entityTypes = metadata.SelectNodes(xpath);
            foreach (XmlNode type in entityTypes)
            {
                string baseTypeNs = string.Empty;
                string baseTypeName = type.Attributes["BaseType"].Value;
                int dotIndex = baseTypeName.LastIndexOf('.');
                if (dotIndex != -1 && dotIndex != 0 && dotIndex != baseTypeName.Length - 1)
                {
                    baseTypeNs = baseTypeName.Substring(0, dotIndex);
                }
                else
                { continue; }

                if (aliasNamespaceMap.ContainsKey(baseTypeNs))
                {
                    baseTypeName = aliasNamespaceMap[baseTypeNs] + baseTypeName.Substring(dotIndex + 1);
                }

                typeInheritTable.Add(new Item(type.ParentNode.Attributes["Namespace"].Value + "." + type.Attributes["Name"].Value, baseTypeName, false));
            }

            // Check the cycle.
            foreach (string type in typesToCheck)
            {
                passed = true;
                if (ChecCycle(type, typeInheritTable))
                {
                    passed = false;
                    break;
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }

        /// <summary>
        /// Check whether a type introduce an inheritance cycle.
        /// </summary>
        /// <param name="type">The type name to check.</param>
        /// <param name="table">The inheritance table.</param>
        /// <returns></returns>
        private bool ChecCycle(string type, List<Item> table)
        {
            bool result = false;
            foreach (Item item in table)
            {
                if(type.Equals(item.type))
                {
                    if (item.accessed)
                    {
                        item.accessed = false; // Clear the flag for we have result.
                        result = true; 
                    }
                    else
                    {
                        item.accessed = true;
                        result = ChecCycle(item.baseType, table);
                        item.accessed = false; // Clear the flag for we have result.
                    }
                    break;
                }
            }

            // if a type inheritents another type that can't find out in the table, 
            // it must be a type that not inherits other types. We don't have to check it.
            return result;
        }
    }

    class Item
    {
        public Item(string type, string baseType, bool accessed)
        {
            this.type = type;
            this.baseType = baseType;
            this.accessed = accessed;
        }
       public string type;
       public string baseType;
       public bool accessed;
    }
}

