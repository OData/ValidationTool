// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The OptimisticConcurrencyControlType structure.
    /// </summary>
    public struct OptimisticConcurrencyControlType
    {
        /// <summary>
        /// The constructor of the OptimisticConcurrencyControlType structure.
        /// </summary>
        /// <param name="eTagDependsOn">The eTagDependsOn.</param>
        public OptimisticConcurrencyControlType(List<string> eTagDependsOn)
        {
            this.eTagDependsOn = eTagDependsOn;
        }

        /// <summary>
        /// Gets the value of eTagDependsOn.
        /// </summary>
        public List<string> ETagDependsOn
        {
            get
            {
                return this.eTagDependsOn;
            }
        }

        /// <summary>
        /// The eTagDependsOn.
        /// </summary>
        private List<string> eTagDependsOn;
    }
}
