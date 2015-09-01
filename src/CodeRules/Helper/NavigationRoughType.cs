// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    #endregion
    /// <summary>
    /// The rough-type of a navigation property.
    /// </summary>
    [Flags]
    public enum NavigationRoughType
    {
        /// <summary>
        /// Indicates the navigation property type is undefined.
        /// </summary>
        None = 0x03,

        /// <summary>
        /// Indicates the navigation property binds to an single entity.
        /// </summary>
        SingleValued = 0x01,

        /// <summary>
        /// Indicates the navigation property binds to a collection of entities.
        /// </summary>
        CollectionValued = 0x02,
    }
}
