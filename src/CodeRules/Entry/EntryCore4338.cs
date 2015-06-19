// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4338
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4338 : ExtensionRule
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
                return "Entry.Core.4338";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the assosiation link is represented, it MUST immediately precede the navigation link if the latter is represented.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.2";
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
        /// Gets the odata metadata type to which the rule applies.
        /// </summary>
        public override ODataMetadataType? OdataMetadataType
        {
            get
            {
                return ODataMetadataType.FullOnly;
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
            JObject entry = null;
            JProperty expandAssociationProp = null;

            context.ResponsePayload.TryToJObject(out entry);
            List<string> expandProps = ODataUriAnalyzer.GetQueryOptionValsFromUrl(context.Destination.ToString(), @"expand");
            
            if (entry == null || entry.Type != JTokenType.Object || expandProps == null)
                return null;

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                if (!jProp.Name.Contains(Constants.OdataNavigationLinkPropertyNameSuffix))
                    continue;

                if (!jProp.Name.Contains(@"@"))
                    continue;

                int index = jProp.Name.IndexOf(@"@");
                string temp = jProp.Name.Remove(index, jProp.Name.Length - index);

                if (!expandProps.Contains(temp))
                    continue;

                if (jProp.Previous == null)
                    continue;

                expandAssociationProp = jProp.Previous as JProperty;
                if (expandAssociationProp.Name.Contains(Constants.OdataAssociationLinkPropertyNameSuffix))
                {
                    if (!expandAssociationProp.Name.Contains(@"@"))
                        continue;

                    index = expandAssociationProp.Name.IndexOf(@"@");
                    temp = expandAssociationProp.Name.Remove(index, expandAssociationProp.Name.Length - index);

                    if (expandProps.Contains(temp))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
