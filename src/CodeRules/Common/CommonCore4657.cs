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
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4657_Feed : CommonCore4657
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
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4657_Entry : CommonCore4657
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
    /// Class of code rule applying to DeltaResponse payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4657_DeltaResponse : CommonCore4657
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
    /// Class of extension rule for Common.Core.4657
    /// </summary>
    public abstract class CommonCore4657 : ExtensionRule
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
                return "Common.Core.4657";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Requests using the $format query option with the abbreviation atom MUST receive the MIME type application/atom+xml for entities and collections of entities, references, or changes.";
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
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

            const string toAccept = null;
            Uri absoluteUri = new Uri(context.Destination.OriginalString.Split('?')[0]);
            Uri relativeUri = new Uri("?$format=atom", UriKind.Relative);
            Uri combinedUri = new Uri(absoluteUri, relativeUri);
            Response response = WebHelper.Get(combinedUri, toAccept, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (response.StatusCode.HasValue && response.StatusCode.Value == System.Net.HttpStatusCode.OK)
            {
                var contentType = context.ResponseHttpHeaders.GetHeaderValue(Constants.ContentType);
                string[] pairs = contentType.Split(new char[] { ';', ' ' });
                foreach (string pair in pairs)
                {
                    if (pair.Equals("application/atom+xml"))
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

