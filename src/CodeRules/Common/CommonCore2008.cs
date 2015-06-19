// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of code rule to check semantic expectation. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2008 : ExtensionRule
    {
        /// <summary>
        /// Gets Categpry property
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
                return "Common.Core.2008";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"URI16: if the Entity Data Model associated with the data service does not include an EntitySet instance with the keyPredicate specified, "
                +@"then this URI MUST be treated as identifying a non-existent resource, as described in Message Processing Events and Sequencing Rules (section 3.2.5).";
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
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
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
        /// Gets the payload type this rule applies to
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Error;
            }
        }

        /// <summary>
        /// Gets the flag whether it applies to projection query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out paramater to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool passed = true;
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

                    if (resp.StatusCode.HasValue)
                    {
                        var code = resp.StatusCode.Value;
                        bool isNonExistentCode = (code == System.Net.HttpStatusCode.NotFound) || (code == System.Net.HttpStatusCode.Gone);
                        if (isNonExistentCode)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo("unexpected status code", target, code.ToString());
                        }
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo("no status code", target, null);
                    }
                }
            }

            return passed;
        }
    }
}
