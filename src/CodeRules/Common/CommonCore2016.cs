// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code of rule #251 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_Entry : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #251 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_Feed : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Feed;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #251 when payload is a service document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_ServiceDocument : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.ServiceDoc;
            }
        }
    }


    /// <summary>
    /// class of concrete code of rule #251 when payload is a metadata document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_Metadata : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Metadata;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #251 when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_Property : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Property;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #251 when payload is a Link
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_Link : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Link;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #251 when payload is a raw value
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2016_RawValue : CommonCore2016
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.RawValue;
            }
        }
    }

    /// <summary>
    /// Abstract base class of rule #251
    /// </summary>
    public abstract class CommonCore2016 : ExtensionRule
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
                return "Common.Core.2016";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The VersionClientUserAgent section, as specified in the previous listing in this section, DataServiceVersion Header ABNF GrammarService Operation Parameters, of the header value is not significant, SHOULD NOT be interpreted by a data service, and SHOULD NOT affect the versioning semantics of a data service.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.3";
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
                return RequirementLevel.ShouldNot;
            }
        }

        /// <summary>
        /// Gets the aspect property.
        /// </summary>
        public override string Aspect
        {
            get
            {
                return "semantic";
            }
        }

        /// <summary>
        /// Gets the flag of context being offline validation
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag of metadata document availability
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
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

            info = null;
            bool? passed = null;
            string odataVersionString = "DataServiceVersion";

            if (context.Version == ODataVersion.V4)
            {
                odataVersionString = "OData-Version";
            }

            //to get the DSV defined in metadata document
            XElement meta = XElement.Parse(context.MetadataDocument);
            string xpath = @"//*[local-name()='DataServices' and @m:DataServiceVersion]";
            var ds = meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            if (ds != null)
            {
                string dsvInResp = context.ResponseHttpHeaders.GetHeaderValue(odataVersionString);
                string dsvInMeta = ds.GetAttributeValue("m:DataServiceVersion", ODataNamespaceManager.Instance);

                if (!string.IsNullOrEmpty(dsvInResp) && !string.IsNullOrEmpty(dsvInMeta))
                {
                    string dsv = GetBiggerDSV(ResourcePathHelper.GetMajorHeaderValue(dsvInMeta), ResourcePathHelper.GetMajorHeaderValue(dsvInResp));
                    var headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(odataVersionString, dsv + VersionClientUserAgent) };

                    var resp = WebResponseHelper.GetWithHeaders(context.Destination,
                        context.PayloadFormat == RuleEngine.PayloadFormat.Json,
                        headers,
                        RuleEngineSetting.Instance().DefaultMaximumPayloadSize,
                        context);

                    //to approximate with checking with status code and responded DSV
                    if (context.HttpStatusCode != resp.StatusCode)
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, resp.StatusCode.ToString());
                    }
                    else
                    {
                        var DsvOld = context.ResponseHttpHeaders.GetHeaderValue(odataVersionString);
                        var DsvNew = resp.ResponseHeaders.GetHeaderValue(odataVersionString);
                        passed = DsvOld == DsvNew;
                        if (!passed.Value)
                        {
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, resp.ResponsePayload);
                        }
                    }
                }
            }
           
            return passed;
        }

        private string GetBiggerDSV(string dsv1, string dsv2)
        {
            if (string.IsNullOrEmpty(dsv1))
            {
                return dsv2;
            }
            else if (string.IsNullOrEmpty(dsv2))
            {
                return dsv1;
            }
            else
            {
                Decimal v1 = Convert.ToDecimal(dsv1);
                Decimal v2 = Convert.ToDecimal(dsv2);
                return (v1 >= v2) ? dsv1 : dsv2;
            }
        }

        private const string VersionClientUserAgent = @";4.5;Interop 6.7;DataServiceVersion:8.9;";
    }
}
