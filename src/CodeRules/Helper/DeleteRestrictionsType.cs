// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The DeleteRestrictionsType structure.
    /// </summary>
    public struct DeleteRestrictionsType
    {
        /// <summary>
        /// The constructor of the DeleteRestrictionsType structure.
        /// </summary>
        /// <param name="deletable">Indicates whether the entity-set can be deletable or not.</param>
        /// <param name="nonDeletableNavigationProperties">The navigation properties cannot be deleted.</param>
        public DeleteRestrictionsType(bool? deletable, List<string> nonDeletableNavigationProperties)
        {
            this.deletable = deletable;
            this.nonDeletableNavigationProperties = nonDeletableNavigationProperties;
        }

        /// <summary>
        /// Gets the value of deletable.
        /// </summary>
        public bool? Deletable
        {
            get
            {
                return this.deletable;
            }
        }

        /// <summary>
        /// Gets the value of nonDeletableNavigationProperties.
        /// </summary>
        public List<string> NonDeletableNavigationProperties
        {
            get
            {
                return this.nonDeletableNavigationProperties;
            }
        }

        /// <summary>
        /// Indicates whether the entity-set can be deletable or not.
        /// </summary>
        private bool? deletable;

        /// <summary>
        /// The navigation properties cannot be deleted.
        /// </summary>
        private List<string> nonDeletableNavigationProperties;
    }
}
