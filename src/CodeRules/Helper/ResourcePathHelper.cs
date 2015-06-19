// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of URI resporce path.
    /// </summary>
    public class ResourcePathHelper
    {
        /// <summary>
        /// Separator of query part in URI string.
        /// </summary>
        private const string QuerySeparator = "?";

        /// <summary>
        /// Separator between query options.
        /// </summary>
        private const string OptionSeparator = "&";

        /// <summary>
        /// Separator of name and value within a query option.
        /// </summary>
        private const string NameValueSeparator = "=";

        /// <summary>
        /// Separator bewteen key-value pairs in combined key.
        /// </summary>
        private const char KeyValuePairSeparator = ',';

        /// <summary>
        /// Separator between key and value within single key-value pair in combined key.
        /// </summary>
        private const char KeyValueSeparator = '=';

        /// <summary>
        /// Separator between values of combined value only literal.
        /// </summary>
        private const string ValueSeparator = ",";

        /// <summary>
        /// String of URI exclusding queries.
        /// </summary>
        private string basePath;

        /// <summary>
        /// Collection of query key-value pairs.
        /// </summary>
        private Dictionary<string, string> queries;

        /// <summary>
        /// Creates an instance of ResourcePathHelper from a Uri object
        /// </summary>
        /// <param name="uri">The input Uri object</param>
        public ResourcePathHelper(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.basePath = uri.GetLeftPart(UriPartial.Path);
            this.queries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var collection = HttpUtility.ParseQueryString(uri.Query);
            foreach (var key in collection.AllKeys)
            {
                this.queries.Add(key, collection[key]);
            }
        }

        /// <summary>
        /// Gets the product of URI string.
        /// </summary>
        public string Product
        {
            get
            {
                StringBuilder sb = new StringBuilder(this.basePath);
                if (this.queries.Any())
                {
                    var queryStrings = from q in this.queries
                                       select q.Key + ResourcePathHelper.NameValueSeparator + q.Value;
                    var queryOptions = string.Join(ResourcePathHelper.OptionSeparator, queryStrings.ToArray());
                    sb.AppendFormat("{0}{1}", ResourcePathHelper.QuerySeparator, queryOptions);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the combined value literal from the combined key.
        /// </summary>
        /// <param name="combinedKey">The key combination</param>
        /// <returns>The value combination</returns>
        public static string GetValuesOfKey(string combinedKey)
        {
            if (string.IsNullOrEmpty(combinedKey))
            {
                return null;
            }

            string[] keyValuePairs = combinedKey.Split(ResourcePathHelper.KeyValuePairSeparator);
            List<string> values = new List<string>();

            foreach (var pair in keyValuePairs)
            {
                string[] items = pair.Split(ResourcePathHelper.KeyValueSeparator);

                switch (items.Length)
                {
                    case 1:
                        values.Add(items[0]);
                        break;
                    case 2:
                        values.Add(items[1]);
                        break;
                }
            }

            return string.Join(ResourcePathHelper.ValueSeparator, values.ToArray());
        }

        /// <summary>
        /// Adds a query option.
        /// </summary>
        /// <param name="key">The key of query option to be added</param>
        /// <param name="value">The value of query option to be added</param>
        public void AddQueryOption(string key, string value)
        {
            this.queries.Add(key, value);
        }

        /// <summary>
        /// Removes a query string having the specified query key. 
        /// </summary>
        /// <param name="key">The key of query option to be removed</param>
        public void RemoveQueryOption(string key)
        {
            this.queries.Remove(key);
        }

        /// <summary>
        /// Gets the value of query option having the specified key. 
        /// </summary>
        /// <param name="key">The key of query option</param>
        /// <returns>literal of query option value</returns>
        public string GetQueryValue(string key)
        {
            string result = null;
            this.queries.TryGetValue(key, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the target Uri of context has only one segment after the base of service document resourse 
        /// </summary>
        /// <param name="context">The context object</param>
        /// <param name="segment">The output parameter of the only segment</param>
        /// <returns>Returns true if the target is one segemnt after the base uri; false otherwise</returns>
        public static bool IsOneSegmentPath(ServiceContext context, out string segment)
        {
            bool result = false;
            segment = string.Empty;

            if (context.ServiceBaseUri != null)
            {
                string pathRelative = context.DestinationBasePath.Substring(context.ServiceBaseUri.AbsoluteUri.Length);
                segment = pathRelative.Trim('/');
                if (!string.IsNullOrEmpty(pathRelative) && !pathRelative.Equals(Constants.Metadata, StringComparison.Ordinal))
                {
                    result = pathRelative.IndexOf('/') < 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets collection of path segments of expansion definition
        /// </summary>
        /// <param name="expandValue">The expansion definion of expand query option</param>
        /// <returns>The collection of path segments</returns>
        public static IEnumerable<string[]> GetBranchedSegments(string expandValue)
        {
            List<string[]> result = new List<string[]>();

            if (!string.IsNullOrEmpty(expandValue))
            {
                string[] branches = expandValue.Split(',');
                foreach (var branch in branches)
                {
                    var br = branch.Trim().Trim('/');
                    string[] paths = br.Split('/');
                    result.Add(paths);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the complex type definition node with the specific name from the metadata document
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <param name="meta">The metadata document</param>
        /// <returns>The XML node of the named complex type if one is found; null otherwise</returns>
        public static XElement GetComplexType(string typeName, XElement meta)
        {
            string fmtXPathToComplexType = "//*[local-name()='Schema' and @Namespace='{0}']/*[local-name()='ComplexType' and @Name='{1}']";
            string xpath = string.Format(fmtXPathToComplexType, GetNamespaceName(typeName), GetBaseName(typeName));
            var node = meta.XPathSelectElement(xpath);
            return node;
        }

        /// <summary>
        /// Gets segments of URI relative path from ServiceContext instance
        /// </summary>
        /// <param name="context">The ServiceContext instance</param>
        /// <returns>The segments of relative path</returns>
        public static IEnumerable<string> GetPathSegments(ServiceContext context)
        {
            var path = context.DestinationBasePath.Substring(context.ServiceBaseUri.AbsoluteUri.Length);
            return ResourcePathHelper.GetPathSegments(path);
        }

        /// <summary>
        /// Gets segments from URI relative path
        /// </summary>
        /// <param name="path">The path in URI relative format</param>
        /// <returns>The segments of the path</returns>
        public static IEnumerable<string> GetPathSegments(string path)
        {
            path = path.Trim('/');
            return path.Split('/');
        }

        /// <summary>
        /// Gets Namespace name part of a type name
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <returns>The namespoace name; empty if type name is not a fully-qualified one</returns>
        public static string GetNamespaceName(string typeName)
        {
            int indexOfLastDot = typeName.LastIndexOf('.');
            return indexOfLastDot <= 0 ? "" : typeName.Substring(0, indexOfLastDot);
        }

        /// <summary>
        /// Gets the base name part of a type name
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <returns>The base name</returns>
        public static string GetBaseName(string typeName)
        {
            int indexOfLastDot = typeName.LastIndexOf('.');
            return indexOfLastDot <= 0 ? typeName : typeName.Substring(indexOfLastDot + 1);
        }

        /// <summary>
        /// Gets segment core from a path segment input
        /// </summary>
        /// <param name="segment">The segment input</param>
        /// <param name="inParenthesis">Output parameter of segment parameters enclosed by parenthesis</param>
        /// <returns>The segment core before the parenthesis</returns>
        public static string ParseSegment(string segment, out string inParenthesis)
        {
            string result = segment;
            inParenthesis = null;

            if (!string.IsNullOrEmpty(segment))
            {
                int posLP = segment.IndexOf('(');
                if (posLP > 0)
                {
                    int posRP = segment.LastIndexOf(')');
                    if (posRP > posLP)
                    {
                        result = segment.Substring(0, posLP);
                        inParenthesis = segment.Substring(posLP + 1, posRP - posLP - 1).Trim();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets URI object from resource path and base OData service root
        /// </summary>
        /// <param name="path">The resource path</param>
        /// <param name="uriBase">The OData service root Uri</param>
        /// <returns>The URI object generated; or null if none valid one is possible</returns>
        public static Uri GetODataResourceUri(string path, Uri uriBase)
        {
            Uri result;
            if (Uri.TryCreate(path, UriKind.Absolute, out result))
            {
                return result;
            }
            else if (Uri.TryCreate(uriBase, path, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the dataservice namespace from payload literal
        /// </summary>
        /// <param name="payload">The payload literal</param>
        /// <returns>The dataservice namespace defined in payload; or the implicit dataservie namespace</returns>
        public static string GetDataServiceNamespace(XElement payload)
        {
            var ps = payload.XPathSelectElement("//m:properties/*", ODataNamespaceManager.Instance);
            if (ps != null && !string.IsNullOrEmpty(ps.Name.NamespaceName))
            {
                return ps.Name.NamespaceName;
            }
            else
            {
                // the default implcit dataservice namespace as defined in [MS-OData] spec
                return "http://schemas.microsoft.com/ado/2007/08/dataservices";
            }
        }

        /// <summary>
        /// Gets the short name of the entity for the context
        /// </summary>
        /// <param name="context">The context object which directly or indirectly points to an entry</param>
        /// <returns>The short name of entity</returns>
        public static string GetEntityName(ServiceContext context)
        {
            string entityName = context.EntityTypeShortName;
            if (string.IsNullOrEmpty(entityName))
            {
                var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
                var segments = ResourcePathHelper.GetPathSegments(context);
                var segsToEntity = GetEntryUriSegments(segments.Take(segments.Count() - 1), edmxHelper);
                UriType uriType;
                var target = edmxHelper.GetTargetType(segsToEntity, out uriType);
                entityName = ((EntityType)target).Name;
            }
            return entityName;
        }


        /// <summary>
        /// Gets the segments pointing to an entry based
        /// </summary>
        /// <param name="segments">The segments pointing to derived resources of the entry</param>
        /// <param name="edmxHelper">The EdmxHelper instance</param>
        /// <returns>The segments pointing to the entry</returns>
        public static IEnumerable<string> GetEntryUriSegments(IEnumerable<string> segments, EdmxHelper edmxHelper)
        {
            if (!segments.Any())
            {
                return segments;
            }

            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);
            if (uriType == UriType.URI2)
            {
                return segments;
            }

            return GetEntryUriSegments(segments.Take(segments.Count() - 1), edmxHelper);
        }

        /// <summary>
        /// Gets the concurrency token property of the specified entity as defined in the metadata document
        /// </summary>
        /// <param name="meta">The metadata document</param>
        /// <param name="entityName">The short name of the entity</param>
        /// <returns>The concurrency property; or null if none is defined</returns>
        public static string GetConcurrencyProperty(XElement meta, string entityName)
        {
            string result = null;

            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name and @ConcurrencyMode='Fixed']",
               entityName);
            var concurrencyProperty = meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            if (concurrencyProperty != null)
            {
                result = concurrencyProperty.GetAttributeValue("Name");
            }

            return result;
        }

        /// <summary>
        /// Gets the major part of header value
        /// </summary>
        /// <param name="value">The header value</param>
        /// <returns>The major part of value</returns>
        public static string GetMajorHeaderValue(string value)
        {
            string result = null;
            string[] parts = value.Split(';');
            if (parts.Length >= 1)
            {
                result = parts[0];
            }
            return result;
        }

        /// <summary>
        /// Checks whether the entity type definition has any property mapping
        /// </summary>
        /// <param name="meta">The metadata document</param>
        /// <param name="entityType">The entity type</param>
        /// <returns>flag of proerty being mapped</returns>
        public static bool HasPropertyMapping(XElement meta, string entityType)
        {
            if (meta == null)
            {
                throw new ArgumentNullException("meta");
            }

            if (string.IsNullOrEmpty(entityType))
            {
                throw new ArgumentException("parameter should not be null or empty", "entityType");
            }

            const string tmplXPath2EntityType = @"//*[local-name()='EntityType' and @Name = '{0}']";
            const string xPath2Property = @"./*[local-name()='Property' and @m:FC_KeepInContent='false']";

            string typeShortName = ResourcePathHelper.GetBaseName(entityType);

            string xPath2EntityType = string.Format(tmplXPath2EntityType, typeShortName);
            var nodeEntityType = meta.XPathSelectElement(xPath2EntityType, ODataNamespaceManager.Instance);
            var nodeProperty = nodeEntityType.XPathSelectElement(xPath2Property, ODataNamespaceManager.Instance);
            if (nodeProperty != null)
            {
                return true;
            }
            else
            {
                string baseType = nodeEntityType.GetAttributeValue("BaseType");
                if (baseType == null)
                {
                    return false;
                }
                else
                {
                    return HasPropertyMapping(meta, baseType);
                }
            }
        }

        /// <summary>
        /// Checks whether a query string has the specied query option
        /// </summary>
        /// <param name="query">The query string</param>
        /// <param name="option">The query option name</param>
        /// <param name="value">Output parameter of found query option value</param>
        /// <returns>true if the named option is found; false otherwise</returns>
        public static bool HasQueryOption(string query, string option, out string value)
        {
            bool result = false;
            var qs = HttpUtility.ParseQueryString(query);
            result = qs.AllKeys.Contains(option);
            value = result ? qs[option] : null;
            return result;
        }
   }
}
