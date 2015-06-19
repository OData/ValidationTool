// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Common.Core.4700
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4700 : ExtensionRule
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
                return "Common.Core.4700";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The odata.metadata=none format parameter indicates that the service SHOULD omit control information other than odata.nextLink and odata.count.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "3.1.3";
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
                return RequirementLevel.Should;
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
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return null;
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
        /// Verify Common.Core.4700
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
            JObject allobject;
            string acceptHeader = string.Empty;

            string noneMetadata = Constants.V3AcceptHeaderJsonNoMetadata;
            string odataCount = Constants.OdataCount;
            string odataNextLink = Constants.OdataNextLink;

            if (context.Version == ODataVersion.V4)
            {
                odataCount = Constants.V4OdataCount;
                odataNextLink = Constants.V4OdataNextLink;
                noneMetadata = Constants.V4AcceptHeaderJsonNoMetadata;
            }

            // Send none metadata Get request with no metadata acceptheader and get the Content-Type for response header.
            acceptHeader = Constants.V3AcceptHeaderJsonNoMetadata;

            if (context.Version == ODataVersion.V4)
            {
                acceptHeader = Constants.V4AcceptHeaderJsonNoMetadata;
            }

            Response response = WebHelper.Get(context.Destination, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            response.ResponsePayload.TryToJObject(out allobject);
            string contentType = response.ResponseHeaders.GetHeaderValue("Content-Type");

            // Whether the odata.metadata=none format parameter exist in response header.
            if (contentType.Contains(noneMetadata))
            {
                if (context.PayloadType == RuleEngine.PayloadType.Feed)
                {
                    JArray result = null;

                    // Get children of Value property.
                    foreach (var r in allobject.Children<JProperty>())
                    {
                        if (JsonSchemaHelper.IsAnnotation(r.Name))
                        {
                            if (r.Name.Equals(odataCount, StringComparison.Ordinal)
                                || r.Name.Equals(odataNextLink, StringComparison.Ordinal))
                            {
                                passed = true;
                            }
                            else
                            {
                                passed = false;
                            }
                        }
                        else if (r.Name.Equals(Constants.Value, StringComparison.Ordinal)
                            && r.Value.Type == JTokenType.Array)
                        {
                            result = (JArray)r.Value;
                        }
                    }

                    foreach (JObject entry in result)
                    {
                        bool? valueArrayPass = this.VerifyNoneMetadataAnnotation(entry, odataCount, odataNextLink);
                        if (!passed.HasValue && valueArrayPass.HasValue)
                        {
                            passed = valueArrayPass;
                        }
                        else if (passed.HasValue && valueArrayPass.HasValue)
                        {
                            passed = valueArrayPass.Value && passed.Value;
                        }
                    }
                }
                else
                {
                    passed = this.VerifyNoneMetadataAnnotation(allobject, odataCount, odataNextLink);
                }
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        /// <summary>
        /// Verify the annotation when odata.metadata=none.
        /// </summary>
        /// <param name="entry">The Entry object</param>
        /// <param name="odataCount">The odata.count name</param>
        /// <param name="odataNextLink">The odata.nextLink name</param>
        /// <returns>Annotation right-true;otherwise-false</returns>
        private bool? VerifyNoneMetadataAnnotation(JObject entry, string odataCount, string odataNextLink)
        {
            bool isExistRight = false;
            bool? isAnnotationsExist = null;
            string[] annotations = 
            {
                @"odata.context", @"odata.metadata", @"odata.metadataEtag", @"odata.type", @"odata.count",
                @"odata.nextLink", @"odata.deltaLink", @"odata.id", @"odata.editLink",
                @"odata.readLink", @"odata.etag", @"odata.navigationLink", @"odata.associationLink",
                @"odata.media"
            };

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                var temps = (from a in annotations where jProp.Name.Contains(a) select a).ToArray();

                if (JsonSchemaHelper.IsAnnotation(jProp.Name) && temps.Length >= 0)
                {
                    // Annotations exist in response payload.
                    isAnnotationsExist = true;

                    if (jProp.Name.Equals(odataCount) || jProp.Name.Equals(odataNextLink))
                    {
                        // In above situation. If the annotation is odata.nextLink or odata.count, the result is true.
                        isExistRight = true;
                    }
                    else
                    {
                        isExistRight = false;
                        break;
                    }
                }
            }

            if (isAnnotationsExist == null)
            {
                isExistRight = true;
            }

            return isExistRight;
        }
    }
}

