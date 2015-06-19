// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespace
    using System.Diagnostics.CodeAnalysis;
    #endregion

    /// <summary>
    /// Interface for a checking constraint
    /// </summary>
    public interface IVerifier
    {
        /// <summary>
        /// Checks whether the constraint on the current request session is satisfied or not
        /// </summary>
        /// <param name="context">context object representing the current OData interop session</param>
        /// <param name="result">output paramater to return detailed information if checking did not pass</param>
        /// <returns>true/false</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "followed the interface definition")]
        bool Verify(ServiceContext context, out TestResult result);
    }
}
