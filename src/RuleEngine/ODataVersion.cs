// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum type of various OData protocol version
    /// </summary>
    public enum ODataVersion
    {
        /// <summary>
        /// unknown version
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// version 1.0 or version 2.0
        /// </summary>
        V1_V2 = -1,

        /// <summary>
        /// any OData version
        /// </summary>
        V_All = -2,

        /// <summary>
        /// veriosn 1.0
        /// </summary>
        V1 = 1,

        /// <summary>
        /// version 2.0
        /// </summary>
        V2 = 2,

        /// <summary>
        /// version 3.0
        /// </summary>
        V3 = 3,

        /// <summary>
        /// version 4.0
        /// </summary>
        V4 = 4,

        /// <summary>
        /// version 3.0 or version 4.0
        /// </summary>
        V3_V4 = 5,

        /// <summary>
        /// version 1.0 or 2.0 or 3.0
        /// </summary>
        V1_V2_V3 = 6,
    }
}
