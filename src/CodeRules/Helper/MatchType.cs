// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Identify the match type when search some context.
    /// </summary>
    public enum MatchType : byte
    {
        /// <summary>
        /// The default value.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Indicate two parameters' context are equal with each others.
        /// </summary>
        Equal = 0x01,

        /// <summary>
        /// Indicate one parameter's context is contained in the other.
        /// </summary>
        Contained = 0x02,
    }
}
