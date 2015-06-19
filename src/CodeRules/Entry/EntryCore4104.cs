// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4104
    /// </summary>
    [Export(typeof(ExtensionRule))]
    class EntryCore4104 : ExtensionRule
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
                return "Entry.Core.4104";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The name of expanded navigation is the name of the navigation property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.3";
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
        /// Gets the IsOfflineContext property to which the rule applies.
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

            List<string> expandVals = this.GetQueryOptionValsFromUrl(context.Destination.ToString(), @"expand");
            List<string> selectVals = this.GetQueryOptionValsFromUrl(context.Destination.ToString(), @"select");

            if (expandVals.Count != 0)
            {
                XElement metadata = XElement.Parse(context.MetadataDocument);

                // Use the XPath query language to access the metadata document and get the node which will be used.
                string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", context.EntityTypeShortName);
                IEnumerable<XElement> props = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                JObject entry;
                context.ResponsePayload.TryToJObject(out entry);

                if (entry != null && entry.Type == JTokenType.Object)
                {
                    // Get all the properties in current entry.
                    var jProps = entry.Children();

                    int counter = 0;
                    if (selectVals.Count == 0)
                    {
                        foreach (JProperty jProp in jProps)
                        {
                            if (expandVals.Contains(jProp.Name))
                            {
                                foreach (var prop in props)
                                {
                                    if (jProp.Name == prop.Attribute("Name").Value)
                                    {
                                        counter++;
                                    }
                                }
                            }
                        }

                        if (counter == expandVals.Count)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        }
                    }
                    else
                    {
                        string[] names = new string[]
                        {
                            @"expand", @"select",
                        };

                        HashSet<string> expandValList = this.GetSameQueryOptionValsFromUrl(context.Destination.ToString(), names);

                        foreach (JProperty jProp in jProps)
                        {
                            if (expandValList.Contains(jProp.Name))
                            {
                                foreach (var prop in props)
                                {
                                    if (jProp.Name == prop.Attribute("Name").Value)
                                    {
                                        counter++;
                                    }
                                }
                            }
                        }

                        if (counter == expandValList.Count)
                        {
                            passed = true;
                        }
                        else 
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        }
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Get a query option value from the input url.
        /// </summary>
        /// <param name="url">Indicate the input url.</param>
        /// <param name="name">Indicate the query option name.</param>
        /// <returns>Returns the values of a query option.</returns>
        private List<string> GetQueryOptionValsFromUrl(string url, string name)
        {
            if(url == null || url == string.Empty || name == null || name == string.Empty)
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
        private HashSet<string> GetSameQueryOptionValsFromUrl(string url, string[] name)
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
                List<string> list = this.GetQueryOptionValsFromUrl(url, name[i]);
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
    }
}
