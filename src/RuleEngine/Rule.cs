// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Class of Interop rule describing all rule related information like spec section, description, checking constraints, etc
    /// </summary>
    public sealed class Rule
    {
        /// <summary>
        /// Category of the rule
        /// </summary>
        private string category;

        /// <summary>
        /// Rule name, such as "basic service document format"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Precise and detailed rule description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Section number with '.' delimited , like "2.6.3.11.5"
        /// </summary>
        public string SpecificationSection { get; set; }

        /// <summary>
        /// Gets specification section for OData V4.
        /// </summary>
        public string V4SpecificationSection { get; set; }
      
        /// <summary>
        /// Gets specification for OData V4.
        /// </summary>
        public string V4Specification { get; set; }    

        /// <summary>
        /// The location of help information for the rule
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// Error or warning message to show when the rule is violated
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Rule requirement level as MUST, SHOULD, MAY, ...
        /// </summary>
        public RequirementLevel RequirementLevel { get; set; }

        /// <summary>
        /// Description of usage case, like readability, addressibility, etc
        /// </summary>
        public string Aspect { get; set; }  

        /// <summary>
        /// Rule category, like core, query, data over form, line of business
        /// </summary>
        public string Category 
        {
            get
            {
                return this.category;
            }

            set
            {
                this.category = (value == null) ? null : value.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Applicable response payload type (feed, entry, svc doc, metadata doc, or none for all)
        /// </summary>
        public PayloadType? PayloadType { get; set; }

        /// <summary>
        /// Whether the rule is about MLE
        /// </summary>
        public bool? IsMediaLinkEntry { get; set; }

        /// <summary>
        /// Whether the rule applies to projected request or not
        /// </summary>
        public bool? Projection { get; set; }

        /// <summary>
        /// Applicable response payload format (atom, json, xml, JsonLight, or none for all)
        /// </summary>
        public PayloadFormat? PayloadFormat { get; set; }

        /// <summary>
        /// Applicable response OData protocol version (V1, V2, V3, v4, or none for all)
        /// </summary>
        public ODataVersion? Version { get; set; }

        /// <summary>
        /// Applicable odata metadata type for json light (minimal, full)
        /// </summary>
        public ODataMetadataType? OdataMetadataType { get; set; }

        /// <summary>
        /// Applicable resource type
        /// </summary>
        public ConformanceServiceType? ResourceType { get; set; }

        /// <summary>
        /// The conformance rule level type: Minimal, Intermediate, Advanced
        /// </summary>
        public ConformanceLevelType? LevelType { get; set; }

        /// <summary>
        /// The conformance rule type: Single, Dependency, Skip
        /// </summary>
        public ConformanceDependencyType? DependencyType { get; set; }

        /// <summary>
        /// The conformance rule dependency info
        /// </summary>
        public ConformanceRuleDependencyInfo DependencyInfo { get; set; }

        /// <summary>
        /// Whether the rule depends on absence/presence of metadata document. if null, rule applies anyway
        /// </summary>
        public bool? RequireMetadata { get; set; }

        /// <summary>
        /// Whether the rule depends on absence/presence of service document. if null, rule applies anyway
        /// </summary>
        public bool? RequireServiceDocument { get; set; }

        /// <summary>
        /// Whether the rule applies to offline context(true), a live context(false), or both(null)
        /// </summary>
        public bool? Offline { get; set; }

        /// <summary>
        /// Fine-grained condition for the rule to meet before it is to be verified
        /// </summary>
        public IVerifier Condition { get; set; }

        /// <summary>
        /// Checking action for the rule tp validate
        /// </summary>
        public IVerifier Action { get; set; }

        /// <summary>
        /// Sanity check for valid rules; only valid rules are picked up by rule engine
        /// </summary>
        /// <returns>true if valid; false invalid</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.Category) || this.Action == null)
            {
                return false;
            }

            return true;
        }
    }
}
