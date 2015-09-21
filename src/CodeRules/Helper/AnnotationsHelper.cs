// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.XPath;
    using System.Xml.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The SupportiveFeatureHelper class.
    /// </summary>
    public static class AnnotationsHelper
    {
        /// <summary>
        /// Get the entity set names which support top query.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="vocDocs">The vocabularies documents.</param>
        /// <returns>Returns a list contains all entity-sets which support $top system query options.</returns>
        public static List<string> SelectEntitySetSupportTop(string metadata, List<string> vocDocs)
        {
            List<string> supportTopEntitySetNames = new List<string>();
            List<string> allEntitySetNames = MetadataHelper.GetAllEntitySetNames(metadata);

            foreach (string entitySet in allEntitySetNames)
            {
                bool? isSupportTop = entitySet.IsEntitySetSupportTopQuery(metadata, vocDocs);

                if (isSupportTop.HasValue && isSupportTop.Value == true)
                {
                    supportTopEntitySetNames.Add(entitySet);
                }
            }

            return supportTopEntitySetNames;
        }

        /// <summary>
        /// Get the entity set names which support skip query.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="vocDocs">The vocabularies documents.</param>
        /// <returns>Returns a list contains all entity-sets which support $skip system query options.</returns>
        public static List<string> SelectEntitySetSupportSkip(string metadata, List<string> vocDocs)
        {
            List<string> supportSkipEntitySetNames = new List<string>();
            List<string> allEntitySetNames = MetadataHelper.GetAllEntitySetNames(metadata);

            foreach (string entitySet in allEntitySetNames)
            {
                bool? isSupportSkip = entitySet.IsEntitySetSupportTopQuery(metadata, vocDocs);

                if (isSupportSkip.HasValue && isSupportSkip.Value == true)
                {
                    supportSkipEntitySetNames.Add(entitySet);
                }
            }

            return supportSkipEntitySetNames;
        }

        /// <summary>
        /// Get the entity-set which support batch operation.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabularies documents.</param>
        /// <returns>Returns all the entity-sets which support batch operation.</returns>
        public static List<string> SelectEntitySetSupportBatch(string metadataDoc, List<string> vocDocs)
        {
            string xPath = "//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
            XElement metadata = XElement.Parse(metadataDoc);
            IEnumerable<XElement> eles = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            List<string> entitySetNames = new List<string>();

            foreach (var ele in eles)
            {
                if (null != ele.Parent &&
                    "EntityContainer" == ele.Parent.Name.LocalName &&
                    true == ele.Parent.GetAttributeValue("Name").IsServiceSupportBatchOperation(metadataDoc, vocDocs))
                {
                    if (null != ele.Attribute("Name"))
                    {
                        entitySetNames.Add(ele.GetAttributeValue("Name"));
                    }
                }
            }

            return entitySetNames;
        }

        #region The term restrictions related helper methods.
        /// <summary>
        /// Gets count restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the count restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the delete restrictions or not.</returns>
        public static bool GetCountRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetCountRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets filter restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the filter restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the filter restrictions or not.</returns>
        public static bool GetFilterRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetFilterRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets sort restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the sort restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>whether find any entity-sets match the sort restrictions or not.</returns>
        public static bool GetSortRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetSortRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets expand restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the expand restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>whether find any entity-sets match the expand restrictions or not.</returns>
        public static bool GetExpandRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetExpandRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets insert restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the insert restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>whether find any entity-sets match the insert restrictions or not.</returns>
        public static bool GetInsertRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetInsertRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets update restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the update restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>whether find any entity-sets match the update restrictions or not.</returns>
        public static bool GetUpdateRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetUpdateRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets delete restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the delete restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the delete restrictions or not.</returns>
        public static bool GetDeleteRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (AnnotationsHelper.GetDeleteRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Gets all restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="funcs">All the related restrictions functions.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the specified restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the specified restrictions or not.</returns>
        public static bool GetRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>> funcs,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload() || null == funcs || !funcs.Any())
            {
                return false;
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return false;
            }

            foreach (var f in feeds)
            {
                List<NormalProperty> normalProps = new List<NormalProperty>();
                List<NavigProperty> navigationProps = new List<NavigProperty>();
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();

                if (string.IsNullOrEmpty(entityTypeShortName))
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                bool flag = true;

                foreach (var func in funcs)
                {
                    if (!func(f, metadataDoc, vocCapabilitiesDoc, props, nprops))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    result.Add(f, new Tuple<List<NormalProperty>, List<NavigProperty>>(normalProps, navigationProps));
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Verify the entity-set whether supports system query options $top or not.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the specified restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the specified restrictions or not.</returns>
        public static bool IsSupportSysQueryTop(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            foreach (var r in restrictions)
            {
                if (true == r.Key.IsEntitySetSupportTopQuery(metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                {
                    result.Add(r.Key, r.Value);
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Verify the entity-set whether supports system query options $skip or not.
        /// </summary>
        /// <param name="metadataDoc"></param>
        /// <param name="vocCapabilitiesDoc"></param>
        /// <param name="restrictions">All the appropriate entity-sets which match the specified restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the specified restrictions or not.</returns>
        public static bool IsSupportSysQuerySkip(
            string metadataDoc,
            string vocCapabilitiesDoc,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            foreach (var r in restrictions)
            {
                if (true == r.Key.IsEntitySetSupportSkipQuery(metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                {
                    result.Add(r.Key, r.Value);
                }
            }

            restrictions = result;

            return result.Any() ? true : false;
        }
        #endregion

        #region Filter out unsuitable elements in restrictions.
        /// <summary>
        /// Filter out unsuitable normal properties.
        /// </summary>
        /// <param name="primitiveTypes">The primitive types' list.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the specified restrictions.
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the specified restrictions or not.</returns>
        private static bool IsSuitableProperty(
            List<string> primitiveTypes,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            if (null == primitiveTypes || !primitiveTypes.Any() || null == restrictions || !restrictions.Any())
            {
                return false;
            }

            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            foreach (var r in restrictions)
            {
                List<NormalProperty> props = new List<NormalProperty>();

                foreach (var p in r.Value.Item1)
                {
                    if (primitiveTypes.Contains(p.PropertyType))
                    {
                        props.Add(p);
                    }
                }

                result.Add(r.Key, new Tuple<List<NormalProperty>, List<NavigProperty>>(props, r.Value.Item2));
            }

            restrictions = result;

            return result.Any() ? true : false;
        }

        /// <summary>
        /// Filter out unsuitable navigation properties.
        /// </summary>
        /// <param name="navigationRoughType">The navigation rough type.</param>
        /// <param name="restrictions">All the appropriate entity-sets which match the specified restrictions.</param>
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether find any entity-sets match the specified restrictions or not.</returns>
        public static bool IsSuitableNavigationProperty(
            NavigationRoughType navigationRoughType,
            ref Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>> restrictions)
        {
            if (NavigationRoughType.None == navigationRoughType || null == restrictions || !restrictions.Any())
            {
                return false;
            }

            var result = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            foreach (var r in restrictions)
            {
                List<NavigProperty> nprops = new List<NavigProperty>();

                foreach (var np in r.Value.Item2)
                {
                    if (navigationRoughType == np.NavigationRoughType)
                    {
                        nprops.Add(np);
                    }
                }

                result.Add(r.Key, new Tuple<List<NormalProperty>, List<NavigProperty>>(r.Value.Item1, nprops));
            }

            restrictions = result;

            return result.Any() ? true : false;
        }
        #endregion

        /// <summary>
        /// Gets count restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports count restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetCountRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(vocCapabilitiesDoc) || !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetCountRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets filter restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports filter restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetFilterRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetFilterRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets filter restrictions without navigation property checking.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <returns>Returns an appropriate tuple element which supports filter restrictions.</returns>
        public static Tuple<string, List<NormalProperty>> GetFilterRestrictionsWithoutNavi(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>>(string.Empty, new List<NormalProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>>(string.Empty, new List<NormalProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                entitySet = f;
                normalProps.AddRange(props);
                break;
            }

            return new Tuple<string, List<NormalProperty>>(entitySet, normalProps);
        }

        /// <summary>
        /// Gets sort restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports sort restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetSortRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetSortRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets expand restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports expand restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetExpandRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySetName = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetExpandRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySetName = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySetName, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets insert restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports insert restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetInsertRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetInsertRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets update restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports update restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetUpdateRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetUpdateRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets delete restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports delete restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetDeleteRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetDeleteRestrictions(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets change tracking.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports delete restrictions.</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetChangeTracking(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (string.IsNullOrEmpty(metadataDoc) ||
                !metadataDoc.IsXmlPayload() ||
                string.IsNullOrEmpty(vocCapabilitiesDoc) ||
                !vocCapabilitiesDoc.IsXmlPayload())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var entityContainers = MetadataHelper.GetEntityContainerNames(metadataDoc);

            foreach (var container in entityContainers)
            {
                ChangeTrackingType? changeTracking = container.GetChangeTranking(metadataDoc, new List<string>() { vocCapabilitiesDoc });

                if (false == changeTracking.Value.Supported)
                {
                    return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
                }
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var func in methods)
                    {
                        if (true != func(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                if (AnnotationsHelper.GetChangeTracking(f, metadataDoc, vocCapabilitiesDoc, props, nprops) && props.Any() && nprops.Any())
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);

                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets all restrictions.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="funcs">All the related restrictions functions.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports all the specified restrictions. 
        /// (Note: The tuple element contains entity-set name, a list of normal properties, a list of navigation properties.)</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetRestrictions(
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>> funcs,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            if (!metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == funcs || !funcs.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            var feeds = MetadataHelper.GetFeeds(metadataDoc);

            if (!feeds.Any())
            {
                return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            foreach (var f in feeds)
            {
                string entityTypeShortName = f.MapEntitySetNameToEntityTypeShortName();
                bool flag = true;

                if (null != methods && methods.Any())
                {
                    foreach (var method in methods)
                    {
                        if (true != method(f, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
                {
                    continue;
                }

                var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
                var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

                if (null == props || !props.Any() || null == nprops || !nprops.Any())
                {
                    continue;
                }

                if (null != primitiveTypes && primitiveTypes.Any() &&
                    !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
                {
                    continue;
                }

                if (NavigationRoughType.None != navigationType &&
                    !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
                {
                    continue;
                }

                flag = true;

                foreach (var func in funcs)
                {
                    if (!func(f, metadataDoc, vocCapabilitiesDoc, props, nprops))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    entitySet = f;
                    normalProps.AddRange(props);
                    navigationProps.AddRange(nprops);
                    break;
                }
            }

            return new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);
        }

        /// <summary>
        /// Gets all restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="funcs">All the related restrictions functions.</param>
        /// <param name="primitiveTypes">The expected primitive types' list.</param>
        /// <param name="navigationType">The expected navigation rough type.</param>
        /// <param name="methods">The delegate function to filter other term restrictions with the type 'Core.Tag'.</param>
        /// <returns>Returns an appropriate tuple element which supports all the specified restrictions. 
        /// (Note: The tuple element contains entity-set name, a list of normal properties, a list of navigation properties.)</returns>
        public static Tuple<string, List<NormalProperty>, List<NavigProperty>> GetRestrictions(
            this string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>> funcs,
            List<string> primitiveTypes = null,
            NavigationRoughType navigationType = NavigationRoughType.None,
            List<Func<string, string, List<string>, bool?>> methods = null)
        {
            var result = new Tuple<string, List<NormalProperty>, List<NavigProperty>>(string.Empty, new List<NormalProperty>(), new List<NavigProperty>());

            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == funcs || !funcs.Any())
            {
                return result;
            }

            string entitySet = string.Empty;
            List<NormalProperty> normalProps = new List<NormalProperty>();
            List<NavigProperty> navigationProps = new List<NavigProperty>();

            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            bool flag = true;

            if (null != methods && methods.Any())
            {
                foreach (var method in methods)
                {
                    if (true != method(entitySetName, metadataDoc, new List<string>() { vocCapabilitiesDoc }))
                    {
                        flag = false;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(entityTypeShortName) || !flag)
            {
                return result;
            }

            var props = MetadataHelper.GetNormalProperties(metadataDoc, entityTypeShortName).ToList();
            var nprops = MetadataHelper.GetNavigationProperties(metadataDoc, entityTypeShortName).ToList();

            if (null == props || !props.Any() || null == nprops || !nprops.Any())
            {
                return result;
            }

            if (null != primitiveTypes && primitiveTypes.Any() &&
                !AnnotationsHelper.IsSuitableProperty(props, primitiveTypes))
            {
                return result;
            }

            if (NavigationRoughType.None != navigationType &&
                !AnnotationsHelper.IsSuitableNavigationProperty(nprops, navigationType))
            {
                return result;
            }

            flag = true;

            foreach (var func in funcs)
            {
                if (!func(entitySetName, metadataDoc, vocCapabilitiesDoc, props, nprops))
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                entitySet = entitySetName;
                normalProps.AddRange(props);
                navigationProps.AddRange(nprops);
            }

            result = new Tuple<string, List<NormalProperty>, List<NavigProperty>>(entitySet, normalProps, navigationProps);

            return result;
        }

        /// <summary>
        /// Gets count restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports $count system query.</returns>
        public static bool GetCountRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            CountRestrictionsType? countRestrictions = entitySetName.GetCountRestrictions(metadataDoc, vocDocs);

            if (false == countRestrictions.Value.Countable)
            {
                return false;
            }
            else
            {
                if (null != countRestrictions.Value.NonCountableProperties &&
                    countRestrictions.Value.NonCountableProperties.Any())
                {
                    foreach (var p in countRestrictions.Value.NonCountableProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }

                if (null != countRestrictions.Value.NonCountableNavigationProperties &&
                    countRestrictions.Value.NonCountableNavigationProperties.Any())
                {
                    foreach (var p in countRestrictions.Value.NonCountableNavigationProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets filter restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports $filter system query.</returns>
        public static bool GetFilterRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            FilterRestrictionsType? filterRestrictions = entitySetName.GetFilterRestrictions(metadataDoc, vocDocs);

            if (false == filterRestrictions.Value.Filteralbe)
            {
                return false;
            }
            else
            {
                if (null != filterRestrictions.Value.NonFilterableProperties &&
                    filterRestrictions.Value.NonFilterableProperties.Any())
                {
                    foreach (var p in filterRestrictions.Value.NonFilterableProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            if (true == filterRestrictions.Value.RequiresFilter)
            {
                if (null != filterRestrictions.Value.RequiredProperties &&
                    filterRestrictions.Value.RequiredProperties.Any())
                {
                    foreach (var p in filterRestrictions.Value.RequiredProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets sort restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports $orderby system query.</returns>
        public static bool GetSortRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            SortRestrictionsType? sortRestrictions = entitySetName.GetSortRestrictions(metadataDoc, vocDocs);

            if (false == sortRestrictions.Value.Sortable)
            {
                return false;
            }
            else
            {
                if (null != sortRestrictions.Value.AscendingOnlyProperties &&
                    sortRestrictions.Value.AscendingOnlyProperties.Any())
                {
                    foreach (var p in sortRestrictions.Value.AscendingOnlyProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }

                if (null != sortRestrictions.Value.DescendingOnlyProperties &&
                    sortRestrictions.Value.DescendingOnlyProperties.Any())
                {
                    foreach (var p in sortRestrictions.Value.DescendingOnlyProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }

                if (null != sortRestrictions.Value.NonSortableProperties &&
                    sortRestrictions.Value.NonSortableProperties.Any())
                {
                    foreach (var p in sortRestrictions.Value.NonSortableProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets expand restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports $expand system query options.</returns>
        public static bool GetExpandRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            ExpandRestrictionsType? expandRestrictions = entitySetName.GetExpandRestrictions(metadataDoc, vocDocs);

            if (false == expandRestrictions.Value.Expandable)
            {
                return false;
            }
            else
            {
                if (null != expandRestrictions.Value.NonExpandableProperties &&
                    expandRestrictions.Value.NonExpandableProperties.Any())
                {
                    foreach (var p in expandRestrictions.Value.NonExpandableProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets insert restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports insert operation.</returns>
        public static bool GetInsertRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            InsertRestrictionsType? insertRestrictions = entitySetName.GetInsertRestrictions(metadataDoc, vocDocs);

            if (false == insertRestrictions.Value.Insertable)
            {
                return false;
            }
            else
            {
                if (null != insertRestrictions.Value.NonInsertableNavigationProperties &&
                    insertRestrictions.Value.NonInsertableNavigationProperties.Any())
                {
                    foreach (var p in insertRestrictions.Value.NonInsertableNavigationProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets update restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports update operation.</returns>
        public static bool GetUpdateRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            UpdateRestrictionsType? updateRestrictions = entitySetName.GetUpdateRestrictions(metadataDoc, vocDocs);

            if (false == updateRestrictions.Value.Updatable)
            {
                return false;
            }
            else
            {
                if (null != updateRestrictions.Value.NonUpdatableNavigationProperties &&
                    updateRestrictions.Value.NonUpdatableNavigationProperties.Any())
                {
                    foreach (var p in updateRestrictions.Value.NonUpdatableNavigationProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets delete restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name which is got from the metadata document.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input entity-set supports delete operation.</returns>
        public static bool GetDeleteRestrictions(
            string entitySetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(entitySetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            DeleteRestrictionsType? deleteRestrictions = entitySetName.GetDeleteRestrictions(metadataDoc, vocDocs);

            if (false == deleteRestrictions.Value.Deletable)
            {
                return false;
            }
            else
            {
                if (null != deleteRestrictions.Value.NonDeletableNavigationProperties &&
                    deleteRestrictions.Value.NonDeletableNavigationProperties.Any())
                {
                    foreach (var p in deleteRestrictions.Value.NonDeletableNavigationProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        /// <summary>
        /// Gets change tracking.
        /// </summary>
        /// <param name="targetName">The target name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocCapabilitiesDoc">The vocabulary-capabilities document.</param>
        /// <param name="normalProperties">The normal properties which was contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <param name="navigProperties">The navigation properties which is contained in the input entity-set. 
        /// (Note: This parameter is a reference parameter, but without 'ref' flag before its type.)</param>
        /// <returns>Returns whether the input target supports change tracking.</returns>
        public static bool GetChangeTracking(
            string targetName,
            string metadataDoc,
            string vocCapabilitiesDoc,
            List<NormalProperty> normalProperties,
            List<NavigProperty> navigProperties)
        {
            if (string.IsNullOrEmpty(targetName) || !metadataDoc.IsXmlPayload() || !vocCapabilitiesDoc.IsXmlPayload() || null == normalProperties || !normalProperties.Any() || null == navigProperties || !navigProperties.Any())
            {
                return false;
            }

            List<string> vocDocs = new List<string>() { vocCapabilitiesDoc };
            string entityTypeShortName = targetName.MapEntitySetNameToEntityTypeShortName();

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            ChangeTrackingType? changeTracking = targetName.GetChangeTranking(metadataDoc, vocDocs);

            if (false == changeTracking.Value.Supported)
            {
                return false;
            }
            else
            {
                if (null != changeTracking.Value.FilterableProperties &&
                    changeTracking.Value.FilterableProperties.Any())
                {
                    foreach (var p in changeTracking.Value.FilterableProperties)
                    {
                        var tempProps = new List<NormalProperty>();
                        tempProps.AddRange(normalProperties.Where(t => p == t.PropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                normalProperties.Remove(t);
                            }
                        }
                    }
                }

                if (null != changeTracking.Value.ExpandableProperties &&
                    changeTracking.Value.ExpandableProperties.Any())
                {
                    foreach (var p in changeTracking.Value.ExpandableProperties)
                    {
                        var tempProps = new List<NavigProperty>();
                        tempProps.AddRange(navigProperties.Where(t => p == t.NavigationPropertyName).Select(t => t));

                        if (null == tempProps || !tempProps.Any())
                        {
                            continue;
                        }
                        else
                        {
                            foreach (var t in tempProps)
                            {
                                navigProperties.Remove(t);
                            }
                        }
                    }
                }
            }

            return normalProperties.Any() && navigProperties.Any() ? true : false;
        }

        #region Private methods.
        /// <summary>
        /// Verify whether the properties' list contain specified primitive types.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="primitiveTypes">The primitive type.</param>
        /// <returns>Returns the boolean result.</returns>
        private static bool IsSuitableProperty(List<NormalProperty> properties, List<string> primitiveTypes)
        {
            bool flag = false;
            List<NormalProperty> props = new List<NormalProperty>();

            foreach (var p in properties)
            {
                if (primitiveTypes.Contains(p.PropertyType))
                {
                    flag = true;
                    props.Add(p);
                }
            }

            properties.Clear();
            properties.AddRange(props);

            return flag;
        }

        /// <summary>
        /// Verify whether the navigation properties' list contain specified navigation rough type.
        /// </summary>
        /// <param name="navigProperties">The navigation properties.</param>
        /// <param name="navigRoughType">The navigation rough type.</param>
        /// <returns>Returns the boolean result.</returns>
        private static bool IsSuitableNavigationProperty(List<NavigProperty> navigProperties, NavigationRoughType navigRoughType)
        {
            bool flag = false;
            List<NavigProperty> nprops = new List<NavigProperty>();

            foreach (var np in navigProperties)
            {
                if (navigRoughType == np.NavigationRoughType)
                {
                    flag = true;
                    nprops.Add(np);
                }
            }

            navigProperties.Clear();
            navigProperties.AddRange(nprops);

            return flag;
        }
        #endregion
    }
}
