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
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4703_Feed : CommonCore4703
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
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4703_Entry : CommonCore4703
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
    /// Class of extension rule for Common.Core.4703
    /// </summary>
    public abstract class CommonCore4703 : ExtensionRule
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
                return "Common.Core.4703";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The namespace for custom annotations MUST NOT start with odata.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "20";
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
                return null;
            }
        }

        /// <summary>
        /// Verifies Common.Core.4703
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

            string[] annotations = 
            {
                @"odata.context", @"odata.metadataEtag", @"odata.type", @"odata.count",
                @"odata.nextLink", @"odata.deltaLink", @"odata.id", @"odata.editLink",
                @"odata.readLink", @"odata.etag", @"odata.navigationLink", @"odata.associationLink",
                @"odata.media", @"odata.metadata"
            };


            JObject jObj;
            context.ResponsePayload.TryToJObject(out jObj);

            if (jObj != null && jObj.Type == JTokenType.Object)
            {
                var jProps = jObj.Children();

                foreach (JProperty jProp in jProps)
                {
                    var temps = (from a in annotations
                                 where jProp.Name.Contains(a)
                                 select a).ToArray();

                    if (JsonSchemaHelper.IsAnnotation(jProp.Name) && temps.Length > 0)
                    {
                        if (ODataVersion.V3 == context.Version)
                        {
                            passed = null;

                            if (jProp.Name.Contains("@"))
                            {
                                string str = jProp.Name.Remove(0, jProp.Name.IndexOf("@") + 1);

                                if (!str.StartsWith("odata"))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    if (!jProp.Name.StartsWith("odata"))
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
                            else
                            {
                                if (!jProp.Name.StartsWith("odata"))
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
                        else if (ODataVersion.V4 == context.Version)
                        {
                            passed = null;
                            string str = jProp.Name.Remove(0, jProp.Name.IndexOf("@") + 1);

                            if (!str.StartsWith("odata"))
                            {
                                passed = true;
                            }
                            else
                            {
                                if (!jProp.Name.StartsWith("odata"))
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
