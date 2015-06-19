// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Enum type of URI path according to OData sepc 2.2.3.5 Resource PAth: Semantics
    /// </summary>
    public enum UriType
    {

        // below are what is not explicitly stated in spec 
        URI_Link = -3,      // transient state
        URI_CollEt = -2,    // Spec seems missing this definition
        URI_Container = -1, // this is reserved for URI path ending with EntityContainer
        URIUNKNOWN = 0,

        // types defined by spec
        URI1 = 1,
        URI2,
        URI3,
        URI4,
        URI5,
        URI6,
        URI7,
        URI8,
        URI9,
        URI10,
        URI11,
        URI12,
        URI13,
        URI14,
        URI15,
        URI16,
        URI17,
    }
}