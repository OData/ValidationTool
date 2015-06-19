// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    #endregion

    /// <summary>
    /// Class of exception thrown when more bytes of payload is received than the maximum number that has been set specified 
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:add other constructors", Justification = "interop only needs the defined constructor")]
    public class OversizedPayloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OversizedPayloadException class with a specified message
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public OversizedPayloadException(string message)
            : base(message)
        {
        }
    }
}
