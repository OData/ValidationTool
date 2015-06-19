// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to feed payload.  
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4506_Feed : CommonCore4506
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
    /// Class of code rule applying to entity payload.  
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4506_Entry : CommonCore4506
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
    /// Class of extension rule for Common.Core.4506
    /// </summary>
    public abstract class CommonCore4506 : ExtensionRule
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
                return "Common.Core.4506";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "OData JSON payloads that format Edm.Int64 and Edm.Decimal values as strings MUST specify IEEE754Compatible format parameter in the media type returned in the Content-Type header.";
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
            bool? isInt64OrDecimalOrCountAsString = null;
            info = null;
            Response response = null;
           
            JObject jo;
            context.ResponsePayload.TryToJObject(out jo);

            string odataCount = Constants.V4OdataCount;
            if (context.Version == ODataVersion.V3)
            {
                odataCount = Constants.OdataCount;
            }

            if (context.PayloadType == RuleEngine.PayloadType.Entry || context.PayloadType == RuleEngine.PayloadType.Feed)
            {
                Uri absoluteUri = new Uri(context.Destination.OriginalString.Split('?')[0]);
                Uri relativeUri = new Uri("?$format=application/json;odata.metadata=full;IEEE754Compatible=true", UriKind.Relative);
                Uri combinedUri = new Uri(absoluteUri, relativeUri);

                // Send full metadata request.
                response = WebHelper.Get(combinedUri, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.ResponsePayload.TryToJObject(out jo);
                }
            }

            isInt64OrDecimalOrCountAsString = IsInt64OrDecimalAsString(jo, context.PayloadType, odataCount);
            if (isInt64OrDecimalOrCountAsString.HasValue && isInt64OrDecimalOrCountAsString.Value == true)
            {
                string contentType = response.ResponseHeaders.GetHeaderValue("Content-Type");

                if (Regex.Replace(contentType, @"\s+", "").Contains("IEEE754Compatible=true;"))
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }
            }

            return passed;
        }

        /// <summary>
        /// // Whether Int64, Decimal or Count are formatted as string in entry or feed.
        /// </summary>
        /// <param name="jo">The response payload object.</param>
        /// <param name="type">The payload type of object.</param>
        /// <param name="odataCount">The odata.count annotation name.</param>
        /// <returns>true:Int64 or Decimal or feed count are formatted as string; false:otherwise; null: not applicable.</returns>
        private bool? IsInt64OrDecimalAsString(JObject jo, PayloadType type, string odataCount)
        {
            bool? isInt64OrDecimalAsString = null;

            if (type == RuleEngine.PayloadType.Entry)
            {
                foreach (JProperty jProp in jo.Children())
                {
                    if (jProp.Name.EndsWith(Constants.V4OdataType) && !jProp.Name.Equals(Constants.V4OdataType)
                        && (jProp.Value.ToString().StripOffDoubleQuotes().Equals("#Decimal") || jProp.Value.ToString().StripOffDoubleQuotes().Equals("#Int64")))
                    {
                        if (((JProperty)jProp.Next).Value.Type == JTokenType.String)
                        {
                            isInt64OrDecimalAsString = true;
                        }
                        else
                        {
                            isInt64OrDecimalAsString = false;
                            break;
                        }
                    }
                }
            }
            else if (type == RuleEngine.PayloadType.Feed)
            {
                foreach (JProperty jp in jo.Children<JProperty>())
                {
                    if (jp.Name.Equals(odataCount))
                    {
                        if (((JProperty)jp).Value.Type == JTokenType.String)
                        {
                            isInt64OrDecimalAsString = true;
                        }
                        else
                        {
                            isInt64OrDecimalAsString = false;
                        }
                        break;
                    }
                }

                foreach (JObject ob in (JArray)jo[Constants.Value])
                {
                    bool? resultMiddle = IsInt64OrDecimalAsString(ob, RuleEngine.PayloadType.Entry, odataCount);

                    if (resultMiddle.HasValue)
                        isInt64OrDecimalAsString = resultMiddle;

                    if (isInt64OrDecimalAsString == false)
                    {
                        break;
                    }
                }
            }

            return isInt64OrDecimalAsString;
        }
    }
}
