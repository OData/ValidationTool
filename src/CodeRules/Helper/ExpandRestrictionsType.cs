// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The ExpandRestrictionsType structure.
    /// </summary>
    public struct ExpandRestrictionsType
    {
        /// <summary>
        /// The constructor of the ExpandRestrictionsType structure.
        /// </summary>
        /// <param name="expandable">Indicates whether the entity-set can be expanded or not.</param>
        /// <param name="nonExpandableProperties">The navigation properties cannot be expanded.</param>
        public ExpandRestrictionsType(bool? expandable, List<string> nonExpandableProperties)
        {
            this.expandable = expandable;
            this.nonExpandableProperties = nonExpandableProperties;
        }

        /// <summary>
        /// Gets the value of expandable.
        /// </summary>
        public bool? Expandable
        {
            get
            {
                return this.expandable;
            }
        }

        /// <summary>
        /// Gets the value of non-expandable properties.
        /// </summary>
        public List<string> NonExpandableProperties
        {
            get
            {
                return this.nonExpandableProperties;
            }
        }

        /// <summary>
        /// Indicates whether the entity-set can be expanded or not.
        /// </summary>
        private bool? expandable;

        /// <summary>
        /// The navigation properties cannot be expanded.
        /// </summary>
        private List<string> nonExpandableProperties;
    }
}
