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
    public class S11_ATOMRule_Feed_Validation : TestSuiteBase
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
        public void S11_TC01_ATOMV4Rule_VerifyFeed()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Feed rule list
                "Feed.Core.4620", "Feed.Core.4600", "Feed.Core.4606", 
                "Feed.Core.4607", "Feed.Core.4608", 
                "Feed.Core.4610", "Feed.Core.4612", "Feed.Core.4621", 

                // Common rule list
                "Common.Core.4600", "Common.Core.4657", "Common.Core.4658",
                "Common.Core.4659", "Common.Core.4614",
                "Common.Core.4664", "Common.Core.4665", "Common.Core.4615", "Common.Core.4616",
                "Common.Core.4653", "Common.Core.4654", "Common.Core.4655",
                "Common.Core.4656",

                //Instance Annoation rules
                "Common.Core.4621", "Common.Core.4622", "Common.Core.4624", "Common.Core.4625",
                "Common.Core.4626", "Common.Core.4627",  "Common.Core.4629","Common.Core.4623", 
                "Common.Core.4630", 
                "Common.Core.4628", "Common.Core.4634", "Common.Core.4636", "Common.Core.4637", 
            };

            List<string> expectedRuleList_Delta = new List<string>()
            {
                // Feed rule list
                "Feed.Core.4613", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Feed.Core.4602", "Feed.Core.4609", "Common.Core.4620", }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Feed.Core.4605",  }, ValidationResultConstants.NotApplicable);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_EntitySet_Products, FormatConstants.FormatAtom), expectedRuleList, negativeDic),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_EntitySet_ProductsSkip, FormatConstants.FormatAtom), expectedRuleList_Delta),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
