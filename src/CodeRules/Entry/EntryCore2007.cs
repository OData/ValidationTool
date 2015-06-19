// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Abstract Base Class of extension rule for Entry.Core.2007
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2007 : ExtensionRule
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
                return "Entry.Core.2007";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"URI16 = scheme serviceRoot ""/"" entitySet ""("" keyPredicate "")"" count MAY identify the count of a single EntityType instance "
                    + @"(the count value SHOULD always equal one), which is within the EntitySet specified in the URI, "
                    + @"where key EntityKey is equal to the value of the keyPredicate specified";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.5";
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

            // check there is only one segement in the relative path (after the service root uri)
            // ensure the segment is a URI-2
            string path;
            if (ResourcePathHelper.IsOneSegmentPath(context, out path))
            {
                if (RegexInUri.IsURI2(path, XElement.Parse(context.MetadataDocument)))
                {
                    // fetch the .../$count resource
                    Uri target = new Uri(context.DestinationBasePath + "/$count");
                    var resp = WebResponseHelper.GetWithHeaders(
                        target,
                        false,  // $count response cannot be application/json Content-Type
                        null,
                        RuleEngineSetting.Instance().DefaultMaximumPayloadSize,
                        context);
                    string payload = resp.ResponsePayload;
                    int count;
                    if (Int32.TryParse(payload, out count))
                    {
                        passed = count == 1;
                    }
                    else
                    {
                        passed = false;
                    }

                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo("unexpected payload content", target, payload);
                    }
                }
            }

            return passed;
        }
    }
}

