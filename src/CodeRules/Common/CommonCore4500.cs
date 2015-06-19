// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion   

    /// <summary>
    /// Class of code rule applying to feed payload.  
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4500_Feed : CommonCore4500
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
    /// Class of code rule applying to entity reference payload.  
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4500_Entry : CommonCore4500
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
    /// Class of extension rule for Common.Core.4500
    /// </summary>
    public abstract class CommonCore4500 : ExtensionRule
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
                return "Common.Core.4500";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If not specified, or specified as IEEE754Compatible=false, all Edm.Int64 and Edm.Decimal numbers MUST be serialized as JSON numbers.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "3.2";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
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
            string acceptHeader = string.Empty;
            bool? resultOfIEEE754CompatibleFalse = null;
            bool? resultOfIEEE754CompatibleNotSpecified = null;
            JObject jo;

            string odataCount = Constants.V4OdataCount;
            if (context.Version == ODataVersion.V3)
            {
                odataCount = Constants.OdataCount;
            }

            if (context.PayloadType == RuleEngine.PayloadType.Feed || context.PayloadType == RuleEngine.PayloadType.Entry) 
            {
                Uri absoluteUri = new Uri(context.Destination.OriginalString.Split('?')[0]);
                Uri relativeUri = new Uri("?$format=application/json;odata.metadata=full;IEEE754Compatible=false", UriKind.Relative);
                Uri combinedUri = new Uri(absoluteUri, relativeUri);

                // Send request with IEEE754Compatible=false query parameter.
                acceptHeader = Constants.AcceptHeaderJson;
                Response responseOfIEEE754CompatibleFalse = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                if (responseOfIEEE754CompatibleFalse.StatusCode == HttpStatusCode.OK)
                {
                    responseOfIEEE754CompatibleFalse.ResponsePayload.TryToJObject(out jo);
                    resultOfIEEE754CompatibleFalse = this.IsInt64AndDecimalAsNumber(jo, context.PayloadType, odataCount);
                }

                if (resultOfIEEE754CompatibleFalse.HasValue)
                {
                    if (resultOfIEEE754CompatibleFalse.Value == true)
                    {
                        // Send request with not specifying IEEE754Compatible query parameter.
                        relativeUri = new Uri("?$format=application/json;odata.metadata=full", UriKind.Relative);
                        combinedUri = new Uri(absoluteUri, relativeUri);
                        Response responseOfIEEE754CompatibleNotSpecified = WebHelper.Get(combinedUri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                        if (responseOfIEEE754CompatibleNotSpecified.StatusCode == HttpStatusCode.OK)
                        {
                            responseOfIEEE754CompatibleNotSpecified.ResponsePayload.TryToJObject(out jo);
                            resultOfIEEE754CompatibleNotSpecified = this.IsInt64AndDecimalAsNumber(jo, context.PayloadType, odataCount);

                            if (resultOfIEEE754CompatibleNotSpecified.HasValue && resultOfIEEE754CompatibleNotSpecified.Value == true)
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
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    }
                }                               
            }

            return passed;
        }

        /// <summary>
        /// Whether the Edm.Int64, Edm.Decimal or count are represented as number in feed or entry full metadata response.
        /// </summary>
        /// <param name="jo">the json object of response.</param>
        /// <param name="payloadType">The response payload type.</param>
        /// <param name="odataCount">The odata.count annotation name.</param>
        /// <returns>true: all Int64 and decimal type value represent as number; false: otherwise; null: not applicable.</returns>      
        public bool? IsInt64AndDecimalAsNumber(JObject jo, RuleEngine.PayloadType payloadType, string odataCount)
        {
            bool? result = null;

            if (payloadType == RuleEngine.PayloadType.Entry)
            {
                foreach (JProperty jp in jo.Children<JProperty>())
                {
                    if (jp.Name.EndsWith(Constants.V4OdataType) && !jp.Name.Equals(Constants.V4OdataType))
                    {
                        if (jp.Value.ToString().StripOffDoubleQuotes().Equals("#Int64"))
                        {
                            if (((JProperty)jp.Next).Value.Type == JTokenType.Integer)
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                                break;
                            }
                        }

                        if (jp.Value.ToString().StripOffDoubleQuotes().Equals("#Decimal"))
                        {
                            if (((JProperty)jp.Next).Value.Type == JTokenType.Float)
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
            }
            else if (payloadType == RuleEngine.PayloadType.Feed)
            {
                foreach (JProperty jp in jo.Children<JProperty>())
                {
                    if (jp.Name.Equals(odataCount))
                    {
                        if (((JProperty)jp).Value.Type == JTokenType.Integer)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    }
                }

                foreach (JObject ob in (JArray)jo[Constants.Value])
                {
                    bool? resultMiddle = IsInt64AndDecimalAsNumber(ob, RuleEngine.PayloadType.Entry, odataCount);
                    
                    if (resultMiddle.HasValue)
                        result = resultMiddle;

                    if (result == false)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
