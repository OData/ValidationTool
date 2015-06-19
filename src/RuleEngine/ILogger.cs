// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Interface of runtime logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a rule engine runtime failure
        /// </summary>
        /// <param name="runtimeError">The RuntimeException to be logged</param>
        void Log(RuntimeException runtimeError);
    }
}
