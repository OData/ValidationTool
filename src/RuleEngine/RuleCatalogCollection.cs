// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    #endregion

    /// <summary>
    /// Rule repository class (singleton)
    /// </summary>
    public sealed class RuleCatalogCollection : Collection<Rule>
    {
        /// <summary>
        /// The unique instance of singleton
        /// </summary>
        private static RuleCatalogCollection instance = new RuleCatalogCollection();

        /// <summary>
        /// Private constructor to prevent any instantiation other than the allowed entry point 
        /// </summary>
        private RuleCatalogCollection()  
        {
        }

        /// <summary>
        /// The single access point for the singleton class
        /// </summary>
        public static RuleCatalogCollection Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Registers valid rules
        /// </summary>
        /// <param name="rules">the collection of rules to be registered</param>
        public void RegisterRules(IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.IsValid())
                {
                    this.Add(rule);
                }
            }
        }
    }
}
