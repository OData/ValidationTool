// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to ServiceDoc payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4003_ServiceDoc : CommonCore4003
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
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
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4003_Entry : CommonCore4003
    {
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
    }

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4003_Feed : CommonCore4003
    {
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
    }

    /// <summary>
    /// Class of code rule applying to DeltaResponse payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4003_DeltaResponse : CommonCore4003
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Delta;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to other payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4003_Other : CommonCore4003
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Other;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4003
    /// </summary>
    public abstract class CommonCore4003 : ExtensionRule
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
                return "Common.Core.4003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"Responses MAY add the odata.streaming parameter with a value of true or false.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.1";
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
                return RequirementLevel.May;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.JsonLight;
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
                return false;
            }
        }

        /// <summary>
        /// Verify Common.Core.4003
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
            string contentType = string.Empty;
            bool isContainTrue = false;
            bool isContainFalse = false;
            string absoluteUri = context.Destination.OriginalString.Split('?')[0];
            Uri combinedUri = new Uri(absoluteUri);

            // Send request with odata.streaming=true query parameter.
            string acceptHeader = "application/json;odata.streaming=true";
            Response response = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (response != null && !string.IsNullOrEmpty(response.ResponseHeaders))
            {
                // Get Content-Type from response headers.
                contentType = context.ResponseHttpHeaders.GetHeaderValue("Content-Type");

                // Split with semicolon.
                string[] elements = contentType.Split(';');

                foreach (string element in elements)
                {
                    // Whether odata.streaming exist in response header.
                    if (element.Contains("odata.streaming=true"))
                    {
                        isContainTrue = true;
                        break;
                    }
                    else
                    {
                        isContainTrue = false;
                    }
                }
            }

            // Send request with odata.streaming=false query parameter.
            acceptHeader = "application/json;odata.streaming=false";
            response = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (response != null && !string.IsNullOrEmpty(response.ResponseHeaders))
            {
                // Get Content-Type from response headers.
                contentType = context.ResponseHttpHeaders.GetHeaderValue("Content-Type");

                // Split with semicolon.
                string[] elements = contentType.Split(';');

                foreach (string element in elements)
                {
                    // Whether odata.streaming exist in response header.
                    if (element.Contains("odata.streaming=false"))
                    {
                        isContainFalse = true;
                        break;
                    }
                    else
                    {
                        isContainFalse = false;
                    }
                }
            }

            if (isContainTrue || isContainFalse)
            {
                passed = true;
            }
            else
            {
                passed = false;
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}

