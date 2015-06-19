// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.100904
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100904 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100904";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.4. SHOULD support $orderby asc and desc on individual properties (section 11.2.4.2.1) ";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();
            ExtensionRuleViolationInfo info1 = null, info2 = null;

            bool? isVerifySortEntitiesOrderByAsc = VerificationHelper.VerifySortEntities(context, SortedType.ASC, out info1);
            bool? isVerifySortEntitiesOrderByDesc = VerificationHelper.VerifySortEntities(context, SortedType.DESC, out info2);
            if (info1 != null) 
            { 
                details.AddRange(info1.Details);
            }
            if (info2 != null) 
            { 
                details.AddRange(info2.Details); 
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            info.SetDetailsName(this.Name);

            return true == isVerifySortEntitiesOrderByAsc && true == isVerifySortEntitiesOrderByDesc;
        }
    }
}
