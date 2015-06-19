// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of code rule applying to Individual Property payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4628_IndividualProperty : CommonCore4628
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
    /// Class of code rule applying to Entity Reference payload.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4628_EntityRef : CommonCore4628
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
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4628_Feed : CommonCore4628
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
    public class CommonCore4628_Entry : CommonCore4628
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
    /// Class of entension rule for Common.Core.4628
    /// </summary>
    public abstract class CommonCore4628 : ExtensionRule
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
                return "Common.Core.4628";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For structural-valued annotations, the annotation element MUST contain the metadata:type attribute of metadata:annotation element.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.2.3";
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
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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
        /// Verify Common.Core.4628
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
            // Url:http://services.odata.org/V4/OData/OData.svc/Products(0)/Description?$format=xml or http://services.odata.org/V4/OData/OData.svc/Persons(0)?$format=atom
            // xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><m:value m:context=\"http://services.odata.org/V4/OData/OData.svc/$metadata#Products(0)/Description\" xmlns:atom=\"http://www.w3.org/2005/Atom\"  xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\">Whole grain bread <m:annotation atom:term=\"com.contoso.address\" m:type=\"#ODataDemo.Address\"><d:Street>2817 Milton Dr.</d:Street><d:City>Albuquerque</d:City><d:State>NM</d:State><d:ZipCode>87110</d:ZipCode><d:Country>USA</d:Country></m:annotation></m:value>");
            xmlDoc.LoadXml(context.ResponsePayload);
            
            XmlNodeList annotationElements = xmlDoc.SelectNodes(@"//*[local-name()='annotation']", ODataNamespaceManager.Instance);
            passed = true;

            foreach (XmlNode annotatEle in annotationElements)
            {
                bool isStructuredValue = false;

                if (annotatEle.ChildNodes != null && annotatEle.ChildNodes.Count > 1)
                {
                    isStructuredValue = true;

                    foreach (XmlNode ele in annotatEle.ChildNodes)
                    {
                        if (!ele.NamespaceURI.Equals(Constants.V4NSData))
                        {
                            isStructuredValue = false;
                            break;
                        }
                    }
                }

                if ( !isStructuredValue )
                    continue;

                bool hasType = annotatEle.Attributes["type", Constants.NSMetadata] != null;

                if (hasType == false)
                {
                    passed = false;
                    break;
                }
            }

            info = new ExtensionRuleViolationInfo(passed == true ? null : this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
