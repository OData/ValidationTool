// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using ODataValidator.RuleEngine.Common;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Delegate type for running semantic extension verification
    /// </summary>
    /// <param name="context">The Interop service context</param>
    /// <param name="info">out parameter to return vialoadtion information</param>
    /// <returns>The flag indicating wheter verifaction passes or not. Null if not applicable.</returns>
    internal delegate bool? ExtensionVerify(ServiceContext context, out ExtensionRuleViolationInfo info);

    /// <summary>
    /// Class of semantic extension rule verifier
    /// </summary>
    internal class ExtensionVerifier : IVerifier
    {
        /// <summary>
        /// The delegate object to semantic extension verification
        /// </summary>
        private ExtensionVerify semanticExtensionVerifier;

        /// <summary>
        /// Creates an instance of SemanticVerifier from an ExtensionVerify delegate object 
        /// </summary>
        /// <param name="extensionVerifier">The ExtensionVerify delegate object</param>
        public ExtensionVerifier(ExtensionVerify extensionVerifier)
        {
            this.semanticExtensionVerifier = extensionVerifier;
        }

        /// <summary>
        /// Verifies whether the specific interop request context pass the validation or not
        /// </summary>
        /// <param name="context">Current interop request context</param>
        /// <param name="result">Output parameter of TestResult object</param>
        /// <returns>True if passed; false if failed</returns>
        public bool Verify(ServiceContext context, out TestResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? flagExec;
            ExtensionRuleViolationInfo info;
            flagExec = this.semanticExtensionVerifier(context, out info);
            bool passed = flagExec.HasValue ? flagExec.Value : true;
            result = new TestResult();

            if (info != null)
            {
                result.ErrorDetail = info.Message;
                result.LineNumberInError = info.PayloadLineNumberInError;
                if (info.Endpoint != null)
                {
                    result.TextInvolved = info.Endpoint.AbsoluteUri;
                }

                if (info.Details != null)
                {
                    result.Details = new List<ExtensionRuleResultDetail>();

                    foreach (ExtensionRuleResultDetail detail in info.Details)
                    {
                        result.Details.Add(detail.Clone());
                    }
                }
            }

            if (!flagExec.HasValue)
            {
                result.Classification = Constants.ClassificationNotApplicable;
            }
            
            return passed;
        }
    }
}
