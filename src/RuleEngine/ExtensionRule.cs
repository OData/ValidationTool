// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System.Diagnostics.CodeAnalysis;
    #endregion

    /// <summary>
    /// Interface of Semantion Extension Rule
    /// </summary>
    public abstract class ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public abstract string HelpLink { get; }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// Gets the requirement level
        /// </summary>
        public abstract RequirementLevel RequirementLevel { get; }

        /// <summary>
        /// Gets descriptive summary of the rule
        /// </summary>
        public virtual string Aspect 
        { 
            get 
            { 
                return null; 
            } 
        }

        /// <summary>
        /// Gets specification section for OData v1_v2, v3.
        /// </summary>
        public virtual string SpecificationSection
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets specification section for OData V4.
        /// </summary>
        public virtual string V4SpecificationSection
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets specification for OData V4.
        /// </summary>
        public virtual string V4Specification
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets payload type
        /// </summary>
        public abstract PayloadType? PayloadType { get; }

        /// <summary>
        /// The conformance level type: Minimal, Intermediate, Advanced
        /// </summary>
        public virtual ConformanceLevelType? LevelType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the conformance resource type (read-write, read-only)
        /// </summary>
        public virtual ConformanceServiceType? ResourceType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The conformance dependency type: Single, Dependency, Skip - Default is "Single"
        /// </summary>
        public virtual ConformanceDependencyType? DependencyType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The rule dependency info, include: relationship, checktype and binding rules.
        /// </summary>
        public virtual ConformanceRuleDependencyInfo DependencyInfo
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule is about an MLE entry
        /// </summary>
        public virtual bool? IsMediaLinkEntry
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag of whether the rule applies to projected request or not
        /// </summary>
        public virtual bool? Projection
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Get the payload format
        /// </summary>
        public virtual PayloadFormat? PayloadFormat
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the OData version
        /// </summary>
        public virtual ODataVersion? Version 
        { 
            get 
            { 
                return null; 
            } 
        }

        /// <summary>
        /// Gets the OData metadata type (minimal, full)
        /// </summary>
        public virtual ODataMetadataType? OdataMetadataType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public virtual bool? RequireMetadata
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires service document
        /// </summary>
        public virtual bool? RequireServiceDocument 
        { 
            get 
            { 
                return null; 
            } 
        }

        /// <summary>
        /// Gets the flag whether this applies to offline context. 
        /// For code rules that requires extra responses from OData producer, flag should be set false, 
        /// since the producer should be a live one available when the validation is going on.
        /// Most of code rules do incur extra responses; hence the false by default.
        /// </summary>
        public virtual bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise. Null if not applicable.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "out parameter here to keep return simple bool")]
        public abstract bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info);
    }
}
