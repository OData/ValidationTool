// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4037 : ExtensionRule
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
                return "Metadata.Core.4037";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A model element MUST NOT specify more than one annotation for a given combination of Term and Qualifier attributes.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "4.6";
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
        /// Verify Metadata.Core.4037
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

            List<string> modelElements = new List<string>() 
            {
                "Action", "ActionImport", "Annotation", "Apply", "Cast", "ComplexType",
                "EntityContainer", "EntitySet", "EntityType",
                "EnumType", "Function", "FunctionImport", "If", "IsOf", 
                "LabeledElement", "Member", "NavigationProperty", "Null", 
                "OnDelete", "Parameter", "Property", "PropertyValue", "Record",
                "ReferentialConstraint", "ReturnType", "Schema", "Singleton", "Term",
                "TypeDefinition", "UrlRef", "Reference", "And", "Or", "Not", "Eq",
                "Ne", "Gt", "Ge", "Lt", "Le", "Annoations"
            };

            foreach (string modelElement in modelElements)
            {
                string xPath = string.Format("//*[local-name()='{0}']", modelElement);
                XmlNodeList nodeList = xmlDoc.SelectNodes(xPath);
                foreach(XmlNode node in nodeList)
                {
                    if(node.HasChildNodes)
                    {
                        // Check if there's any duplicate Annoation Term and Qualifier combination for the target model element.
                        bool duplicate = false;
                        HashSet<string> annotaionSet = new HashSet<string>(StringComparer.Ordinal);

                        foreach(XmlNode child in node.ChildNodes)
                        {
                            if(child.Name.Equals("Annotation"))
                            {
                                passed = true;
                                string termQualifier = string.Empty;
                                if (child.Attributes["Term"] != null)
                                {
                                    termQualifier = child.Attributes["Term"].Value;
                                    if (child.Attributes["Qualifier"] != null)
                                    {
                                        termQualifier += child.Attributes["Qualifier"].Value;
                                    }

                                    if (!(annotaionSet.Add(termQualifier)))
                                    {
                                        duplicate = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if(duplicate)
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }

                if(passed == false)
                {
                    break;
                }
            }

            return passed;
        }
    }
}
