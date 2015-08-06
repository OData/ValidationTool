// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public class ValidationElement
    {
        /// <summary>
        /// Request element for validation
        /// </summary>
        private RequestElement requestEle;
        public RequestElement RequestEle
        {
            get
            {
                return this.requestEle;
            }
            private set
            {
                this.requestEle = null;
            }
        }

        /// <summary>
        /// Expected passed rule list which is to be verified
        /// </summary>
        private List<string> passRuleList;
        public List<string> PassRuleList
        {
            get
            {
                return this.passRuleList;
            }
            private set
            {
                this.passRuleList = null;
            }
        }

        /// <summary>
        /// Expected negative rule list which is not to be verified
        /// </summary>
        private Dictionary<List<string>, string> negativeRuleDic;
        public Dictionary<List<string>, string> NegativeRuleDic
        {
            get
            {
                return this.negativeRuleDic;
            }
            private set
            {
                this.negativeRuleDic = null;
            }
        }

        public ValidationElement(RequestElement reqEle, List<string> passRules, Dictionary<List<string>, string> negativeRulesWithResultType = null)
        {
            this.requestEle = reqEle;
            this.passRuleList = passRules;
            this.negativeRuleDic = negativeRulesWithResultType;
        }
    }
}
