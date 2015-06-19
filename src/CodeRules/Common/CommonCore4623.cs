// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of code rule applying to Individual Property payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4623_IndividualProperty : CommonCore4623
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.IndividualProperty;
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
    }

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4623_Feed : CommonCore4623
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Feed;
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
    }

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4623_Entry : CommonCore4623
    {
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to Entity Reference payload.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4623_EntityRef : CommonCore4623
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.EntityRef;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4623
    /// </summary>
    public abstract class CommonCore4623 : ExtensionRule
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
                return "Common.Core.4623";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When specified in the content of an annotation element representing a primitive value, the content MUST be formatted as per Primitive Types in Atom.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.2.1";
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
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
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
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
        /// Verify Common.Core.4623
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

            // Single primitive individual property is not allowed to have instance annotation.
            if (VerificationHelper.IsIndividualPropertySinglePrimitiveType(context.ResponsePayload, context.PayloadType))
                return null;

            XmlDocument xmlDoc = new XmlDocument();

            // Test Data
            // Url:http://services.odata.org/V4/OData/OData.svc/Products(0)/Description?$format=xml
            // xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><m:value m:context=\"http://services.odata.org/V4/OData/OData.svc/$metadata#Products(0)/Description\" xmlns:atom=\"http://www.w3.org/2005/Atom\"  xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\">Whole grain bread <m:annotation atom:term=\"ODataDemo.Product.Display\" m:target=\"Description\" m:type=\"Boolean\">true</m:annotation></m:value>");
            xmlDoc.LoadXml(context.ResponsePayload);

            XmlNodeList annotationElements = xmlDoc.SelectNodes(@"//*[local-name()='annotation']", ODataNamespaceManager.Instance);

            foreach (XmlNode annotatEle in annotationElements)
            {
                if (annotatEle.Attributes["null", Constants.NSMetadata] != null &&
                            annotatEle.Attributes["null", Constants.NSMetadata].Value.Equals("true"))
                {
                    return null;
                }

                string content = annotatEle.InnerText;

                if (annotatEle.Attributes["type", Constants.NSMetadata] != null)
                {
                    string typeName = annotatEle.Attributes["type", Constants.NSMetadata].Value;

                    if (!typeName.Contains("."))
                    {
                        typeName = "Edm." + typeName;
                    }

                    if (EdmTypeManager.IsEdmSimpleType(typeName))
                    {
                        IEdmType edmType = EdmTypeManager.GetEdmType(typeName);
                        if (edmType.IsGoodWith(content))
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }
            }
           
            return passed;
        }
    }
}
