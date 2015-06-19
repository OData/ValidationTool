// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2004_Entry : CommonCore2004
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
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2004_Feed : CommonCore2004
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
    /// Class of code rule applying to error payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2004_Error : CommonCore2004
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Error;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to other payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2004_Other : CommonCore2004
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Other;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to null payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2004_None : CommonCore2004
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.None;
            }
        }
    }

    /// <summary>
    /// Abstract base class of code rule to check semantic expectation. 
    /// </summary>
    public abstract class CommonCore2004 : ExtensionRule
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
                return "Common.Core.2004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "URI1: If the Entity Data Model associated with the data service does not include an EntitySet (or FunctionImport) with the name specified, " +
                    "then this URI (and any URI created by appending additional path segments) MUST be treated as identifying a non-existent resource, " +
                    "as s described in Message Processing Events and Sequencing Rules (section 3.2.5)";
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
        /// Gets the flag whether it requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
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

            bool passed = true;
            info = null;

            // get the first segment (entity set) after the service root URI
            char[] delimiters = new char[] { '/', '(' };
            string path = context.DestinationBasePath.Substring(context.ServiceBaseUri.AbsoluteUri.Length);
            string firstSegment = path.IndexOfAny(delimiters) >= 0 ? path.Substring(0, path.IndexOfAny(delimiters)) : path;
            if (!string.IsNullOrEmpty(firstSegment) && !firstSegment.Equals(Constants.Metadata, StringComparison.Ordinal))
            {
                string title = string.Empty;

                if (context.PayloadFormat == RuleEngine.PayloadFormat.Atom || context.PayloadFormat == RuleEngine.PayloadFormat.Xml)
                {
                    string xpathOfSvcDoc = string.Format("//*[local-name()='workspace']/*[local-name()= 'function-import' or 'collection' and @href='{0}']/*[local-name()='title']", firstSegment);
                    var titleEle = XElement.Parse(context.ServiceDocument).XPathSelectElement(xpathOfSvcDoc);
                    if (titleEle != null)
                    {
                        title = titleEle.Value;
                    }
                }
                else if (context.PayloadFormat == RuleEngine.PayloadFormat.JsonLight || context.PayloadFormat == RuleEngine.PayloadFormat.Json)
                {
                    JObject jo;
                    context.ServiceDocument.TryToJObject(out jo);
                   
                    if (jo[Constants.Value] != null)
                    {
                        IEnumerable<string> names = from a in (JArray)jo[Constants.Value]
                                                    where a[Constants.Url].Value<string>().Equals(firstSegment) 
                                                    && (a[Constants.Kind] == null || a[Constants.Kind].Value<string>().StripOffDoubleQuotes().Equals(Constants.EntitySet) || a[Constants.Kind].Value<string>().StripOffDoubleQuotes().Equals(Constants.FunctionImport))
                                                    select a[Constants.Name].Value<string>();

                        if (names.Any())
                        {
                            title = names.First();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(title))
                {
                    string xpath = string.Format("//*[local-name()='EntitySet' and @Name='{0}'] | //*[local-name()='FunctionImport' and @Name='{0}']", title);
                    bool definedInMeta = XElement.Parse(context.MetadataDocument).XPathSelectElement(xpath) != null;

                    if (!definedInMeta)
                    {
                        if (!(context.HttpStatusCode.Value == System.Net.HttpStatusCode.NotFound || context.HttpStatusCode.Value == System.Net.HttpStatusCode.Gone))
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo("unexpected status code " + context.HttpStatusCode.Value.ToString(), context.Destination, context.HttpStatusCode.Value.ToString());
                        }
                        else if (context.PayloadType != RuleEngine.PayloadType.Error)
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo("error payload expected", context.Destination, context.PayloadType.ToString());
                        }
                    }
                }
            }

            return passed;
        }
    }
}
