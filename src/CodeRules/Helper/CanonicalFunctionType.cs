// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Canonical function type.
    /// </summary>
    public enum CanonicalFunctionType : byte
    {
        /// <summary>
        /// The function has been defined in the technical document.
        /// </summary>
        Supported = 0x01,

        /// <summary>
        /// The function has not been defined in the technical document.
        /// </summary>
        Unsupported = 0x02,
    }
}
