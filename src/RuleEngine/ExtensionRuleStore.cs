// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.CodeAnalysis;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of rule store as MEF directory catalog
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812: remove code never been used from the assembly", Justification = "extenal caller shall use it to create rule store")]
    public class ExtensionRuleStore : IRuleStore
    {
        /// <summary>
        /// Collection of semantic rules. They shall be populated by MEF
        /// </summary>
        [ImportMany]
        private IEnumerable<ExtensionRule> Extensions = new List<ExtensionRule>();

        /// <summary>
        /// Creates an instance of ExtensionRuleStore from a folder.
        /// </summary>
        /// <param name="path">The folder of extension rule assemblies</param>
        public ExtensionRuleStore(string path)
            : this(path, null)
        {
        }

        /// <summary>
        /// Creates an instance of ExtensionRuleStore from a folder and ILogger object.
        /// </summary>
        /// <param name="path">The folder of extension rule assemblies</param>
        /// <param name="logger">The ILogger object to log runtime errors</param>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public ExtensionRuleStore(string path, ILogger logger)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "path");
            }

            using (var catalog = new AggregateCatalog())
            {
                try
                {
                    var directoryCatalog = new DirectoryCatalog(path);
                    catalog.Catalogs.Add(directoryCatalog);
                    using (var container = new CompositionContainer(catalog))
                    {
                        container.ComposeParts(this);
                    }
                }
                catch (Exception ex)
                {
                    if (!ExceptionHelper.IsCatchableExceptionType(ex))
                    {
                        throw;
                    }

                    RuntimeException.WrapAndLog(ex, Resource.InvalidExtensionRules + ":" + path, logger);
                }
            }
        }

        /// <summary>
        /// Gets rules from the rule store
        /// </summary>
        /// <returns>Collection of rules</returns>
        public IEnumerable<Rule> GetRules()
        {
            List<Rule> rules = new List<Rule>();

            foreach (var e in this.Extensions)
            {
                Rule rule = ExtensionRuleStore.CreateRule(e);
                rules.Add(rule);
            }

            return rules;
        }

        /// <summary>
        /// Converts a semantic rule extension to Interop Rule object
        /// </summary>
        /// <param name="extension">semantic rule extension</param>
        /// <returns>The Interop Rule object</returns>
        private static Rule CreateRule(ExtensionRule extension)
        {
            Rule rule = new Rule();
            rule.Name = extension.Name;
            rule.Category = extension.Category;
            rule.Description = extension.Description;
            rule.SpecificationSection = extension.SpecificationSection;
            rule.V4SpecificationSection = extension.V4SpecificationSection;
            rule.V4Specification = extension.V4Specification;
            rule.HelpLink = extension.HelpLink;
            rule.ErrorMessage = extension.ErrorMessage;
            rule.RequirementLevel = extension.RequirementLevel;
            rule.Aspect = extension.Aspect;
            rule.PayloadType = extension.PayloadType;
            rule.IsMediaLinkEntry = extension.IsMediaLinkEntry;
            rule.Projection = extension.Projection;
            rule.PayloadFormat = extension.PayloadFormat;
            rule.Version = extension.Version;
            rule.RequireMetadata = extension.RequireMetadata;
            rule.RequireServiceDocument = extension.RequireServiceDocument;
            rule.Offline = extension.IsOfflineContext;
            rule.OdataMetadataType = extension.OdataMetadataType;
            rule.LevelType = extension.LevelType;
            rule.ResourceType = extension.ResourceType;
            rule.DependencyType = extension.DependencyType;
            if (extension.DependencyType.HasValue && extension.DependencyType.Value == ConformanceDependencyType.Dependency && extension.DependencyInfo != null)
            {
                rule.DependencyInfo = new ConformanceRuleDependencyInfo(extension.DependencyInfo.CheckType, extension.DependencyInfo.RuleRelationship, extension.DependencyInfo.BindingRules);
            }

            rule.Action = new ExtensionVerifier(extension.Verify);
            rule.Condition = null;

            return rule;
        }
    }
}
