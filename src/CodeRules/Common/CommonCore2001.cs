// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /* Cannot know where is the specification for V4
    /// <summary>
    /// Rule for v4 version.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2001_V4 : CommonCore2001
    {
        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If a request includes a OData-Version header, the server MUST validate that the header value is correctly formatted according to the rules in OData-Version.";
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
    }*/

    /// <summary>
    /// Rule for v1, v2, v3 version.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2001_V1_V2_V3 : CommonCore2001
    {
        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If a request includes a DataServiceVersion (section 2.2.5.3) header, the server MUST validate that the header value is correctly formatted according to the rules in DataServiceVersion.";
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
    }


    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>  
    public abstract class CommonCore2001 : ExtensionRule
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
                return "Common.Core.2001";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If a request includes a DataServiceVersion (section 2.2.5.3) header, the server MUST validate that the header value is correctly formatted according to the rules in DataServiceVersion.";
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

            bool passed = false;
            info = null;
            string odataVersionString = "DataServiceVersion";

            if (context.Version == ODataVersion.V4)
            {
                odataVersionString = "OData-Version";
            }

            var headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(odataVersionString, "m.n") };
            var resp = WebResponseHelper.GetWithHeaders(
                context.Destination,
                context.PayloadFormat == RuleEngine.PayloadFormat.Json || context.PayloadFormat == RuleEngine.PayloadFormat.JsonLight,
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
