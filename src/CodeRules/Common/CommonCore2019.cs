// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Concrete class of rule #1137 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2019_Feed : CommonCore2019
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
    /// Concrete class of rule #1137 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2019_Entry : CommonCore2019
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
    /// base class of rule #1137
    /// </summary>
    public abstract class CommonCore2019 : ExtensionRule
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
                return "Common.Core.2019";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If all property mappings that have been defined in the CSDL on EntityTypes addressed by the request contain an FC_KeepInContent attribute with value \"true\","
                    +" then the Data Service SHOULD respond with a 1.0 version response provided there is no other aspect of the response that would cause the version of the response to be higher.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "3.2.5.2.1";
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
                return RequirementLevel.Should;
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
            string odataVersionString = "DataServiceVersion";

            if (context.Version == ODataVersion.V4)
            {
                odataVersionString = "OData-Version";
            }

            if (!ToElevateResponseVersion(context))
            {
                // for any entity type in the response payload
                // to get its type definition and check if it has any property mapping
                HashSet<string> entityTypes = new HashSet<string>();
                const string xpath = @"//*[local-name()='category' and @term]";
                XElement payload = XElement.Parse(context.ResponsePayload);
                var ets = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
                foreach (var et in ets)
                {
                    entityTypes.Add(et.GetAttributeValue("term"));
                }

                bool hasPropertyMapping = false;

                if (entityTypes.Any())
                {
                    XElement meta = XElement.Parse(context.MetadataDocument);
                    foreach (var et in entityTypes)
                    {
                        hasPropertyMapping = ResourcePathHelper.HasPropertyMapping(meta, et);
                        if (hasPropertyMapping)
                        {
                            break;
                        }
                    }
                }

                if (!hasPropertyMapping)
                {
                    var dsvInResp = context.ResponseHttpHeaders.GetHeaderValue(odataVersionString);
                    var majorVersion = ResourcePathHelper.GetMajorHeaderValue(dsvInResp);
                    Double version = Convert.ToDouble(majorVersion);
                    passed = version == 1.0;
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, dsvInResp);
                    }
                }
            }

            return passed;
        }

        private static bool ToElevateResponseVersion(ServiceContext context)
        {
            bool result = false;

            // requests containing $inlinecount=allpages would elevate the response DSV
            string value;
            bool hasInlineCount = ResourcePathHelper.HasQueryOption(context.Destination.Query, "$inlinecount", out value);
            if (hasInlineCount && value.Equals("allpages", StringComparison.Ordinal))
            {
                result = true;
            }

            //TODO: other aspenct ould elevate the response DSV

            return result;
        }
    }
}

