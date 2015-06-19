// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces

    using System;

    #endregion

    /// <summary>
    /// Interface for interop validation result consumer
    /// </summary>
    public interface IResultProvider
    {
        /// <summary>
        /// Method which accepts a validation result
        /// </summary>
        /// <param name="result">result of a rule validation</param>
        void Accept(TestResult result);

        /// <summary>
        /// Method which notifies the rule engine finishes the specific validation job
        /// </summary>
        /// <param name="errorOccurred">fkag whether any runtime failure took place validating the current job</param>
        void JobCompleted(bool errorOccurred);
    }
}
