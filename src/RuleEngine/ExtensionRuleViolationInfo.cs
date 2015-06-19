// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Class of semantic extension rule violating information
    /// </summary>
    public sealed class ExtensionRuleViolationInfo
    {
        /// <summary>
        /// Creates an instance of SemanticRuleViolationInfo from message, uri and content
        /// </summary>
        /// <param name="message">Detail message of violation</param>
        /// <param name="uri">Uri of the interesting OData endpoint</param>
        /// <param name="content">Text of payload or other involved content</param>
        public ExtensionRuleViolationInfo(string message, Uri uri, string content)
            : this(message, uri, content, -1)
        {
        }

        public ExtensionRuleViolationInfo(string message, Uri uri, string content, int lineNumInError)
        {
            this.Message = message;
            this.Endpoint = uri;
            this.Content = content;
            this.PayloadLineNumberInError = lineNumInError;
        }

        public ExtensionRuleViolationInfo(string message, Uri uri, string content, List<ExtensionRuleResultDetail> details, int lineNumInError = -1)
        {
            this.Message = message;
            this.Endpoint = uri;
            this.Content = content;
            this.Details = new List<ExtensionRuleResultDetail>();

            foreach (ExtensionRuleResultDetail detail in details)
            {
                this.Details.Add(detail.Clone());
            }

            this.PayloadLineNumberInError = lineNumInError;
        }

        public ExtensionRuleViolationInfo(Uri uri, string content, List<ExtensionRuleResultDetail> details)
        {
            this.Endpoint = uri;
            this.Content = content;
            this.Details = new List<ExtensionRuleResultDetail>();

            foreach (ExtensionRuleResultDetail detail in details)
            {
                this.Details.Add(detail.Clone());
            }
        }

        public ExtensionRuleViolationInfo(Uri uri, string content, ExtensionRuleResultDetail detail)
        {
            this.Endpoint = uri;
            this.Content = content;
            this.Details = new List<ExtensionRuleResultDetail>();
            if (detail != null)
            {
                this.Details.Add(detail.Clone());
            }
        }

        /// <summary>
        /// Add detail information to extension info.
        /// </summary>
        /// <param name="detail"></param>
        public void AddDetail(ExtensionRuleResultDetail detail)
        {
            this.Details.Add(detail);
        }

        /// <summary>
        /// set element's rulename in details
        /// </summary>
        /// <param name="Name"></param>
        public void SetDetailsName(string Name)
        {
            if (this.Details == null) return;
            foreach (var detail in Details)
            {
                detail.RuleName = Name;
            }
        }
        /// <summary>
        /// Gets the detail message of rule violation
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the payload or other relevant content 
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the interesting Uri being used 
        /// </summary>
        public Uri Endpoint { get; private set; }

        /// <summary>
        /// Gets the number of line of payload where validation had issues
        /// </summary>
        public int PayloadLineNumberInError { get; private set; }
        /// <summary>
        /// Gets the detail information of the validation.
        /// </summary>
        public List<ExtensionRuleResultDetail> Details { get; private set; }
    }
}
