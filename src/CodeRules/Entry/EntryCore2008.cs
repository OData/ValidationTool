// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Entry.Core.2008
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2008 : ExtensionRule
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
                return "Entry.Core.2008";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the FC_KeepInContent attribute is not supplied in the mapping, the data service MUST function as if it were specified with a value of true.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.7.2.1";
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
        /// Gets the version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to proected response or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false; ;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verify Entry.Core.2008
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

            // Load MetadataDocument into XMLDOM
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            // Check to see if the type has base type
            XmlNode entityTypeNode = xmlDoc.SelectSingleNode("//*[local-name()='EntityType' and @Name = '" + context.EntityTypeShortName + "']");
            bool hasBaseType = false;
            string baseTypeName = string.Empty;

            if (entityTypeNode.Attributes["BaseType"] != null)
            {
                baseTypeName = entityTypeNode.Attributes["BaseType"].Value.Split('.').Last();
                hasBaseType = true;
            }

            XmlNodeList baseTypePropertyNodeList = null;
            int expectedCount = 0;

            if (hasBaseType)
            {
                baseTypePropertyNodeList = xmlDoc.SelectNodes("//*[local-name()='EntityType' and @Name = '" + baseTypeName + "']/*[local-name()='Property']");

                foreach (XmlNode node in baseTypePropertyNodeList)
                {
                    if (node.Attributes["m:FC_KeepInContent"] != null)
                    {
                        if (node.Attributes["m:FC_KeepInContent"].Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            expectedCount++;
                        }
                    }
                    else
                    {
                        expectedCount++;
                    }
                }
            }

            XmlNodeList propertyNodeList = xmlDoc.SelectNodes("//*[local-name()='EntityType' and @Name = '" + context.EntityTypeShortName + "']/*[local-name()='Property']");          
            
            foreach (XmlNode node in propertyNodeList)
            {
                if (node.Attributes["m:FC_KeepInContent"] != null)
                {
                    if (node.Attributes["m:FC_KeepInContent"].Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        expectedCount++;
                    }
                }
                else
                {
                    expectedCount++;
                }
            }

            // Get the actual count
            XElement entry;
            context.ResponsePayload.TryToXElement(out entry);
            var properties = entry.XPathSelectElements("//m:properties/*", ODataNamespaceManager.Instance);

            // to exclude those expended from linked properties
            var propertiesEcpanded = entry.XPathSelectElements("//atom:link//m:properties/*", ODataNamespaceManager.Instance);

            int actualCount = properties.Count() - propertiesEcpanded.Count();

            if (actualCount == expectedCount)
            {
                passed = true;
            }
            else
            {
                passed = false;
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}

