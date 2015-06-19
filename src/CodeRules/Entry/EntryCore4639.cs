// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Entry
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4639
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4639 : ExtensionRule
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
                return "Entry.Core.4639";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For navigation properties declared by a complex type that is used in a collection of complex type values, the URL should be the canonical URL of the target entity.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1.1.2";
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
                return null;
            }
        }

        /// <summary>
        /// Verify Entry.Core.4639
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

            var complexTypeProps = MetadataHelper.GetComplexTypeProperties(context.MetadataDocument, context.EntityTypeShortName, true);
            var payload = XElement.Parse(context.ResponsePayload);
            //payload = XElement.Parse("<entry xml:base=\"http://services.odata.org/V4/OData/(S(pua1r5tipwt5cdl3zpffct2u))/OData.svc/\" m:context=\"http://services.odata.org/V4/OData/(S(pua1r5tipwt5cdl3zpffct2u))/OData.svc/$metadata#Suppliers/$entity\" m:etag=\"W/&quot;0&quot;\" xmlns=\"http://www.w3.org/2005/Atom\" xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\"><id>http://services.odata.org/V4/OData/(S(pua1r5tipwt5cdl3zpffct2u))/OData.svc/Suppliers(0)</id><category term=\"#ODataDemo.Supplier\" scheme=\"http://docs.oasis-open.org/odata/ns/scheme\"/><link rel=\"edit\" title=\"Supplier\" href=\"Suppliers(0)\"/><link rel=\"http://docs.oasis-open.org/odata/ns/relatedlinks/Products\" type=\"application/xml\" title=\"Products\" href=\"Suppliers(0)/Products/$ref\"/><link rel=\"http://docs.oasis-open.org/odata/ns/related/Products\" type=\"application/atom+xml;type=feed\" title=\"Products\" href=\"Suppliers(0)/Products\"/><title/><updated>2014-09-15T07:55:18Z</updated><author><name/></author><content type=\"application/xml\"><m:properties><d:ID m:type=\"Int32\">0</d:ID><d:Name>Exotic Liquids</d:Name><d:Address m:type=\"#ODataDemo.Address\"><m:element><link rel=\"http://docs.oasis-open.org/odata/ns/related/Country\" href=\"Countries('DE')\" type=\"application/atom+xml;type=entry\" title=\"Country\" /></m:element></d:Address><d:Location m:type=\"GeographyPoint\"><gml:Point gml:srsName=\"http://www.opengis.net/def/crs/EPSG/0/4326\"><gml:pos>47.6316604614258 -122.03547668457</gml:pos></gml:Point></d:Location><d:Concurrency m:type=\"Int32\">0</d:Concurrency></m:properties></content></entry>");

            if (null != payload)
            {
                foreach (var cp in complexTypeProps)
                {
                    if (null != cp.Attribute("Name"))
                    {
                        var propName = cp.Attribute("Name").Value;
                        string xPath = string.Format("//*[local-name()='{0}']/*[local-name()='element']/*[local-name()='link']", propName);
                        var linkElems = payload.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

                        foreach (var le in linkElems)
                        {
                            if (null != le.Attribute("href"))
                            {
                                var hrefVal = le.Attribute("href").Value;
                                if (Uri.IsWellFormedUriString(hrefVal, UriKind.RelativeOrAbsolute))
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

                        if (passed == false)
                        {
                            break;
                        }
                    }
                }
            }

            return passed;
        }
    }
}
