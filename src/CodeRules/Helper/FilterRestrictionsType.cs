// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The FilterRestrictionsType structure.
    /// </summary>
    public struct FilterRestrictionsType
    {
        /// <summary>
        /// The constructor of the FilterRestrictionsType structure.
        /// </summary>
        /// <param name="countable">The countable value.</param>
        /// <param name="requiresFilter">The requires-filter value.</param>
        /// <param name="nonCountableProperties">The NonCountableProperties value.</param>
        /// <param name="nonCountableNavigationProperties">The NonCountableNavigationProperties value.</param>
        public FilterRestrictionsType(bool? countable, bool? requiresFilter, List<string> nonCountableProperties, List<string> nonCountableNavigationProperties)
        {
            this.filterable = countable;
            this.requiresFilter = requiresFilter;
            this.requiredProperties = nonCountableProperties;
            this.nonFilterableProperties = nonCountableNavigationProperties;
        }

        /// <summary>
        /// Gets the filterable value.
        /// </summary>
        public bool? Filteralbe
        {
            get
            {
                return this.filterable;
            }
        }

        /// <summary>
        /// Gets the requires-flilter value.
        /// </summary>
        public bool? RequiresFilter
        {
            get
            {
                return this.requiresFilter;
            }
        }

        /// <summary>
        /// Gets the required-properties.
        /// </summary>
        public List<string> RequiredProperties
        {
            get
            {
                return this.requiredProperties;
            }
        }

        /// <summary>
        /// Gets the non-filterable properties.
        /// </summary>
        public List<string> NonFilterableProperties
        {
            get
            {
                return this.nonFilterableProperties;
            }
        }

        /// <summary>
        /// The filterable attribute.
        /// </summary>
        private bool? filterable;

        /// <summary>
        /// The requires-filter attribute.
        /// </summary>
        private bool? requiresFilter;

        /// <summary>
        /// The required-properties.
        /// </summary>
        private List<string> requiredProperties;

        /// <summary>
        /// The non-filterable properties.
        /// </summary>
        private List<string> nonFilterableProperties;
    }
}
