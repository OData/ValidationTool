// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The UpdateRestrictionsType structure.
    /// </summary>
    public struct UpdateRestrictionsType
    {
        /// <summary>
        /// The constructor of the UpdateRestrictionsType structure.
        /// </summary>
        /// <param name="updatable">Indicates whether the entity-set can be updatable or not.</param>
        /// <param name="nonUpdatableNavigationProperties">The navigation properties cannot be updated.</param>
        public UpdateRestrictionsType(bool? updatable, List<string> nonUpdatableNavigationProperties)
        {
            this.updatable = updatable;
            this.nonUpdatableNavigationProperties = nonUpdatableNavigationProperties;
        }

        /// <summary>
        /// Gets the value of updatable.
        /// </summary>
        public bool? Updatable
        {
            get
            {
                return this.updatable;
            }
        }

        /// <summary>
        /// Gets the value of nonUpdatableNavigationProperties
        /// </summary>
        public List<string> NonUpdatableNavigationProperties
        {
            get
            {
                return this.nonUpdatableNavigationProperties;
            }
        }

        /// <summary>
        /// Indicates whether the entity-set can be updatable or not.
        /// </summary>
        private bool? updatable;

        /// <summary>
        /// The navigation properties cannot be updated.
        /// </summary>
        private List<string> nonUpdatableNavigationProperties;
    }
}
