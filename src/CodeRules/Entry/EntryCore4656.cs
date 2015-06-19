// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of entension rule for Entry.Core.4656
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4656 : ExtensionRule
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
                return "Entry.Core.4656";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If at most one entity can be related in an expanded navigation property, the value of the metadata:inline element is the representation of the related entity.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "8.3";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
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
        /// Verify Entry.Core.4656
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out paramater to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            if (string.Empty != context.Destination.Query)
            {
                List<string> expandQueryVals = ODataUriAnalyzer.GetQueryOptionValsFromUrl(context.Destination.ToString(), @"expand");
                List<string> collectionTypeNavigPropNames = MetadataHelper.GetAppropriateNavigationPropertyNames(context, NavigationPropertyType.Entity);

                string xpath = @"/atom:entry/atom:link[@type='application/atom+xml;type=feed' or @type='application/atom+xml;type=entry']";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(context.ResponsePayload);
                XmlNodeList linkElements = xmlDoc.SelectNodes(xpath, ODataNamespaceManager.Instance);

                if (linkElements.Count > 0)
                {
                    string metadataNp = ODataVersion.V4 == context.Version ?
                        linkElements[0].ParentNode.GetPrefixOfNamespace(Constants.NSMetadata) :
                        linkElements[0].ParentNode.GetPrefixOfNamespace(Constants.V3NSMetadata);

                    foreach (XmlNode le in linkElements)
                    {
                        if (null == le.Attributes["title"])
                        {
                            continue;
                        }

                        if (collectionTypeNavigPropNames.Contains(le.Attributes["title"].Value) && 
                            expandQueryVals.Contains(le.Attributes["title"].Value))
                        {
                            if (le.FirstChild == le.LastChild && metadataNp + @":inline" == le.FirstChild.Name)
                            {
                                XmlNode inlineNode = le.FirstChild;

                                if (inlineNode.HasChildNodes)
                                {
                                    if (inlineNode.FirstChild == inlineNode.LastChild && @"entry" == inlineNode.FirstChild.Name)
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
                            else
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                break;
                            }
                        }
                    }
                }
            }

            return passed;
        }
    }
}
