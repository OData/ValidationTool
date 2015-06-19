// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Linq;
    #endregion

    /// <summary>
    /// Helper class of Simple OData URI Analysis
    /// </summary>
    public static class ODataUriAnalyzer
    {
        /// <summary>
        /// Gets the target MetadataItem for sequence of uri path segments
        /// </summary>
        /// <param name="edmxHelper">The EdmxHelper class object</param>
        /// <param name="pathSegment">The sequence of uri path segment</param>
        /// <param name="uriType">Output parameter of UriType</param>
        /// <returns>The target MetadataItem for the uri path segments; or null if a meaningful one cannot be derived</returns>
        public static MetadataItem GetTargetType(this EdmxHelper edmxHelper, IEnumerable<string> pathSegment, out UriType uriType)
        {
            EntityContainer currContainer;
            string leadSegment = pathSegment.First();
            var containers = from c in edmxHelper.containers where c.Name.Equals(leadSegment, System.StringComparison.Ordinal) select c;
            if (!containers.Any())
            {
                // get the default container
                currContainer = edmxHelper.containerDefault;
            }
            else
            {
                currContainer = containers.First();
                pathSegment = pathSegment.Skip(1);
            }

            UriType uType = UriType.URIUNKNOWN;
            var result = GetTargetType(currContainer, pathSegment, ref uType);
            uriType = uType;
            return result;
        }

        /// <summary>
        /// Gets collection of RelationshipEndMember as navigation targets along the navigation path from the EntityType object 
        /// </summary>
        /// <param name="entityType">The EntityType instance to start with</param>
        /// <param name="navigation">The navigation path</param>
        /// <returns>Collection of navigation targets</returns>
        public static IEnumerable<RelationshipEndMember> GetNavigationStack(EntityType entityType, IEnumerable<string> navigation)
        {
            var result = new List<RelationshipEndMember>();

            EntityType currEntityType = entityType;
            UriType uriType = UriType.URI2;
            foreach(var nav in navigation)
            {
                ODataUriItem curr = new ODataUriItem(currEntityType, uriType);
                var next = curr.GetItem(nav);
                RelationshipEndMember navRole = (RelationshipEndMember)next.Item;
                result.Add(navRole);
                currEntityType = navRole.GetEntityType();
            }

            return result;
        }

        /// <summary>
        /// Get a query option value from the input url.
        /// </summary>
        /// <param name="url">Indicate the input url.</param>
        /// <param name="name">Indicate the query option name.</param>
        /// <returns>Returns the values of a query option.</returns>
        public static List<string> GetQueryOptionValsFromUrl(string url, string name)
        {
            if (url == null || url == string.Empty || name == null || name == string.Empty)
            {
                return null;
            }

            List<string> vals = new List<string>();

            string Exp = string.Empty;

            char[] splitChars = new char[]
            {
                '$', '&', ';',
            };

            foreach (string str in url.Split(splitChars))
            {
                if (str.Contains(name))
                {
                    Exp = str;
                    break;
                }
            }

            string[] splitedStrs = Exp.Split('=');

            if (splitedStrs.Length > 1)
            {
                foreach (string val in splitedStrs[1].Split(','))
                {
                    if (val != string.Empty)
                    {
                        vals.Add(val);
                    }
                }
            }

            return vals;
        }

        /// <summary>
        /// Merge the same value in the query options.
        /// </summary>
        /// <param name="url">Indicate the input url.</param>
        /// <param name="name">Indicate the query option name.</param>
        /// <returns>Returns a string list which contains all the same elements.</returns>
        public static HashSet<string> GetSameQueryOptionValsFromUrl(string url, string[] name)
        {
            if (url == null || url == string.Empty || name.Length < 2)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string>();
            HashSet<string> delList = new HashSet<string>();
            List<List<string>> lists = new List<List<string>>();

            for (int i = 0; i < name.Length; i++)
            {
                List<string> list = GetQueryOptionValsFromUrl(url, name[i]);
                lists.Add(list);
            }

            foreach (List<string> l in lists)
            {
                foreach (string s in l)
                {
                    result.Add(s);
                }
            }

            foreach (List<string> l in lists)
            {
                foreach (string s in result)
                {
                    if (!l.Contains(s))
                    {
                        delList.Add(s);
                    }
                }
            }

            foreach (string s in delList)
            {
                result.Remove(s);
            }

            return result;
        }

        /// <summary>
        /// Get the relative string from URL.
        /// </summary>
        /// <param name="url">Store the Url as string parameter.</param>
        /// <returns>Returns the relative string.</returns>
        public static string GetRelativeStringFromUrl(string url)
        {
            if (null == url)
            {
                return null;
            }

            if (url.Contains(@"#"))
            {
                string temp = url.Remove(0, url.IndexOf("#") + 1);

                if (temp.Contains(@"/"))
                {
                    int index = temp.IndexOf("/");
                    return temp.Remove(index, temp.Length - index);
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets MetadataItem object for URI relative path under the specified EntityContainer instance 
        /// </summary>
        /// <param name="container">The EntityContainer instance</param>
        /// <param name="pathSegment">The URI relative path</param>
        /// <param name="uriType">Output parameter of UriType value</param>
        /// <returns>The MetadataItem instance if one is found; null otherwise</returns>
        private static MetadataItem GetTargetType(EntityContainer container, IEnumerable<string> pathSegment, ref UriType uriType)
        {
            ODataUriItem curr = new ODataUriItem(container, UriType.URI_Container);
            foreach (var segment in pathSegment)
            {
                // to normalize first segment
                string segmentKey;
                string segmentCore = ResourcePathHelper.ParseSegment(segment, out segmentKey);
                ODataUriItem subItem = curr.GetItem(segmentCore);
                if (subItem == null)
                {
                    uriType = UriType.URIUNKNOWN;
                    return null;
                }

                if (subItem.uriType == UriType.URI1 && !string.IsNullOrEmpty(segmentKey))
                {
                    subItem = new ODataUriItem(((EntitySet)subItem.Item).ElementType, UriType.URI2); 
                }

                curr = subItem;
            }

            uriType = curr.uriType;
            return curr.Item;
        }
    }
}
