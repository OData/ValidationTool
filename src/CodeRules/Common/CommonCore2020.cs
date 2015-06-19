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
    /// Concrete class of rule #1138 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2020_Feed : CommonCore2020
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
    /// Concrete class of rule #1138 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2020_Entry : CommonCore2020
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
    /// base class of rule #1138
    /// </summary>
    public abstract class CommonCore2020 : ExtensionRule
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
                return "Common.Core.2020";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If any EntityType addressed by the request has a property mapping defined on it in the CSDL with an FC_KeepInContent attribute with a value of false,"                
                +" then the Data Service MUST respond with a 2.0 or greater version response.";
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
                return RequirementLevel.Must;
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2_V3;
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

            if (hasPropertyMapping)
            {
                var dsvInResp = context.ResponseHttpHeaders.GetHeaderValue(odataVersionString);
                var majorVersion = ResourcePathHelper.GetMajorHeaderValue(dsvInResp);
                Double version = Convert.ToDouble(majorVersion);
                passed = version >= 2.0;
                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, dsvInResp);
                }
            }

            return passed;
        }
    }
}