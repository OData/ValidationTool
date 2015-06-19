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
    public class CommonCore4014_Entry : CommonCore4014
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
    public class CommonCore4014_Feed : CommonCore4014
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
    public abstract class CommonCore4014 : ExtensionRule
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
                return "Common.Core.4014";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"To support streaming scenarios the following payload ordering constraints have to be met: All annotations for a structural or navigation property MUST appear as a group immediately before the property they annotate.";
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
        /// Verify Common.Core.4014
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
                List<XElement> navigProps = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Navigations);
                List<XElement> normalProps = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Normal);

                List<string> navigPropNames = (from navigP in navigProps select navigP.Attribute("Name").Value).ToList();
                List<string> normalPropNames = (from normaP in normalProps select normaP.Attribute("Name").Value).ToList();

                JObject jObj;
                context.ResponsePayload.TryToJObject(out jObj);

                if (jObj != null && jObj.Type == JTokenType.Object)
                {
                    passed = AnnotationsImmeBefore(jObj, navigPropNames, normalPropNames, context.PayloadType);

                    if (passed.HasValue && passed.Value == false)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Get whether the group of annotations of structural or navigation properties stand immediately before the property they annotate.
        /// </summary>
        /// <param name="jo">The JSON jobject.</param>
        /// <param name="navigPropNames">Navigation property name list.</param>
        /// <param name="normalPropNames">normal structure property name list.</param>
        /// <param name="payloadType">Feed or entry for the payload type.</param>
        /// <returns>True: rule pass; false: rule fail.</returns>
        public bool? AnnotationsImmeBefore(JObject jo, List<string> navigPropNames, List<string> normalPropNames, RuleEngine.PayloadType payloadType)
        {
            if (payloadType == RuleEngine.PayloadType.Feed)
            {
                bool? result = null;
                foreach (JObject ob in (JArray)jo[Constants.Value])
                {
                    result = AnnotationsImmeBefore(ob, navigPropNames, normalPropNames, RuleEngine.PayloadType.Entry);

                    if (result.HasValue && result.Value == false)
                    {
                        break;
                    }
                }
                return result;
            }
            else if (payloadType == RuleEngine.PayloadType.Entry)
            {
                bool? result = null;

                List<string> annotationNamesGroup = new List<string>();
                string record = string.Empty;

                var jProps = jo.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (JsonSchemaHelper.IsAnnotation(jProp.Name))
                    {
                        if (jProp.Name.Contains("@") && !jProp.Name.StartsWith("@"))
                        {
                            string temp = jProp.Name.Remove(jProp.Name.IndexOf("@"), jProp.Name.Length - jProp.Name.IndexOf("@"));

                            if (navigPropNames.Contains(temp) || normalPropNames.Contains(temp))
                            {
                                if (string.Empty == record || temp == record)
                                {
                                    annotationNamesGroup.Add(jProp.Name);
                                    record = temp;
                                }
                                else
                                {
                                    annotationNamesGroup.Clear();
                                    annotationNamesGroup.Add(jProp.Name);
                                    record = temp;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (annotationNamesGroup.Count > 0)
                        {

                            if (record == jProp.Name)
                            {
                                result = true;
                            }
                            else if (navigPropNames.Contains(record) && normalPropNames.Contains(jProp.Name))
                            {
                                result = null;
                            }
                            else
                            {
                                result = false;
                                break;
                            }

                            record = string.Empty;
                            annotationNamesGroup.Clear();
                        }
                    }
                }
                return result;
            }
            return null;
        }
    }
}
