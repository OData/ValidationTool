// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// enum type of rule requirement level
    /// </summary>
    public enum RequirementLevel
    {
        /// <summary>
        /// MUST-level rule
        /// </summary>
        Must = 0,

        /// <summary>
        /// SHOULD-level
        /// </summary>
        Should,

        /// <summary>
        /// RECOMMENDED-level
        /// </summary>
        Recommended,

        /// <summary>
        /// MAY-level
        /// </summary>
        May,

        /// <summary>
        /// MUST NOT level
        /// </summary>
        MustNot,

        /// <summary>
        /// SHOULD NOT level
        /// </summary>
        ShouldNot,
    }
}
