// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    public class ServiceVerification
    {
        /// <summary>
        /// Asc Sort Verification Result
        /// </summary>
        public ServiceVerificationResult AscSortVerResult = null;

        /// <summary>
        /// Desc Sort Verification Result
        /// </summary>
        public ServiceVerificationResult DescSortVerResult = null;

        /// <summary>
        /// Search Verification Result
        /// </summary>
        public ServiceVerificationResult SearchVerResult = null;

        /// <summary>
        /// Cross Join Verification Result
        /// </summary>
        public ServiceVerificationResult CrossJoinVerResult = null;

        /// <summary>
        /// Select Verification Result
        /// </summary>
        public ServiceVerificationResult SelectResult = null;

        /// <summary>
        /// Expand Verification Result
        /// </summary>
        public ServiceVerificationResult ExpandResult = null;

        /// <summary>
        /// Top Verification Result
        /// </summary>
        public ServiceVerificationResult TopResult = null;

        /// <summary>
        /// Count Verification Result
        /// </summary>
        public ServiceVerificationResult CountResult = null;

        /// <summary>
        /// Skip Verification Result
        /// </summary>
        public ServiceVerificationResult SkipResult = null;
    }
}
