// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Net;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1003
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1003 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "3. MUST support server-driven paging when returning partial results (section 11.2.5.7)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.1";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            var countRestrictions = AnnotationsHelper.GetCountRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(countRestrictions.Item1))
            {
                detail.ErrorMessage = "Cannot find an entity-set which supports $count system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string entitySet = countRestrictions.Item1;
            bool isNextLinkPropExist = false;
            Int64 totalCount = 0;
            string url = string.Format("{0}/{1}?$count=true", context.ServiceBaseUri.AbsoluteUri.TrimEnd('/'), entitySet);
            Response resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), resp);
            
            if (resp.StatusCode.HasValue && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);
                var entries = JsonParserHelper.GetEntries(feed);

                if (feed[Constants.V4OdataCount] != null)
                {
                    totalCount = Convert.ToInt64(feed[Constants.V4OdataCount].Value<string>().StripOffDoubleQuotes());

                    if (entries.Count == totalCount)
                    {
                        passed = true;
                    }
                    else if (entries.Count < totalCount)
                    {
                        var jProps = feed.Children();

                        foreach (JProperty jProp in jProps)
                        {
                            if (jProp.Name == Constants.V4OdataNextLink)
                            {
                                isNextLinkPropExist = true;
                                break;
                            }
                        }

                        if (isNextLinkPropExist)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            detail.ErrorMessage = string.Format("The feed has {0} entities totally, but it only display {1} entities and there is no NextLink annotation.", totalCount, entries.Count);
                        }
                    }
                    else
                    {
                        passed = null;
                        detail.ErrorMessage = string.Format("The response of feed {0} has only one page, so can not test partial results.", entitySet);
                    }
                }
                else
                {
                    passed = null;
                    detail.ErrorMessage = string.Format("The response of feed {0} does not have \"@Odata.count\" annotation so it cannot get the total count of entities.", entitySet);
                }
            }
            else
            {
                passed = null;
                detail.ErrorMessage = "Cannot get response from above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
