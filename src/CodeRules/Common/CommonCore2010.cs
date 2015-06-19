// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// class of concrete code rule implementation
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2010_Feed : CommonCore2010
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Feed; }
        }
    }

    /// <summary>
    /// class of concrete code rule implementation
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2010_Entry : CommonCore2010
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Entry; }
        }
    }

    /// <summary>
    /// Abstract base class of rule #286 
    /// </summary>
    public abstract class CommonCore2010 : ExtensionRule
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
                return "Common.Core.2010";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the value of a NavigationProperty is null, then an empty element MUST appear under the element which represents the NavigationProperty,"
                    + @" indicating that the element has been expanded but that there was no content associated with it.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.6.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataatom";
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
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
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
            bool passed = true;

            // check each <m:inline>/.. to ensure if @href gets error response (indicating null value) then <m:inline> is empty
            XElement payload = XElement.Parse(context.ResponsePayload);
            string xpath = "//m:inline";
            var inlines = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            // to optimize out duplicate resource fetches
            WebResponseCache cache = new WebResponseCache();

            foreach (var inline in inlines)
            {
                var paren = inline.Parent;
                var href = paren.GetAttributeValue("href");
                Uri uri = ResourcePathHelper.GetODataResourceUri(href, context.ServiceBaseUri);
                var response = cache.GetResponse(uri, context);
                int code = (int)response.StatusCode;
                if (code >= 400 && code < 500)
                {
                    // resource is null valued => inline is empty | fail
                    if (!inline.IsEmpty)
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo("m:inline should be empty", uri, context.DestinationBasePath);
                        break;
                    }
                }
            }

            return passed;
        }

        class WebResponseCache : Dictionary<Uri, Response>
        {
            public WebResponseCache() { }

            public Response GetResponse(Uri uri, ServiceContext ctx)
            {
                Response response;
                if (!this.TryGetValue(uri, out response))
                {
                    response = WebResponseHelper.GetWithHeaders(uri, false, null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, ctx);
                    this.Add(uri, response);
                }

                return response;
            }
        }
    }
}
