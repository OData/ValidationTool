// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The SortRestrictionsType structure.
    /// </summary>
    public struct SortRestrictionsType
    {
        /// <summary>
        /// The constructor of the SortRestrictionsType structure.
        /// </summary>
        /// <param name="sortable">Indicates whether the entity-set can be sorted.</param>
        /// <param name="ascendingOnlyProperties">The properties only can be sorted by asc.</param>
        /// <param name="descendingOnlyProperties">The properties only can be sorted by desc.</param>
        /// <param name="nonSortableProperties">The properties cannot be sorted.</param>
        public SortRestrictionsType(bool? sortable, List<string> ascendingOnlyProperties, List<string> descendingOnlyProperties, List<string> nonSortableProperties)
        {
            this.sortable = sortable;
            this.ascendingOnlyProperties = ascendingOnlyProperties;
            this.descendingOnlyProperties = descendingOnlyProperties;
            this.nonSortableProperties = nonSortableProperties;
        }

        /// <summary>
        /// Gets the value of sortable.
        /// </summary>
        public bool? Sortable
        {
            get
            {
                return this.sortable;
            }
        }

        /// <summary>
        /// Gets the value of ascendingOnlyProperties.
        /// </summary>
        public List<string> AscendingOnlyProperties
        {
            get
            {
                return this.ascendingOnlyProperties;
            }
        }

        /// <summary>
        /// Gets the value of descendingOnlyProperties.
        /// </summary>
        public List<string> DescendingOnlyProperties
        {
            get
            {
                return this.descendingOnlyProperties;
            }
        }

        /// <summary>
        /// Gets the value of nonSortableProperties.
        /// </summary>
        public List<string> NonSortableProperties
        {
            get
            {
                return this.nonSortableProperties;
            }
        }

        /// <summary>
        /// Indicates whether the entity-set can be sorted.
        /// </summary>
        private bool? sortable;

        /// <summary>
        /// The properties only can be sorted by asc.
        /// </summary>
        private List<string> ascendingOnlyProperties;

        /// <summary>
        /// The properties only can be sorted by desc.
        /// </summary>
        private List<string> descendingOnlyProperties;

        /// <summary>
        /// The properties cannot be sorted.
        /// </summary>
        private List<string> nonSortableProperties;
    }
}
