// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code rule when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2015_Entry : CommonCore2015
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
    public class CommonCore2015_Property : CommonCore2015
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
    public class CommonCore2015_RawValue : CommonCore2015
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
    /// Abstract base class of rule #252
    /// </summary>
    public abstract class CommonCore2015 : ExtensionRule
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
                return "Common.Core.2015";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When a server responds, indicating success, to a request performed against a URI that identifies a single entity, properties of an entity or a Media Resource (as specified in URI Format: Resource Addressing Rules (section 2.2.3)), and whose EntityType is enabled for optimistic concurrency control, it MUST include an ETag header in the HTTP response.";
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
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

            //success is implied; otherwise OData error payload would have been received
            //apply rule when URI path points to a single entity, properties of an entity or Media Resource
            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);
            bool IsUriPathApplicable = uriType == UriType.URI2 
                || uriType == UriType.URI3 
                || uriType == UriType.URI4 
                || uriType == UriType.URI5 
                || uriType == UriType.URI10 
                || uriType == UriType.URI17;
            if (IsUriPathApplicable)
            {
                string entityName = ResourcePathHelper.GetEntityName(context);
                string concurrencyProperty = ResourcePathHelper.GetConcurrencyProperty(XElement.Parse(context.MetadataDocument), entityName);
                if (concurrencyProperty != null)
                {
                    //to check the existence of ETag header
                    var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag");
                    passed = etagInHeader != null;
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, string.Empty);
                    }
                }
            }
            return passed;
        }
    }
}
