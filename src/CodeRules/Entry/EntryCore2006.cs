// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
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
    /// Abstract Base Class of extension rule for Entry.Core.2006
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2006 : ExtensionRule
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
                return "Entry.Core.2006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the request includes an If-Match (section 2.2.5.5) or an If-None-Match (section 2.2.5.6) header but the EntityType associated with the resource identified by the request URI, referred to as the target EntityType, does not define a concurrency token, then the server MUST return a 4xx error response.";
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
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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
        /// Verify the rule
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

            // if concurrency token is not defined in metadata for the entity type, 
            // verify the service in question returns 4xx code when If-Match header 
            XElement meta;
            context.MetadataDocument.TryToXElement(out meta);
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @ConcurrencyMode='Fixed']",
                context.EntityTypeShortName);
            bool IsConcurrent = meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance) != null;
            if (!IsConcurrent)
            {
                var headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("If-Match", "\t") };
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
                        passed = false;
                    }
                }
                else
                {
                    info = new ExtensionRuleViolationInfo(Resource.ExpectingError4xx, context.Destination, null);
                    passed = false;
                }
            }

            return passed;
        }
    }
}

