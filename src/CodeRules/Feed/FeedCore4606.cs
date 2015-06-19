// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces

    using System;
    using System.Xml;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Net;

    #endregion

    /// <summary>
    /// Class of extension rule for Feed.Core.4606
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore4606 : ExtensionRule
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
                return "Feed.Core.4606";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If server-side paging has been applied, the feed MUST include a next results link.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "12.3";
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

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
          //  xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><feed xml:base=\"http://services.odata.org/V4/OData/OData.svc/\" xmlns=\"http://www.w3.org/2005/Atom\" xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\" m:context=\"http://services.odata.org/V4/OData/OData.svc/$metadata#Products\" m:metadata-etag=\"test\"><id>http://services.odata.org/V4/OData/OData.svc/Products/</id><title type=\"text\">Products</title><updated>2014-09-10T08:23:46Z</updated><link rel=\"self\" title=\"Products\" href=\"Products\" /><entry><id>http://services.odata.org/V4/OData/OData.svc/Products(0)</id><category term=\"#ODataDemo.Product\" scheme=\"http://docs.oasis-open.org/odata/ns/scheme\" /><link rel=\"edit\" title=\"Product\" href=\"Products(0)\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/Categories\" type=\"application/xml\" title=\"Categories\" href=\"Products(0)/Categories/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/Categories\" type=\"application/atom+xml;type=feed\" title=\"Categories\" href=\"Products(0)/Categories\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/Supplier\" type=\"application/xml\" title=\"Supplier\" href=\"Products(0)/Supplier/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/Supplier\" type=\"application/atom+xml;type=entry\" title=\"Supplier\" href=\"Products(0)/Supplier\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/ProductDetail\" type=\"application/xml\" title=\"ProductDetail\" href=\"Products(0)/ProductDetail/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/ProductDetail\" type=\"application/atom+xml;type=entry\" title=\"ProductDetail\" href=\"Products(0)/ProductDetail\" /><title /><updated>2014-09-10T08:23:46Z</updated><author><name /></author><content type=\"application/xml\"><m:properties><d:ID m:type=\"Int32\">0</d:ID><d:Name>Bread</d:Name><d:Description>Whole grain bread</d:Description><d:ReleaseDate m:type=\"DateTimeOffset\">1992-01-01T00:00:00Z</d:ReleaseDate><d:DiscontinuedDate m:null=\"true\" /><d:Rating m:type=\"Int16\">4</d:Rating><d:Price m:type=\"Double\">2.5</d:Price></m:properties></content></entry><entry><id>http://services.odata.org/V4/OData/OData.svc/Products(1)</id><category term=\"#ODataDemo.Product\" scheme=\"http://docs.oasis-open.org/odata/ns/scheme\" /><link rel=\"edit\" title=\"Product\" href=\"Products(1)\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/Categories\" type=\"application/xml\" title=\"Categories\" href=\"Products(1)/Categories/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/Categories\" type=\"application/atom+xml;type=feed\" title=\"Categories\" href=\"Products(1)/Categories\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/Supplier\" type=\"application/xml\" title=\"Supplier\" href=\"Products(1)/Supplier/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/Supplier\" type=\"application/atom+xml;type=entry\" title=\"Supplier\" href=\"Products(1)/Supplier\" /><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/ProductDetail\" type=\"application/xml\" title=\"ProductDetail\" href=\"Products(1)/ProductDetail/$ref\" /><link rel=\"http://docs.oasis-open.org/odata/ns/related/ProductDetail\" type=\"application/atom+xml;type=entry\" title=\"ProductDetail\" href=\"Products(1)/ProductDetail\" /><title /><updated>2014-09-10T08:23:46Z</updated><author><name /></author><content type=\"application/xml\"><m:properties><d:ID m:type=\"Int32\">1</d:ID><d:Name>Milk</d:Name><d:Description>Low fat milk</d:Description><d:ReleaseDate m:type=\"DateTimeOffset\">1995-10-01T00:00:00Z</d:ReleaseDate><d:DiscontinuedDate m:null=\"true\" /><d:Rating m:type=\"Int16\">3</d:Rating><d:Price m:type=\"Double\">3.5</d:Price></m:properties></content></entry><link rel=\"next\" href=\"http://services.odata.org/V4/OData/OData.svc/Products?$skiptoken=12\"/></feed>");
            XmlNodeList entryNodes = xmlDoc.SelectNodes(@"/atom:feed/atom:entry", ODataNamespaceManager.Instance);

            if (null != entryNodes)
            {
                int count = 0;
                string url = context.DestinationBasePath + "/$count";
                var countResp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                if (null != countResp && HttpStatusCode.OK == countResp.StatusCode)
                {
                    count = Convert.ToInt32(countResp.ResponsePayload);
                    if (count > entryNodes.Count)
                    {
                        XmlNode nextLinkNode = xmlDoc.SelectSingleNode(@"/atom:feed/atom:link[@rel='next']", ODataNamespaceManager.Instance);

                        if (null != nextLinkNode)
                        {
                            var linkHrefNode = nextLinkNode.Attributes["href"];
                            if (null != linkHrefNode)
                            {
                                if (Uri.IsWellFormedUriString(linkHrefNode.Value, UriKind.RelativeOrAbsolute))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
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
