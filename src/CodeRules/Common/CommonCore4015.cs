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
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4015_Entry : CommonCore4015
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
    public class CommonCore4015_Feed : CommonCore4015
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
    /// Class of code rule applying to DeltaResponse payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public abstract class CommonCore4015 : ExtensionRule
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
                return "Common.Core.4015";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"To support streaming scenarios the following payload ordering constraints have to be met:The one exception is the odata.nextlink annotation of an expanded collection which MAY appear after the navigation property it annotates.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.4";
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
                return RequirementLevel.May;
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
        /// Gets the odata metadata type to which the rule applies.
        /// </summary>
        public override ODataMetadataType? OdataMetadataType
        {
            get
            {
                return RuleEngine.ODataMetadataType.FullOnly;
            }
        }

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verify Common.Core.4015
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            if (context.ResponseHttpHeaders.Contains(context.Version == ODataVersion.V4 ? Constants.V4OdataStreaming : Constants.OdataStreaming) &&
                context.Destination.Query.Contains(@"$expand="))
            {
                List<XElement> props = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Navigations);
                List<string> navigPropNames = new List<string>();

                foreach (var prop in props)
                {
                    navigPropNames.Add(prop.Attribute("Name").Value);
                }

                JObject jObj;
                context.ResponsePayload.TryToJObject(out jObj);

                if (jObj != null && jObj.Type == JTokenType.Object)
                {
                    var jProps = jObj.Children();

                    foreach (JProperty j in jProps)
                    {
                        if (navigPropNames.Contains(j.Name) && j.Value.Type == JTokenType.Array)
                        {
                            var req = WebRequest.Create(context.DestinationBasePath + "/" + j.Name + @"/$count") as HttpWebRequest;
                            Response resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
                            passed = null;

                            if ((j.Value as JArray).Count < Convert.ToInt32(resp.ResponsePayload))
                            {
                                if ((j.Next as JProperty).Name ==
                                    (ODataVersion.V4 == context.Version ?
                                    Constants.V4OdataNextLink : Constants.OdataNextLink))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return passed;
        }
    }
}
