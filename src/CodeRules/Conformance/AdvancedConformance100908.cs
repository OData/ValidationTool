// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.100908
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100908 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100908";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.8. SHOULD support $levels for recursive expand (section 11.2.4.2.1.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            var restrictions = AnnotationsHelper.GetExpandRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(restrictions.Item1) ||
                null == restrictions.Item3 || !restrictions.Item3.Any())
            {
                detail.ErrorMessage = "Cannot find any appropriate entity-sets which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            // Set a expected level number as 2, and store it in the parameter expectedLevel.
            int expectedLevels = 2;
            string entitySetName = restrictions.Item1;
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            string[] navigPropNames = MetadataHelper.GetNavigPropNamesRecurseByLevels(entityTypeShortName, context.MetadataDocument, expectedLevels);
            string url = string.Format("{0}/{1}?$expand=*($levels={2})", context.ServiceBaseUri, entitySetName, expectedLevels);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);
            
            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);
                var entities = JsonParserHelper.GetEntries(feed);

                if (null == entities || !entities.Any())
                {
                    detail.ErrorMessage = string.Format("Cannot find any entities from the entity-set '{0}'", entitySetName);
                    info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                    return passed;
                }

                bool isFirstLevelEmpty = false;
                int levelsCounter = 0;
                JObject entry = null;

                foreach (var en in entities)
                {
                    levelsCounter = 0;
                    entry = en as JObject;
                    isFirstLevelEmpty = false;
                    for (int i = 0; i < expectedLevels; i++)
                    {
                        if (entry != null && JTokenType.Object == entry.Type)
                        {
                            var navigPropVal = entry[navigPropNames[i]];

                            if (navigPropVal != null)
                            {
                                levelsCounter++;
                                entry = navigPropVal.Type == JTokenType.Array ? navigPropVal.First as JObject : navigPropVal as JObject;
                                if (entry == null)
                                {
                                    isFirstLevelEmpty = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (expectedLevels == levelsCounter)
                    {
                        passed = true;
                    }
                    else if (!(levelsCounter < expectedLevels && isFirstLevelEmpty)) // If not no data
                    {
                        passed = false;
                        detail.ErrorMessage = "The service does not execute an accurate result on the system query option '$levels' for expanded properties.";

                        break;
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support the system query option '$levels' for expanded properties.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
