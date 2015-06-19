// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class LinkCore2000 : ExtensionRule
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
                return "Link.Core.2000";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "entityNavProperty-es: This rule is the same as entityNavProperty, but with the added constraint that the NavigationProperty MUST point to an endpoint of an association with a cardinality of 'many' (for example, such that traversing the association yields a set).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.4";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "14.5.12";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odatacsdl";
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
        /// Gets the flag whether it requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to offline context or not
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to projected query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Link;
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
        /// Verify extension rule
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

            if (context.DestinationBasePath.Contains("$links"))
            {
                // Load Payload into XMLDOM
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(context.ResponsePayload);
                XmlNode linkNode = xmlDoc.SelectSingleNode("//*[local-name()='links']");

                if (linkNode != null)
                {
                    string entitySetName = context.DestinationBasePath.Substring(context.ServiceBaseUri.OriginalString.Length, context.DestinationBasePath.Length - context.ServiceBaseUri.OriginalString.Length).Split('/').First().Split('(').First();
                    string navPropName = context.DestinationBaseLastSegment;

                    // Load MetadataDocument into XMLDOM
                    XmlDocument mdDoc = new XmlDocument();
                    mdDoc.LoadXml(context.MetadataDocument);

                    XmlNode entitySetNode = mdDoc.SelectSingleNode(string.Format("//*[local-name()='EntitySet' and @Name='{0}']", entitySetName));
                    XmlNode navPropNode = mdDoc.SelectSingleNode(string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty' and @Name = '{1}']", entitySetNode.Attributes["EntityType"].Value.Split('.').Last(), navPropName));
                    XmlNode assoicationNode = mdDoc.SelectSingleNode(string.Format("//*[local-name()='Association' and @Name='{0}']", navPropNode.Attributes["Relationship"].Value.Split('.').Last()));

                    foreach (XmlNode endNode in assoicationNode.ChildNodes)
                    {
                        if (endNode.Attributes["Role"] != null)
                        {
                            if (endNode.Attributes["Role"].Value.Equals(navPropNode.Attributes["ToRole"].Value, StringComparison.InvariantCulture))
                            {
                                if (endNode.Attributes["Multiplicity"].Value.Equals("*", StringComparison.Ordinal))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            if (passed.HasValue && !passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}