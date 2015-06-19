// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The CountRestrictionsType structure.
    /// </summary>
    public struct CountRestrictionsType
    {
        /// <summary>
        /// The constructor of the CountRestrictionsType structure.
        /// </summary>
        /// <param name="countable"></param>
        /// <param name="nonCountableProperties"></param>
        /// <param name="nonCountableNavigationProperties"></param>
        public CountRestrictionsType(bool? countable, List<string> nonCountableProperties, List<string> nonCountableNavigationProperties)
        {
            this.countable = countable;
            this.nonCountableProperties = nonCountableProperties;
            this.nonCountableNavigationProperties = nonCountableNavigationProperties;
        }

        /// <summary>
        /// Gets the value of Countable attribute.
        /// </summary>
        public bool? Countable
        {
            get
            {
                return countable;
            }
        }

        /// <summary>
        /// Gets the value of NonCountableProperties attribute.
        /// </summary>
        public List<string> NonCountableProperties
        {
            get
            {
                return this.nonCountableProperties;
            }
        }

        /// <summary>
        /// Gets the value of NonCountableNavigationProperties.
        /// </summary>
        public List<string> NonCountableNavigationProperties
        {
            get
            {
                return this.nonCountableNavigationProperties;
            }
        }

        /// <summary>
        /// The Countable attribute.
        /// </summary>
        private bool? countable;

        /// <summary>
        /// The NonCountableProperties attribute.
        /// </summary>
        private List<string> nonCountableProperties;

        /// <summary>
        /// The NonCountableNavigationProperties attribute.
        /// </summary>
        private List<string> nonCountableNavigationProperties;
    }
}
