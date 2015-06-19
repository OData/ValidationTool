// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Class of service context core characteristics.
    /// </summary>
    public abstract class ServiceContextCore
    {
        /// <summary>
        /// Gets type of payload (feed, entry, service document or metadata) based on payload content
        /// </summary>
        public PayloadType PayloadType { get; protected set; }

        /// <summary>
        /// Gets payload format based on payload content
        /// </summary>
        public PayloadFormat PayloadFormat { get; protected set; }

        /// <summary>
        /// Gets OData protocol version extracted from http response header of DataServiceVersion
        /// </summary>
        public ODataVersion Version { get; protected set; }

        /// <summary>
        /// Gets whether a service document has been found for the OData service this context refers to
        /// </summary>
        public bool HasServiceDocument { get; protected set; }

        /// <summary>
        /// Gets whether metadata document has been found for the OData service this context refers to
        /// </summary>
        public bool HasMetadata { get; protected set; }

        /// <summary>
        /// Gets whether it is a media link entry or not - applicable to payload type of entry only
        /// </summary>
        public bool IsMediaLinkEntry { get; protected set; }

        /// <summary>
        /// Gets whether it is of a prjected request or not
        /// </summary>
        public bool Projection { get; protected set; }

        /// <summary>
        /// Gets whether it is an offline context or live context
        /// </summary>
        public bool IsOffline { get; protected set; }
        
        /// <summary>
        /// OData metadata type.
        /// </summary>
        public ODataMetadataType OdataMetadataType { get; protected set; }

        /// <summary>
        /// Gets the category of context. 
        /// </summary>
        public string Category { get; protected set; }

        /// <summary>
        ///  Gets the conformance service type of context. 
        /// </summary>
        public ConformanceServiceType ServiceType { get; protected set; }

        /// <summary>
        ///  Gets the conformance level type of context. 
        /// </summary>
        public ConformanceLevelType[] LevelTypes { get; protected set; }
    }
}
