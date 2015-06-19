// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum of format (or content type) of payload
    /// </summary>
    public enum PayloadFormat
    {
        /// <summary>
        /// No payload format.
        /// </summary>
        None = 0,

        /// <summary>
        /// Payload is of atompub format.
        /// </summary>
        Atom,

        /// <summary>
        /// Payload is of json verbose format.
        /// </summary>
        Json,
       
        /// <summary>
        /// Payload is json light format.
        /// </summary>
        JsonLight,

        /// <summary>
        /// Payload is of general xml format.
        /// </summary>
        Xml,

        /// <summary>
        /// Image format.
        /// </summary>
        Image,       

        /// <summary>
        /// Other format.
        /// </summary>
        Other,
    }
}
