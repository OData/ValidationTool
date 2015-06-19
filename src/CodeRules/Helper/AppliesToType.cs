// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// The AppliesToType enumeration.
    /// </summary>
    public enum AppliesToType
    {
        /// <summary>
        /// The EntityType note in metadata document.
        /// </summary>
        EntityType,

        /// <summary>
        /// The ComplexType note in metadata document.
        /// </summary>
        ComplexType,

        /// <summary>
        /// The EntityContainer note in metadata document.
        /// </summary>
        EntityContainer,

        /// <summary>
        /// The Annotations note in metadata document.
        /// </summary>
        Annotations,

        /// <summary>
        /// The EntitySet note in metadata document.
        /// </summary>
        EntitySet,

        /// <summary>
        /// The Property note in metadata document.
        /// </summary>
        Property,

        /// <summary>
        /// The NavigationProperty note in metadata document.
        /// </summary>
        NavigationProperty,
    }
}
