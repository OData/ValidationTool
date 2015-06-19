// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.2016
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore2016 : ExtensionRule
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
                return "Metadata.Core.2016";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A data service SHOULD support target paths that define elements with mixed content.";
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2_V3;
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
                return RequirementLevel.Should;
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
        /// Verify Metadata.Core.2016
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

            // Adding all AtomPub mappings to a list
            List<string> atomPubMapping = new List<string>(new string[]
            {
                "SyndicationAuthorName",
                "SyndicationAuthorEmail",
                "SyndicationAuthorUri",
                "SyndicationPublished",
                "SyndicationRights",
                "SyndicationTitle",
                "SyndicationUpdated",
                "SyndicationContributorName",
                "SyndicationContributorEmail",
                "SyndicationContributorUri",
                "SyndicationSource",
                "SyndicationSummary"
            });

            // Load MetadataDocument into XMLDOM
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            // Find the namespace(s) in order to query for EntitySet name
            XmlNodeList namespaceList = xmlDoc.SelectNodes("//*[@*[name()='Namespace']]");

            // Find all nodes
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes("//*[@*[name()='m:FC_TargetPath']]");

            if (xmlNodeList.Count != 0)
            {
                XmlNode entitySetNode = null;
                foreach (XmlNode node in xmlNodeList)
                {
                    // Find the EntitySet node
                    bool foundEntitySet = false;
                    foreach (XmlNode nsNode in namespaceList)
                    {
                        if (!foundEntitySet)
                        {
                            entitySetNode = xmlDoc.SelectSingleNode("//*[@*[name()='EntityType'] = '" + nsNode.Attributes["Namespace"].Value + "." + node.ParentNode.Attributes["Name"].Value + "']");

                            if (entitySetNode != null)
                            {
                                foundEntitySet = true;
                            }
                        }
                    }

                    List<string> pathList = new List<string>();
                    if (!atomPubMapping.Exists(item => item == node.Attributes["m:FC_TargetPath"].Value))
                    {
                        // Construct the XPath query
                        pathList = node.Attributes["m:FC_TargetPath"].Value.Split('/').ToList();
                        StringBuilder xpathQuery = new StringBuilder();
                        xpathQuery.Append("//*/*[local-name()='entry']");

                        foreach (string path in pathList)
                        {
                            xpathQuery.Append("/*[local-name()='");
                            xpathQuery.Append(path);
                            xpathQuery.Append("']");
                        }

                        // Query to find the first entity in the EntitySet
                        Uri absoluteUri = new Uri(context.Destination.OriginalString);
                        Uri relativeUri = new Uri(entitySetNode.Attributes["Name"].Value + "?$top=1", UriKind.Relative);
                        Uri combinedUri = new Uri(absoluteUri, relativeUri);
                        Response response = WebHelper.Get(combinedUri, Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(response.ResponsePayload);

                            if (xmlDoc2.SelectSingleNode(xpathQuery.ToString()) != null)
                            {
                                passed = true;
                            }
                            else
                            {
                                passed = false;
                                break;
                            }
                        }
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}

