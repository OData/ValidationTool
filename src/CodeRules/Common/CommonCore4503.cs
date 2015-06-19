// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to feed payload.  
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4503_Feed : CommonCore4503
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
    /// Class of extension rule for Common.Core.4503
    /// </summary>
    public abstract class CommonCore4503 : ExtensionRule
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
                return "Common.Core.4503";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "When odata.metadata=minimal, the response payload MUST contain odata.count common annotation, if requested.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "3.1.1";
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

            JObject jo;
            context.ResponsePayload.TryToJObject(out jo);

            string acceptHeader = string.Empty;
            string odataCountAnnoatation = Constants.V4OdataCount;

            if (context.Version == ODataVersion.V3)
            {
                odataCountAnnoatation = Constants.OdataCount;
            }

            // The odata.count annotation contains the count of a feed
            if (context.PayloadType == RuleEngine.PayloadType.Feed)
            {
                // Use regex to remove all blanks.
                if (!Regex.Replace(context.Destination.ToString(), @"\s+", "").Contains(@"inlinecount=allpages")
                    && !Regex.Replace(context.Destination.ToString(), @"\s+", "").Contains(@"?$count=true"))
                {
                    Uri absoluteUri = new Uri(context.Destination.OriginalString.Split('?')[0]);
                    Uri relativeUri = null;

                    if (context.Version == ODataVersion.V3)
                    {
                        acceptHeader = Constants.V3AcceptHeaderJsonMinimalMetadata;
                        relativeUri = new Uri("?$inlinecount=allpages", UriKind.Relative);
                    }
                    else if (context.Version == ODataVersion.V4)
                    {
                        acceptHeader = Constants.V4AcceptHeaderJsonMinimalMetadata;
                        relativeUri = new Uri("?$count=true", UriKind.Relative);
                    }

                    Uri combinedUri = new Uri(absoluteUri, relativeUri);

                    // Send request with query parameter "inlinecount=allpages"
                    Response response = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        response.ResponsePayload.TryToJObject(out jo);
                    }
                }

                foreach (JProperty jp in jo.Children())
                {
                    if (jp.Name.Equals(odataCountAnnoatation))
                    {
                        passed = true;
                        break;
                    }
                }

                if (passed == null)
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }   
            }
           
            return passed;
        }       
    }
}
