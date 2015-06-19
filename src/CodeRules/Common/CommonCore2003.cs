// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule to check semantic expectation. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2003 : ExtensionRule
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
                return "Common.Core.2003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the request includes a MaxDataServiceVersion (section 2.2.5.7) header, the server MUST parse and validate the header value to ensure it adheres to the syntax specified in MaxDataServiceVersion (section 2.2.5.7).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "3.2.5.1";
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
                return "If the request includes a MaxDataServiceVersion (section 2.2.5.7) header, the server MUST parse and validate the header value to ensure it adheres to the syntax specified in MaxDataServiceVersion (section 2.2.5.7).";
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
                return null;
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
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
            const string MalformedMDSV = "malformed";

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool passed = false;
            info = null;
            var headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("MaxDataServiceVersion", MalformedMDSV) };
            var resp = WebResponseHelper.GetWithHeaders(
                context.Destination, 
                context.PayloadFormat == RuleEngine.PayloadFormat.Json,
                headers,
                RuleEngineSetting.Instance().DefaultMaximumPayloadSize,
                context);

            if (resp.StatusCode.HasValue)
            {
                int code = (int)resp.StatusCode.Value;
                if (code >= 400 && code < 500)
                {
                    passed = true;
                }
                else
                {
                    info = new ExtensionRuleViolationInfo(Resource.ExpectingError4xx, context.Destination, resp.StatusCode.Value.ToString());
                }
            }
            else
            {
                info = new ExtensionRuleViolationInfo(Resource.ExpectingError4xx, context.Destination, null);
            }

            return passed;
        }
    }
}
