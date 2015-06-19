// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
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
    public class CommonCore4013_Entry : CommonCore4013
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
    public class CommonCore4013_Feed : CommonCore4013
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
    public abstract class CommonCore4013 : ExtensionRule
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
                return "Common.Core.4013";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"To support streaming scenarios the following payload ordering constraints have to be met: The odata.id and odata.etag annotations MUST appear before any property or property annotation.";
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
                return RequirementLevel.Must;
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
        /// Verify Common.Core.4013
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

            if (context.ResponseHttpHeaders.Contains(context.Version == ODataVersion.V4 ? Constants.V4OdataStreaming : Constants.OdataStreaming))
            {
                List<XElement> allProps = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.All);
                List<string> propNames = new List<string>();

                foreach (var prop in allProps)
                {
                    propNames.Add(prop.Attribute("Name").Value);
                }

                JObject jObj;
                context.ResponsePayload.TryToJObject(out jObj);

                if (jObj != null && jObj.Type == JTokenType.Object)
                {
                    var jProps = jObj.Children();
                    bool hasODataIdProp = false;
                    bool hasODataEtagProp = false;
                    List<string> propsOrItsAnnsBeforeOdataId = new List<string>();
                    List<string> propsOrItsAnnsBeforeOdataEtag = new List<string>();

                    foreach (JProperty j in jProps)
                    {
                        propsOrItsAnnsBeforeOdataId.AddRange(!hasODataIdProp ? propNames.Where(name => j.Name.Contains(name)).ToList() : new List<string>());
                        propsOrItsAnnsBeforeOdataEtag.AddRange(!hasODataEtagProp ? propNames.Where(name => j.Name.Contains(name)).ToList() : new List<string>());

                        if (j.Name == (context.Version == ODataVersion.V4 ? Constants.V4OdataId : Constants.OdataId))
                        {
                            hasODataIdProp = true;
                        }
                        else if (j.Name == (context.Version == ODataVersion.V4 ? Constants.V4OdataEtag : Constants.OdataEtag))
                        {
                            hasODataEtagProp = true;
                        }
                    }

                    bool odataIdValidataion = !(hasODataIdProp ^ propsOrItsAnnsBeforeOdataId.Count == 0) || (!hasODataIdProp && propsOrItsAnnsBeforeOdataId.Count == 0);
                    bool odataEtagValidation = !(hasODataEtagProp ^ propsOrItsAnnsBeforeOdataEtag.Count == 0) || (!hasODataEtagProp && propsOrItsAnnsBeforeOdataEtag.Count == 0);

                    if (odataEtagValidation && odataIdValidataion)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}
