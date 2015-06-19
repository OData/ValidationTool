// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Lambda operator type.
    /// </summary>
    public enum LambdaOperatorType : byte
    {
        /// <summary>
        /// The any operator applies a Boolean expression to each member of a collection and returns true 
        /// if the expression is true for any member of the collection, otherwise it returns false.
        /// </summary>
        Any = 0x01,

        /// <summary>
        /// The all operator applies a Boolean expression to each member of a collection and returns true 
        /// if the expression is true for all members of the collection, otherwise it returns false.
        /// </summary>
        All = 0x02,
    }
}
