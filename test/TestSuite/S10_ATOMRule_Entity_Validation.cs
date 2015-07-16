// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Protocols.TestSuites.Validator
{
    /// <summary>
    /// Summary description for S10_ATOMRule_Entity_Validation
    /// </summary>
    [TestClass]
    public class S10_ATOMRule_Entity_Validation : TestSuiteBase
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
        /// Test case to Verify Entry Rules
        /// </summary>
        [TestMethod]
        public void S10_TC01_ATOMV4Rule_VerifyEntity()
        {
            // Bugs need to be fixed.
            // "Entry.Core.4621", "Entry.Core.4622", "Entry.Core.4623", "Entry.Core.4625",

            List<string> expectedRuleListForODataDemoProductExpandAll = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4653", "Entry.Core.4655", "Entry.Core.4656", "Entry.Core.4657", "Entry.Core.4658",

                // Common rule list.
                "Common.Core.4600", "Common.Core.4659", "Common.Core.4657", "Common.Core.4658",
            };

            List<string> expectedRuleListForODataDemoProductXmlFormat = new List<string>()
            {
                // Common rule list.
                "Common.Core.4614", "Common.Core.4664", "Common.Core.4665", "Common.Core.4621", "Common.Core.4622",
                "Common.Core.4623", "Common.Core.4624", "Common.Core.4625", "Common.Core.4626", "Common.Core.4627", 
                "Common.Core.4628", "Common.Core.4631", "Common.Core.4634", "Common.Core.4636", "Common.Core.4637",
                "Common.Core.4638",
            };

            List<string> expectedRuleListForODataDemoProduct = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4601", "Entry.Core.4605", "Entry.Core.4607", "Entry.Core.4615", "Entry.Core.4616", 
                "Entry.Core.4617", "Entry.Core.4650", "Entry.Core.4651", "Entry.Core.4652",
                
                // XML rule list.
                "Entry.Core.4632", "Entry.Core.4633", "Entry.Core.4645", "Entry.Core.4646", "Entry.Core.4648",

                // Common rule list.
                "Common.Core.4615", "Common.Core.4616", "Common.Core.4653", "Common.Core.4654", "Common.Core.4656",

            };

            List<string> recommRuleListForODataDemoProduct = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4609", "Entry.Core.4610", "Entry.Core.4614", "Entry.Core.4644",  
                
                // XML rule list.
                "Entry.Core.4604", "Entry.Core.4628",
            };

            List<string> notAppRuleListForODataDemoProduct = new List<string>()
            {
                // Common rule list.
                "Common.Core.4620",
            };

            List<string> expectedRuleListForODataDemoPersonDetail = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4661", "Entry.Core.4662", "Entry.Core.4664", "Entry.Core.4665", "Entry.Core.4668", 
                "Entry.Core.4669", "Entry.Core.4673", "Entry.Core.4715", 
            };

            List<string> recommRuleListForODataDemoPersonDetail = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4671", 
            };

            List<string> expectedRuleListForTripPinAirport = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4627", "Entry.Core.4638", "Entry.Core.4639", "Entry.Core.4641"
            };

            List<string> expectedRuleListForTripPinPerson = new List<string>()
            {
                // Code rule list.
                "Entry.Core.4620", "Entry.Core.4624", "Entry.Core.4626", "Entry.Core.4629", "Entry.Core.4634",
                "Entry.Core.4637", "Entry.Core.4640", "Entry.Core.4643", "Entry.Core.4647", "Entry.Core.4700", 
                "Entry.Core.4701", "Entry.Core.4659", "Entry.Core.4717", "Entry.Core.4718", "Entry.Core.4621", 
                "Entry.Core.4622", "Entry.Core.4623", "Entry.Core.4625", 
            };

            List<string> expectedRuleListForTripPinPhoto = new List<string>()
            {
                "Entry.Core.4676", "Entry.Core.4677", "Entry.Core.4678", "Entry.Core.4679", "Entry.Core.4681", 
                "Entry.Core.4682", "Entry.Core.4683", "Entry.Core.4684", "Entry.Core.4685", 
            };

            Dictionary<List<string>, string> negativeDicForODataDemoProduct = new Dictionary<List<string>, string>();
            negativeDicForODataDemoProduct.Add(recommRuleListForODataDemoProduct, ValidationResultConstants.Recommendation);
            negativeDicForODataDemoProduct.Add(notAppRuleListForODataDemoProduct, ValidationResultConstants.NotApplicable);

            Dictionary<List<string>, string> negativeDicForODataDemoPersonDetail = new Dictionary<List<string>, string>();
            negativeDicForODataDemoPersonDetail.Add(recommRuleListForODataDemoPersonDetail, ValidationResultConstants.Recommendation);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                // remove RequestHeaderConstants.ContentTypeAtom
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_Entity_Product_ExpandAll, FormatConstants.FormatAtomAbbre), expectedRuleListForODataDemoProductExpandAll, null),

                // remove RequestHeaderConstants.ContentTypeXml
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_Entity_Product_XmlFormat, FormatConstants.FormatXml), expectedRuleListForODataDemoProductXmlFormat, null),
                
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_Entity_Product, FormatConstants.FormatAtomAbbre), expectedRuleListForODataDemoProduct, negativeDicForODataDemoProduct),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_Entity_PersonDetail, FormatConstants.FormatAtomAbbre), expectedRuleListForODataDemoPersonDetail, negativeDicForODataDemoPersonDetail),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_Airports_AtomAbbrFormat, FormatConstants.FormatAtomAbbre), expectedRuleListForTripPinAirport, null),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_AtomAbbrFormat, FormatConstants.FormatAtomAbbre), expectedRuleListForTripPinPerson, null),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_Photos_AtomAbbrFormat, FormatConstants.FormatAtomAbbre), expectedRuleListForTripPinPhoto, null),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }
        #endregion
    }
}
