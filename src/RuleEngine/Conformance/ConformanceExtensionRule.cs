// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using ODataValidator.RuleEngine;
    #endregion

    public abstract class ConformanceExtensionRule : ExtensionRule
    {
        /// <summary>
        /// Gets Category property - Default is "conformance"
        /// </summary>
        public override string Category
        {
            get
            {
                return "conformance";
            }
        }

        /// <summary>
        /// Gets the V4 specification - Default is "ODataV4SpecificationUriForProtocol"
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "ODataV4SpecificationUriForProtocol";
            }
        }

        /// <summary>
        /// Gets the version - Default is "V4"
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
                return null;
            }
        }

        /// <summary>
        /// Gets the requirement level - Default is "MUST"
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
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
        /// Gets the payload format to which the rule applies - Default is "JsonLight"
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.JsonLight;
            }
        }

        /// <summary>
        /// The conformance dependency type: Single, Dependency, Skip - Default is "Single"
        /// </summary>
        public override ConformanceDependencyType? DependencyType
        {
            get
            {
                return ConformanceDependencyType.Single;
            }
        }
    }
}
