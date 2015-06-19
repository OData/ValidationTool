// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of Rule Engine Runtime Failures
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032: Add other constructors", Justification = "interop only needs the defined constructor")]
    [SuppressMessage("Microsoft.Usage", "CA2240: Add an implementation of GetObjectData to type 'RuntimeException'.", Justification = "interop does not need GetObjectData")]
    public class RuntimeException : Exception, ISerializable 
    {
        /// <summary>
        /// Creates a instance of RuntimeException from an Exception object and detail string
        /// </summary>
        /// <param name="exception">The Exception object as the inner exception</param>
        /// <param name="detail">The detail information of the runtime failure</param>
        [SuppressMessage("Microsoft.Design", "CA1062:validate 'exception' parameter before using it", Justification = "any value is permittable")]
        public RuntimeException(Exception exception, string detail)
            : base(exception.Message, exception)
        {
            this.Timestamp = DateTime.Now;
            this.Detail = detail;
        }

        /// <summary>
        /// Gets when the runtime exception happens
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets the detail information about the runtime exception
        /// </summary>
        public string Detail { get; private set; }

        /// <summary>
        /// Gets/sets the associated validation job Id
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Gets/sets the associated validation rule name
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets/sets the assiciated Uri string
        /// </summary>
        public string DestinationEndpoint { get; set; }

        /// <summary>
        /// Wraps exception inside RuntimeException and sends it to logging object.
        /// </summary>
        /// <param name="exception">Exception object to be logged</param>
        /// <param name="detail">Detail information of the exception</param>
        /// <param name="logger">The logging object</param>
        public static void WrapAndLog(Exception exception, string detail, ILogger logger)
        {
            if (logger != null)
            {
                var e = new RuntimeException(exception, detail);
                e.JobId = Guid.Empty;
                e.RuleName = string.Empty;
                e.DestinationEndpoint = string.Empty;
                logger.Log(e);
            }
        }
    }
}
