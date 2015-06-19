// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The InsertRestrictionsType structure.
    /// </summary>
    public struct InsertRestrictionsType
    {
        /// <summary>
        /// The constructor of the InsertRestrictionsType structure.
        /// </summary>
        /// <param name="insertable">Indicates whether the entity-set can be insertable or not.</param>
        /// <param name="nonInsertableNavigationProperties">The navigation properties cannot be inserted.</param>
        public InsertRestrictionsType(bool? insertable, List<string> nonInsertableNavigationProperties)
        {
            this.insertable = insertable;
            this.nonInsertableNavigationProperties = nonInsertableNavigationProperties;
        }

        /// <summary>
        /// Gets the value of insertable.
        /// </summary>
        public bool? Insertable
        {
            get
            {
                return this.insertable;
            }
        }

        /// <summary>
        /// Gets the value of nonInsertableNavigationProperties.
        /// </summary>
        public List<string> NonInsertableNavigationProperties
        {
            get
            {
                return this.nonInsertableNavigationProperties;
            }
        }

        /// <summary>
        /// Indicates whether the entity-set can be insertable or not.
        /// </summary>
        private bool? insertable;

        /// <summary>
        /// The navigation properties cannot be inserted.
        /// </summary>
        private List<string> nonInsertableNavigationProperties;
    }
}
