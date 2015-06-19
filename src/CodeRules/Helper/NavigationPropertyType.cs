// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Indicate the navigation property's type.
    /// </summary>
    public enum NavigationPropertyType
    {
        /// <summary>
        /// An entity.
        /// </summary>
        Entity = 0x01,

        /// <summary>
        /// A set of entities.
        /// </summary>
        SetOfEntities = 0x02
    }
}
