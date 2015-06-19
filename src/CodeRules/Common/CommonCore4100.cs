// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Class of code rule applying to ServiceDoc payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4100_ServiceDoc : CommonCore4100
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.ServiceDoc;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4100_Entry : CommonCore4100
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
    public class CommonCore4100_Feed : CommonCore4100
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
    public class CommonCore4100_DeltaResponse : CommonCore4100
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Delta;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to EntityRef payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4100_EntityRef : CommonCore4100
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.EntityRef;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to other payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4100_Other : CommonCore4100
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Other;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4100
    /// </summary>
    public abstract class CommonCore4100 : ExtensionRule
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
                return "Common.Core.4100";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"Responses MUST include the odata.metadata parameter(odata.metadata=full, odata.metadata=minimal or odata.metadata=none) to specify the amount of metadata included in the response in V4.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.1";
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
                return ODataVersion.V4;
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
        /// Verify Common.Core.4100
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
            bool IsodatametadataParameter = false;
            info = null;

            if (!string.IsNullOrEmpty(context.ResponseHttpHeaders))
            {
                // Extracts content type value from ResponseHttpHeaders.
                var contentType = context.ResponseHttpHeaders.GetHeaderValue(Constants.ContentType);

                if (!string.IsNullOrEmpty(contentType))
                {
                    if (context.OdataMetadataType.Equals(RuleEngine.ODataMetadataType.FullOnly))
                    {
                        // Verify whether the value of odata.metadata is in response.
                        string[] pairs = contentType.Split(';');
                        if (pairs.Length >= 1)
                        {
                            foreach (string pair in pairs)
                            {
                                if (Regex.Replace(pair, @"\s", "").Equals("odata.metadata=full"))
                                {
                                    IsodatametadataParameter = true;
                                    break;
                                }
                            }
                        }
                    }
                    else if (context.OdataMetadataType.Equals(RuleEngine.ODataMetadataType.MinOnly))
                    {
                        // Verify whether the value of odata.metadata is in response.
                        string[] pairs = contentType.Split(';');
                        if (pairs.Length >= 1)
                        {
                            foreach (string pair in pairs)
                            {
                                if (Regex.Replace(pair, @"\s", "").Equals("odata.metadata=minimal"))
                                {
                                    IsodatametadataParameter = true;
                                    break;
                                }
                            }
                        }
                    }
                    else if (context.OdataMetadataType.Equals(RuleEngine.ODataMetadataType.None))
                    {
                        // Verify whether the value of odata.metadata is in response.
                        string[] pairs = contentType.Split(';');
                        if (pairs.Length >= 1)
                        {
                            foreach (string pair in pairs)
                            {
                                if (Regex.Replace(pair, @"\s", "").Equals("odata.metadata=none"))
                                {
                                    IsodatametadataParameter = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (IsodatametadataParameter)
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
    }
}

