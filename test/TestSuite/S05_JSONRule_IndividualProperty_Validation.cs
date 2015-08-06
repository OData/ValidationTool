// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This class contains test cases for creating objects.
    /// </summary>
    [TestClass]
    public class S05_JSONRule_IndividualProperty_Validation : TestSuiteBase
    {
        #region Class Initialization and Cleanup

        /// <summary>
        /// Initialize class fields before running any test case.
        /// </summary>
        /// <param name="context">
        /// Used to store information that is provided to unit tests.
        /// </param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestSuiteBase.TestSuiteClassInitialize(context);
        }

        /// <summary>
        /// Cleanup class fields after running all test cases.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestSuiteBase.TestSuiteClassCleanup();
        }

        #endregion

        #region Test Cases

        /// <summary>
        /// Test case to Verify IndividualProperty Rules
        /// </summary>
        [TestMethod]
        public void S05_TC01_JSONV4Rule_VerifyIndividualProperty()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Code rule list
                "IndividualProperty.Core.4200", "IndividualProperty.Core.4201",
                // Common rule list
                "Common.Core.4000", "Common.Core.4004", "Common.Core.4018", "Common.Core.4119", 
                "Common.Core.4122", "Common.Core.4123", "Common.Core.4601",
            };

            List<string> collectionRuleList = new List<string>()
            {
                // Code rule list
                "IndividualProperty.Core.4202"
            };

            List<string> complexRuleList = new List<string>()
            {
                // Code rule list
                "IndividualProperty.Core.4204"
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Common.Core.4006" }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4010", "Common.Core.4700" }, ValidationResultConstants.NotApplicable);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_IndividualProperty_Primitive), expectedRuleList, negativeDic),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_IndividualProperty_Collection), collectionRuleList, null),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_IndividualProperty_Complex), complexRuleList, null)
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
