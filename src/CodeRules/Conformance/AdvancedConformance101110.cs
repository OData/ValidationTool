// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;


    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.101110
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance101110 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.101110";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"8). Structurally, a batch response body MUST match one-to-one with the corresponding batch request body. (section 11.7.4)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.7.4";
            }
        }

        /// <summary>
        /// Gets the resource type to which the rule applies.
        /// </summary>
        public override ConformanceServiceType? ResourceType
        {
            get
            {
                return ConformanceServiceType.ReadWrite;
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
            var serviceStatus = ServiceStatus.GetInstance();
            string feedUrl = string.Empty;
            string entityUrl = string.Empty;
            KeyValuePair<string, IEnumerable<string>> entityUrls;
            if (JsonParserHelper.GetBatchSupportedEntityUrls(out entityUrls))
            {
                feedUrl = string.Format("{0}/{1}", context.ServiceBaseUri, entityUrls.Key);
                entityUrl = entityUrls.Value.First();
            }

            string relativeUrl = new Uri(entityUrl).LocalPath;
            string host = entityUrl.Remove(entityUrl.IndexOf(relativeUrl));

            string batchRequest = string.Format(@"
--batch_36522ad7-fc75-4b56-8c71-56071383e77b
Content-Type: application/http 
Content-Transfer-Encoding:binary

GET {0} HTTP/1.1

--batch_36522ad7-fc75-4b56-8c71-56071383e77b
Content-Type: application/http 
Content-Transfer-Encoding:binary

GET {0} HTTP/1.1

--batch_36522ad7-fc75-4b56-8c71-56071383e77b--", entityUrl);

            string boundary = @"batch_36522ad7-fc75-4b56-8c71-56071383e77b";
            List<string> batchResponseSigns = new List<string>();
            Response batchResponse = WebHelper.BatchOperation(context.ServiceBaseUri.OriginalString, batchRequest, boundary);
            var detail = new ExtensionRuleResultDetail(this.Name, context.ServiceBaseUri.OriginalString + "/$batch", HttpMethod.Post, string.Empty, batchResponse, string.Empty, batchRequest);

            if (batchResponse != null && !string.IsNullOrEmpty(batchResponse.ResponsePayload))
            {
                string[] responseSegments = batchResponse.ResponsePayload.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string seg in responseSegments)
                {
                    if (seg.StartsWith(@"--batchresponse"))
                    {
                        batchResponseSigns.Add(seg);
                    }
                }

                // The request has 3 batch separators
                if (batchResponseSigns.Count == 3)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = string.Format("The batch response body does not match one-to-one with the batch request body, the batch request has 3 batch separators, but response has {0} batch separators. ", batchResponseSigns.Count);
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = string.Format("Batch request failed: {0}.", batchRequest);
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
