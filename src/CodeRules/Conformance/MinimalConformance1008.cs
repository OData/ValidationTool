// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1008
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1008 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1008";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "8. MUST expose only data types defined in [OData-CSDL]";
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

            bool? passed = true;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);

            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            string entitySetName = feeds.First().MapEntitySetURLToEntitySetName();
            Uri firstFeedFullUrl = new Uri(string.Format("{0}/{1}", context.DestinationBasePath, entitySetName));
            Response response = WebHelper.Get(firstFeedFullUrl, Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, firstFeedFullUrl.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);
            
            if (response != null && response.StatusCode != null)
            {
                JObject feed;
                response.ResponsePayload.TryToJObject(out feed);
                var entries = JsonParserHelper.GetEntries(feed);

                foreach (JObject entry in entries)
                {
                    var jProps = entry.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Name.EndsWith("@" + Constants.OdataType) && !jProp.Name.StartsWith("@" + Constants.OdataType))
                        {
                            string typeValue = jProp.Value.ToString().TrimStart('#');

                            if (typeValue.Contains("Collection("))
                            {
                                typeValue = typeValue.Substring(typeValue.IndexOf("(") + 1, typeValue.LastIndexOf(")") - typeValue.IndexOf("(") - 1);
                            }

                            // Don't contains dot means it is primitive value or collection.
                            if (!typeValue.Contains("."))
                            {
                                if (PrimitiveDataTypes.NonQualifiedTypes.Contains(typeValue))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    detail.ErrorMessage += string.Format("The type {0} is not defined in OData-CSDL.", typeValue);
                                    break;
                                }
                            }
                        }
                    }

                    if (passed == false)  // Break to foreach all if anyone of them cannot match the rule
                    {
                        break;
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = string.Format(Constants.ErrorURI, "GET", firstFeedFullUrl.AbsoluteUri, "failed");
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
