// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Identify the matching property type when search in odata service metadata.
    /// </summary>
    public enum MatchPropertyType : byte
    {
        /// <summary>
        /// All the normal properties.
        /// </summary>
        Normal = 0x01,

        /// <summary>
        /// All the navigation properties.
        /// </summary>
        Navigations = 0x02,

        /// <summary>
        /// The properties with all types.
        /// </summary>
        All = 0x03,
    }
}
