// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Indicate the type of an annotation.
    /// </summary>
    public enum AnnotationType : byte
    {
        /// <summary>
        /// All types.
        /// </summary>
        All = 0x01,

        /// <summary>
        /// Json Object only.
        /// </summary>
        Object = 0x02,

        /// <summary>
        /// Json Array or Json Primitive.
        /// </summary>
        ArrayOrPrimitive = 0x03,
    }
}
