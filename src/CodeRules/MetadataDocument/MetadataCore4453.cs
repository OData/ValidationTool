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
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4453 : ExtensionRule
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
                return "Metadata.Core.4453";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The last path segment MUST resolve to a navigation property in the context of the preceding path part, or to a term cast where the term MUST be of type Edm.EntityType, a concrete entity type or a collection of Edm.EntityType or concrete entity type.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "14.5.11";
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
        /// Verify Metadata.Core.4453
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

            string mergedMetadata = context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument;
            XmlDocument xmlDoc_MergedMetadata = new XmlDocument();
            xmlDoc_MergedMetadata.LoadXml(mergedMetadata);

            // Load MetadataDocument into XMLDOM
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            List<XmlNode> NvPropNodeList = new List<XmlNode>();

            XmlNodeList NodeList = xmlDoc.SelectNodes("//*[local-name()='NavigationPropertyPath']");
            XmlNodeList AttributeNodeList = xmlDoc.SelectNodes("//*[@NavigationPropertyPath]");

            foreach (XmlNode NvProp in AttributeNodeList)
            {
                NvPropNodeList.Add(NvProp);        
            }

            foreach (XmlNode NvProp in NodeList)
            {
                NvPropNodeList.Add(NvProp);
            }

            foreach (XmlNode NvPropPath in NvPropNodeList)
            {
                string path;
                if(NvPropPath.Attributes["NavigationPropertyPath"]!=null)
                {
                    path = NvPropPath.Attributes["NavigationPropertyPath"].Value;
                }
                else
                {
                    path = NvPropPath.InnerText;
                }
                 
                XmlNode parentOrTarget = NvPropPath;

                while (!parentOrTarget.LocalName.Equals("Annotation") && parentOrTarget.ParentNode!=null)
                {
                    parentOrTarget = parentOrTarget.ParentNode;
                }

                parentOrTarget = parentOrTarget.ParentNode;

                if(parentOrTarget.LocalName.Equals("Annotations"))
                {
                    string parentTargetPath = parentOrTarget.Attributes["Target"].Value;
                    parentOrTarget = parentOrTarget.ParentNode;
                    if (MetadataHelper.Path(parentTargetPath, xmlDoc_MergedMetadata, context, ref parentOrTarget))
                    {
                        // For annotations targeting a property of an entity type or complex type, 
                        // the path expression is evaluated starting at the outermost entity type or complex type 
                        // named in the Target of the enclosing edm:Annotations element.
                        if (parentOrTarget.LocalName.Equals("Property")||
                            parentOrTarget.LocalName.Equals("NavigationProperty"))
                        {
                            while(!(parentOrTarget.LocalName.Equals("EntityType")||
                                parentOrTarget.LocalName.Equals("ComplexType")))
                            {
                                parentOrTarget = parentOrTarget.ParentNode;
                            }
                        }

                        XmlNode resolveNode = parentOrTarget;

                        if (MetadataHelper.Path(path, xmlDoc_MergedMetadata, context, ref resolveNode))
                        {
                            if(resolveNode.LocalName.Equals("NavigationProperty"))
                            {
                                passed = true; 
                            }
                            else if(resolveNode.LocalName.Equals("Term")) 
                            {
                                if (resolveNode.Attributes["Type"].Value.RemoveCollectionFlag().Equals("Edm.EntityType"))
                                {
                                    passed = true;
                                }
                                else 
                                {
                                    string type = resolveNode.Attributes["Type"].Value.RemoveCollectionFlag();
                                    XElement termTypeEle;

                                    termTypeEle = MetadataHelper.GetTypeDefinitionEleInScope("EntityType", type, context);
                                    if (termTypeEle != null)
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                        passed = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    XmlNode resolveNode = parentOrTarget;
                    if (MetadataHelper.Path(path, xmlDoc_MergedMetadata, context, ref resolveNode))
                    {
                        if (resolveNode.LocalName.Equals("NavigationProperty"))
                        {
                            passed = true;
                        }
                        else if (resolveNode.LocalName.Equals("Term"))
                        {
                            if (resolveNode.Attributes["Type"].Value.RemoveCollectionFlag().Equals("Edm.EntityType"))
                            {
                                passed = true;
                            }
                            else
                            {
                                string type = resolveNode.Attributes["Type"].Value.RemoveCollectionFlag();
                                XElement termTypeEle;

                                termTypeEle = MetadataHelper.GetTypeDefinitionEleInScope("EntityType", type, context);
                                if (termTypeEle != null)
                                {
                                    passed = true;
                                }
                                else
                                {
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    passed = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return passed;
        }
    }
}
