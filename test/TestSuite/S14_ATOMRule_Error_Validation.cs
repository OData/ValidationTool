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
    public class S14_ATOMRule_Error_Validation : TestSuiteBase
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

        protected override void TestInitialize()
        {
            dataService = new AtomDataService();
            base.TestInitialize();
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
        /// Test case to Verify ServiceDocument Rules
        /// </summary>
        [TestMethod]
        public void S14_TC01_ATOMV4Rule_VerifyError()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Entity Reference rule list
                "Error.Core.4600", "Error.Core.4601", "Error.Core.4602", "Error.Core.4603",
                "Error.Core.4605", "Error.Core.4607", "Error.Core.4609", "Error.Core.4612",
                "Error.Core.4613", "Error.Core.4611", "Error.Core.4614", "Error.Core.4610",
                "Error.Core.4606",

                // Common rule list
                "Common.Core.4643", "Common.Core.4648",
                "Common.Core.4649", "Common.Core.4650", "Common.Core.4651",
                "Common.Core.4653", "Common.Core.4654", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Common.Core.4617", "Common.Core.4644" }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4656", }, ValidationResultConstants.Warning);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_Error, FormatConstants.FormatXml), expectedRuleList, negativeDic),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
