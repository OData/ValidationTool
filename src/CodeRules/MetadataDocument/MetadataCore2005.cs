// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Xml;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.2005
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore2005 : ExtensionRule
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
                return "Metadata.Core.2005";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The presence of HasStream attribute with a value of \"true\" on an <EntityType> element states that the Entity Type is associated with a Media Resource.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.7.2";
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
        /// Verify Metadata.Core.2005
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

            // Find the namespace(s) in order to query for EntitySet name
            XmlNodeList namespaceList = xmlDoc.SelectNodes("//*[@*[name()='Namespace']]");

            // Find all properties marked m:HasStream = "true" in the document             
            XmlNodeList hasStreamList = xmlDoc.SelectNodes("//*[@*[name()='m:HasStream'] = 'true']");

            if (hasStreamList.Count != 0)
            {
                // Find the EntitySet node
                XmlNode entitySetNode = null;
                foreach (XmlNode hsNode in hasStreamList)
                {
                    bool foundEntitySet = false;
                    foreach (XmlNode nsNode in namespaceList)
                    {
                        if (!foundEntitySet)
                        {
                            entitySetNode = xmlDoc.SelectSingleNode("//*[@*[name()='EntityType'] = '" + nsNode.Attributes["Namespace"].Value + "." + hsNode.Attributes["Name"].Value + "']");

                            if (entitySetNode != null)
                            {
                                foundEntitySet = true;
                            }
                        }
                    }

                    // Query to find the first entity in the EntitySet
                    Uri absoluteUri = new Uri(context.Destination.OriginalString);
                    Uri relativeUri = new Uri(entitySetNode.Attributes["Name"].Value + "?$top=1", UriKind.Relative);
                    Uri combinedUri = new Uri(absoluteUri, relativeUri);
                    Response response = WebHelper.Get(combinedUri, Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            // Find the Uri that has media resource and query for it
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(response.ResponsePayload);
                            XmlNode node = xmlDoc2.SelectSingleNode("//*[local-name()='entry']/*[local-name()='id']");
                            response = WebHelper.Get(new Uri(node.InnerText + "/$value"), Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                passed = true;
                            }
                            else
                            {
                                passed = false;
                            }
                        }
                        catch (Exception)
                        {
                            passed = false;
                        }
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}

