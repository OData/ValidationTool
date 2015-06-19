// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum of payload types like servcie document, metadata document, feed, entry etc
    /// </summary>
    public enum PayloadType
    {
        /// <summary>
        /// No payload type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Payload type of service document.
        /// </summary>
        ServiceDoc = 1,

        /// <summary>
        /// Payload type of metadata document.
        /// </summary>
        Metadata = 2,

        /// <summary>
        /// Payload type of feed (entity set).
        /// </summary>
        Feed = 3,

        /// <summary>
        /// Payload type of entry (entity).
        /// </summary>
        Entry = 4,

        /// <summary>
        /// Payload type of OData error message.
        /// </summary>
        Error = 5,

        /// <summary>
        /// Payload type of OData property (including complex type property and primitive property)
        /// </summary>
        Property = 6,

        /// <summary>
        /// Payload type of raw value
        /// </summary>
        RawValue = 7,

        /// <summary>
        /// Payload type of OData link
        /// </summary>
        Link = 8,

        /// <summary>
        /// Payload type of delta response
        /// </summary>
        Delta = 9,
        
        /// <summary>
        /// Payload type of entity reference
        /// </summary>
        EntityRef = 10,

        /// <summary>
        /// Payload type of Individual Property
        /// </summary>
        IndividualProperty = 11,

        /// <summary>
        /// Other payload irrelevant to OData.
        /// </summary>
        Other = 99,
    }
}
