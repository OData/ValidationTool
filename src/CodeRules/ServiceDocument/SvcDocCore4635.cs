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
    /// Class of extension rule for SvcDoc.Core.4635
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class SvcDocCore4635 : ExtensionRule
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
                return "SvcDoc.Core.4635";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Requests using the $format query option with the abbreviation atom MUST receive the MIME type application/atomsvc+xml for the service document.";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
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
                return RuleEngine.PayloadType.ServiceDoc;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
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
        /// Verify rule logic
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

            string acceptHeader = null;
            Uri absoluteUri = new Uri(context.ServiceBaseUri.OriginalString.Split('?')[0]);
            Uri relativeUri = new Uri("?$format=atom", UriKind.Relative);
            Uri combinedUri = new Uri(absoluteUri, relativeUri);
            Response response = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (response.StatusCode.HasValue && response.StatusCode.Value == System.Net.HttpStatusCode.OK)
            {
                var contentType = context.ResponseHttpHeaders.GetHeaderValue(Constants.ContentType);
                string[] pairs = contentType.Split(new char[] { ';', ' ' });
                foreach (string pair in pairs)
                {
                    if (pair.Equals("application/atomsvc+xml"))
                    {
                        passed = true;
                        break;
                    }
                    else
                    {
                        passed = false;
                    }
                }
            }
            else
            {
                passed = false;
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(ErrorMessage, context.Destination, response.StatusCode.ToString());
            }

            return passed;
        }
    }
}