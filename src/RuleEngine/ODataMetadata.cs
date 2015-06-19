// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum type of various OData protocol version
    /// </summary>
    public enum ODataMetadataType
    {       
        /// <summary>
        /// odata.metadata=minimal
        /// </summary>
        MinOnly,    

        /// <summary>
        /// odata.metadata=full
        /// </summary>
        FullOnly,

        /// <summary>
        /// odata.metadata=none
        /// </summary>
        None
    }
}
