// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Interface of rule store
    /// </summary>
    internal interface IRuleStore
    {
        /// <summary>
        /// Gets all the rules from the rule store
        /// </summary>
        /// <returns>Rules defined in the rule store</returns>
        IEnumerable<Rule> GetRules();
    }
}
