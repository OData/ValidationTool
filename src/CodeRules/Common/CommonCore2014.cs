// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code rule when payload is an enrty
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2014_Entry : CommonCore2014
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Entry; }
        }
    }

    /// <summary>
    /// class of concrete code rule when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2014_Property : CommonCore2014
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Property; }
        }
    }

    /// <summary>
    /// class of concrete code rule when payload is a raw value
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2014_RawValue : CommonCore2014
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.RawValue; }
        }
    }

    /// <summary>
    /// Abstract base class of rule #253
    /// </summary>
    public abstract class CommonCore2014 : ExtensionRule
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
                return "Common.Core.2014";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"(ETag)The value of the header MUST represent the concurrency token for the entity that is identified by the request URI.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.4";
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
        /// Gets the flag of context being offline validation
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag of metadata document availability
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

            info = null;
            bool? passed = null;

            //to apply rule when payload has ETag header
            var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag");
            if (etagInHeader != null)
            {
                var valueInHeader = GetETagValueCore(etagInHeader.Trim());
                string entityName = ResourcePathHelper.GetEntityName(context);

                string concurrencyProperty = ResourcePathHelper.GetConcurrencyProperty(XElement.Parse(context.MetadataDocument), entityName);
                if (concurrencyProperty != null)
                {
                    string valueInEntry = null;

                    //to get the concurrency token for the entity
                    switch (context.PayloadFormat)
                    {
                        case RuleEngine.PayloadFormat.Atom:
                        case RuleEngine.PayloadFormat.Xml:
                            valueInEntry = GetProperty(XElement.Parse(context.ResponsePayload), concurrencyProperty);
                            if (valueInEntry == null)
                            {
                                valueInEntry = GetRawValueOfProperty(context, concurrencyProperty, context);
                            }
                            break;
                        case RuleEngine.PayloadFormat.Json:
                            JObject jo = JObject.Parse(context.ResponsePayload);
                            valueInEntry = GetProperty(jo, concurrencyProperty);
                            if (valueInEntry == null)
                            {
                                valueInEntry = GetRawValueOfProperty(context, concurrencyProperty, context);
                            }
                            break;
                        case RuleEngine.PayloadFormat.Other:
                            {
                                var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
                                var segments = ResourcePathHelper.GetPathSegments(context);
                                var segs = ResourcePathHelper.GetEntryUriSegments(segments.Take(segments.Count() - 1), edmxHelper);
                                Uri uriEntry = new Uri(context.ServiceBaseUri, string.Join("/", segs) + "/");
                                Uri uriConcurrencyValue = new Uri(uriEntry, concurrencyProperty + "/$value");
                                if (context.Destination == uriConcurrencyValue)
                                {
                                    valueInEntry = context.ResponsePayload;
                                }
                                else
                                {
                                    valueInEntry = GetProperty(uriEntry, concurrencyProperty, context);
                                }
                            }
                            break;
                    }

                    if (valueInEntry != null)
                    {
                        passed = valueInEntry == valueInHeader;
                    }
                }
            }
           
            if (passed.HasValue && !passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, etagInHeader);
            }

            return passed;
        }

        /// <summary>
        /// Gets the raw value of the property of the entry
        /// </summary>
        /// <param name="context">The context object which directly or indirectly points to an entry </param>
        /// <param name="property">The property name</param>
        /// <returns>The raw value returned from the remote service</returns>
        private static string GetRawValueOfProperty(ServiceContext context, string property, ServiceContext ctx)
        {
            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            var segs = ResourcePathHelper.GetEntryUriSegments(segments.Take(segments.Count() - 1), edmxHelper);
            Uri uriEntry = new Uri(context.ServiceBaseUri, string.Join("/", segs) + "/");
            return GetProperty(uriEntry, property, ctx);
        }

        /// <summary>
        /// Gets the core part of ETag header value
        /// </summary>
        /// <param name="value">The original ETag value</param>
        /// <returns>The stripped core part of ETag value</returns>
        private static string GetETagValueCore(string value)
        {
            if (value.StartsWith(@"W/"))
            {
                return GetETagValueCore(value.Substring(2));
            }
            else
            {
                return value.Trim('"');
            }
        }

        /// <summary>
        /// Gets value of the specified property of payload
        /// </summary>
        /// <param name="payload">The payload</param>
        /// <param name="property">The name of property</param>
        /// <returns>The property value</returns>
        private static string GetProperty(XElement payload, string property)
        {
            string result = null;

            const string tmplXpath = "//*[local-name()='{0}']";
            string xpath = string.Format(tmplXpath, property);
            var node = payload.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            if (node != null)
            {
                result = node.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets value of the specified property of payload
        /// </summary>
        /// <param name="payload">The payload</param>
        /// <param name="property">The name of property</param>
        /// <returns>The property value</returns>
        private static string GetProperty(JObject payload, string property)
        {
            string result = null;
            var x = from p in payload.Descendants()
                    where p is JProperty && ((JProperty)p).Name == property
                    select ((JProperty)p).Value;
            if (x.Any())
            {
                result = x.First().Value<string>();
            }

            return result;
        }

        /// <summary>
        /// Gets value of the specified property of the entry
        /// </summary>
        /// <param name="uriEntry">The entry</param>
        /// <param name="property">The name of property</param>
        /// <returns>The property value</returns>
        private static string GetProperty(Uri uriEntry, string property, ServiceContext ctx)
        {
            Uri uri = new Uri(uriEntry, property + "/$value");
            var resp = WebResponseHelper.GetWithHeaders(uri, false, null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, ctx);
            return resp.ResponsePayload;
        }
    }
}
