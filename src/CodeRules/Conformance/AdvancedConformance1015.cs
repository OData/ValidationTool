// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.Linq;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.1015
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1015 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1015";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "15.	SHOULD support cross-join queries defined in [OData-URL].";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
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
            JObject jo;

            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);

            if (feeds.Length < 2)
            {
                passed = false;
                detail.ErrorMessage = "It cannot test cross-join because there are less than two entity sets.";
            }
            else
            {
                string crossJoinSegment = string.Format("$crossjoin({0},{1})", feeds[0], feeds[1]);

                if (!context.DestinationBasePath.EndsWith(@"/"))
                {
                    crossJoinSegment = @"/" + crossJoinSegment;
                }

                Uri crossJoinUrl = new Uri(context.DestinationBasePath + crossJoinSegment);
                var req = WebRequest.Create(crossJoinUrl) as HttpWebRequest;
                Response response = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
                detail = new ExtensionRuleResultDetail(this.Name, crossJoinUrl.AbsoluteUri, "GET", string.Empty, response);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    response.ResponsePayload.TryToJObject(out jo);

                    if (jo != null)
                    {
                        if (jo[Constants.OdataV4JsonIdentity] != null && jo[Constants.Value] != null)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            detail.ErrorMessage = "The response does not have '@Odata.context', 'value' properties.";
                        }
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = "The response is not the cross join entity sets. Please refer to section 4.11 of [OData-URL].";
                    }
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "The service does not support cross-join queries, please refer to section 4.11 of [OData-URL].";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
