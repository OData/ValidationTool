// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    #endregion

    /// <summary>
    /// Helper class to query the global rule respository for applicable rules
    /// </summary>
    public static class RuleSelector
    {
        /// <summary>
        /// Gets all applicable rules to a specific context
        /// </summary>
        /// <param name="contextTrait">the current interop request context these rules are applicable</param>
        /// <returns>rules applicable to the specified request context</returns>
        public static IEnumerable<Rule> GetRules(this ServiceContextCore contextTrait)
        {
            if (contextTrait == null)
            {
                throw new ArgumentNullException("contextTrait");
            }

            return RuleCatalogCollection.Instance.SelectRulesOfCategory(contextTrait.Category)
                .SelectRules(contextTrait.PayloadType, contextTrait.IsMediaLinkEntry, contextTrait.Projection)
                .SelectRules(contextTrait.PayloadFormat)
                .SelectRulesByMetadataFlag(contextTrait.OdataMetadataType)
                .SelectRulesByConformanceType(contextTrait.ServiceType, contextTrait.LevelTypes)
                .SelectRules(contextTrait.Version)
                .SelectRulesByMetadata(contextTrait.HasMetadata)
                .SelectRulesByServiceDocument(contextTrait.HasServiceDocument)
                .SelectRulesByOfflineFlag(contextTrait.IsOffline);
        }

        /// <summary>
        /// Filters rules based on rule category.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="category">Rule category</param>
        /// <returns>The subset of input rules that meet the condition of category</returns>
        public static IEnumerable<Rule> SelectRulesOfCategory(this IEnumerable<Rule> rules, string category)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            return from r in rules
                   where r.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase)
                   select r;
        }

        /// <summary>
        /// Filters rules based on payload type and flag of media link entry.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="payloadType">Payload type</param>
        /// <param name="isMediaLinkEntry">Flag of media link entry</param>
        /// <param name="projection">Flag of projected request</param>
        /// <returns>The subset of input rules that meet the conditions of payload type and media link entry flag</returns>
        private static IEnumerable<Rule> SelectRules(this IEnumerable<Rule> rules, PayloadType payloadType, bool isMediaLinkEntry, bool projection)
        {
            rules = from r in rules
                   where !r.PayloadType.HasValue || r.PayloadType == payloadType
                   select r;
            
            if (payloadType == PayloadType.Entry)
            {
                rules = from r in rules
                        where !r.IsMediaLinkEntry.HasValue || r.IsMediaLinkEntry.Value == isMediaLinkEntry
                        select r;
            }

            if (payloadType == PayloadType.Entry || payloadType == PayloadType.Feed)
            {
                rules = from r in rules
                        where !r.Projection.HasValue || r.Projection.Value == projection
                        select r;
            }

            return rules;
        }

        /// <summary>
        /// Filters rules based on payload format.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="payloadFormat">Payload format</param>
        /// <returns>The subset of input rules that meet the condition of payload format</returns>
        private static IEnumerable<Rule> SelectRules(this IEnumerable<Rule> rules, PayloadFormat payloadFormat)
        {
            return from r in rules
                   where !r.PayloadFormat.HasValue || r.PayloadFormat == payloadFormat
                   select r;
        }

        /// <summary>
        /// Filters rules based on OData version.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="version">OData version</param>
        /// <returns>The subset of input rules that meet the condition of OData version</returns>
        public static IEnumerable<Rule> SelectRules(this IEnumerable<Rule> rules, ODataVersion version)
        {
            return from r in rules
                   where (!r.Version.HasValue && (version == ODataVersion.V1 || version == ODataVersion.V2 || version == ODataVersion.V3 || version == ODataVersion.V1_V2) || version == ODataVersion.V1_V2_V3)
                  || r.Version == version 
                  || (r.Version == ODataVersion.V1_V2 && (version == ODataVersion.V1 || version == ODataVersion.V2))
                  || (version == ODataVersion.V1_V2 && (r.Version == ODataVersion.V1 || r.Version == ODataVersion.V2))
                  || (version == ODataVersion.V3_V4 && (r.Version == ODataVersion.V3 || r.Version == ODataVersion.V4))
                  || (r.Version == ODataVersion.V3_V4 && (version == ODataVersion.V3 || version == ODataVersion.V4))
                  || (version == ODataVersion.V4 && (r.Version == ODataVersion.V4 || r.Version == ODataVersion.V3_V4))
                  || (r.Version == ODataVersion.V1_V2_V3 && (version == ODataVersion.V1 || version == ODataVersion.V2 || version == ODataVersion.V3 || version == ODataVersion.V1_V2))
                  || (r.Version == ODataVersion.V_All && r.Version != ODataVersion.UNKNOWN)
                  select r;
        }

        /// <summary>
        /// Filters rules based on whether metadata document is available.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="hasMetadata">Flag of metadata document availability</param>
        /// <returns>The subset of input rules that meet the condition of metadata document availability</returns>
        private static IEnumerable<Rule> SelectRulesByMetadata(this IEnumerable<Rule> rules, bool hasMetadata)
        {
            return from r in rules
                   where !r.RequireMetadata.HasValue || r.RequireMetadata.Value == hasMetadata || (r.RequireMetadata.HasValue && r.RequireMetadata.Value == false)
                   select r;
        }

        /// <summary>
        /// Filters rules based on whether service document is available.
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="hasServiceDocument">Flag of service document availability</param>
        /// <returns>The subset of input rules that meet the condition of service document availability</returns>
        private static IEnumerable<Rule> SelectRulesByServiceDocument(this IEnumerable<Rule> rules, bool hasServiceDocument)
        {
            return from r in rules
                   where !r.RequireServiceDocument.HasValue || r.RequireServiceDocument.Value == hasServiceDocument
                   select r;
        }

        /// <summary>
        /// Filters rules based on offline context or live context
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="isOfflineContext">Flag of context being offline or live</param>
        /// <returns>The subset of input rules that applies to the context</returns>
        private static IEnumerable<Rule> SelectRulesByOfflineFlag(this IEnumerable<Rule> rules, bool isOfflineContext)
        {
            if (isOfflineContext)
            {
                return from r in rules
                       where !r.Offline.HasValue || r.Offline.Value == true 
                       select r;
            }
            else
            {
                return rules;
            }
        }

        /// <summary>
        /// Filters rules based on odata metadata type
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="ODataMetadataType">OData metadata type</param>
        /// <returns>The subset of input rules that applies to the context</returns>
        private static IEnumerable<Rule> SelectRulesByMetadataFlag(this IEnumerable<Rule> rules, ODataMetadataType odataMetadataType)
        {
            return from r in rules
                   where !r.OdataMetadataType.HasValue || r.OdataMetadataType.Value == odataMetadataType
                   select r;
        }

        /// <summary>
        /// Filters rules based on resource type
        /// </summary>
        /// <param name="rules">The collection of input rules</param>
        /// <param name="resourceType">resource type</param>
        /// <returns>The subset of input rules that applies to the context</returns>
        private static IEnumerable<Rule> SelectRulesByConformanceType(this IEnumerable<Rule> rules, ConformanceServiceType resourceType, ConformanceLevelType[] levelTypes = null)
        {
            rules = from r in rules
                   where !r.ResourceType.HasValue || r.ResourceType.Value == resourceType
                   select r;

            if (null != levelTypes && levelTypes.Count() > 0)
            {
                rules = from r in rules
                       where !r.LevelType.HasValue || levelTypes.Contains(r.LevelType.Value)
                       select r;
            }

            return rules;
        }
    }
}
