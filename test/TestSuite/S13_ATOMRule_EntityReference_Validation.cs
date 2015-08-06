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
    public class S13_ATOMRule_EntityReference_Validation : TestSuiteBase
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
        public void S13_TC01_ATOMV4Rule_VerifyEntityReference()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Entity Reference rule list
                "EntityReference.Core.4602", 
                "EntityReference.Core.4605","EntityReference.Core.4606", 

                "Common.Core.4649", "Common.Core.4650", "Common.Core.4651",

                // Common rule list
                "Common.Core.4600",  "Common.Core.4614", "Common.Core.4656",
                "Common.Core.4664", "Common.Core.4665", "Common.Core.4616",
                "Common.Core.4653", "Common.Core.4654", 
            };

            List<string> expectedRuleList_EntityRefCollection = new List<string>()
            {
                // Entity Reference rule list
                "EntityReference.Core.4605","EntityReference.Core.4606",
                "Common.Core.4656",
                

                //Instance Annoation rules
                "Common.Core.4621", "Common.Core.4622", "Common.Core.4623", "Common.Core.4624", 
                "Common.Core.4625", "Common.Core.4626", "Common.Core.4627", "Common.Core.4628", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Common.Core.4617", }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4620", "EntityReference.Core.4601", "EntityReference.Core.4607" }, ValidationResultConstants.NotApplicable);

            Dictionary<List<string>, string> negativeDic_EntityRefCollection = new Dictionary<List<string>, string>();
            negativeDic_EntityRefCollection.Add(new List<string>() { "EntityReference.Core.4604"}, ValidationResultConstants.Recommendation);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_EntityReferenceSingle, FormatConstants.FormatXml), expectedRuleList, negativeDic),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_EntityReferenceCollection, FormatConstants.FormatAtom), expectedRuleList_EntityRefCollection, negativeDic_EntityRefCollection),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
