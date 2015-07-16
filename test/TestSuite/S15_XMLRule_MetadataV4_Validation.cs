// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    /// <summary>
    /// This class contains test cases for creating objects.
    /// </summary>
    [TestClass]
    public class S15_XMLRule_MetadataV4_Validation : TestSuiteBase
    {
        #region Class Initialization and Cleanup
        /// <summary>
        /// Initialize class fields before running any test case.
        /// </summary>
        /// <param name = "context">
        /// Used to store information that is provided to unit tests.
        /// </param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestSuiteBase.TestSuiteClassInitialize(context);
            TestSuiteBase.isVerifyMetadata = "yes";
        }

        /// <summary>
        /// Cleanup class fields after running all test cases.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestSuiteBase.isVerifyMetadata = "no";
            TestSuiteBase.TestSuiteClassCleanup();
        }
        #endregion

        #region Test Cases
        /// <summary>
        /// Test case to verify Metadata version4 XML Rules.
        /// </summary>
        [TestMethod]
        public void S15_TC01_XMLRule_VerifyMetadataV4()
        {

            List<string> expectedRuleList = new List<string>()
            {
                "Metadata.Core.4000",
                "Metadata.Core.4001",
                "Metadata.Core.4003",
                "Metadata.Core.4004",
                "Metadata.Core.4005",
                "Metadata.Core.4006",
                "Metadata.Core.4007",
                "Metadata.Core.4008",
                "Metadata.Core.4009",
                "Metadata.Core.4013",
                "Metadata.Core.4014",
                "Metadata.Core.4015",
                "Metadata.Core.4018",
                "Metadata.Core.4019",
                "Metadata.Core.4020",
                "Metadata.Core.4021",
                "Metadata.Core.4024",
                "Metadata.Core.4025",
                "Metadata.Core.4027",
                "Metadata.Core.4028",
                "Metadata.Core.4029",
                "Metadata.Core.4030",
                "Metadata.Core.4031",
                "Metadata.Core.4032",
                "Metadata.Core.4034",
                "Metadata.Core.4035",
                "Metadata.Core.4037",
                "Metadata.Core.4039",
                "Metadata.Core.4040",
                "Metadata.Core.4043",
                "Metadata.Core.4045",
                "Metadata.Core.4046",
                "Metadata.Core.4047",
                "Metadata.Core.4049",
                "Metadata.Core.4051",
                "Metadata.Core.4052",
                "Metadata.Core.4053",
                "Metadata.Core.4054",
                "Metadata.Core.4055",
                "Metadata.Core.4056",
                "Metadata.Core.4057",
                "Metadata.Core.4058",
                "Metadata.Core.4074",
                "Metadata.Core.4076",
                "Metadata.Core.4085",
                "Metadata.Core.4092",
                "Metadata.Core.4095",
                "Metadata.Core.4098",
                "Metadata.Core.4100",
                "Metadata.Core.4101",
                "Metadata.Core.4102",
                "Metadata.Core.4103",
                "Metadata.Core.4111",
                "Metadata.Core.4113",
                "Metadata.Core.4114",
                "Metadata.Core.4117",
                "Metadata.Core.4120",
                "Metadata.Core.4121",
                "Metadata.Core.4125",
                "Metadata.Core.4127",
                "Metadata.Core.4128",
                "Metadata.Core.4129",
                "Metadata.Core.4130",
                "Metadata.Core.4132",
                "Metadata.Core.4134",
                "Metadata.Core.4138",
                "Metadata.Core.4139",
                "Metadata.Core.4140",
                "Metadata.Core.4141",
                "Metadata.Core.4142",
                "Metadata.Core.4144",
                "Metadata.Core.4145",
                "Metadata.Core.4147",
                "Metadata.Core.4153",
                "Metadata.Core.4154",
                "Metadata.Core.4155",
                "Metadata.Core.4157",
                "Metadata.Core.4159",
                "Metadata.Core.4160",
                "Metadata.Core.4163",
                "Metadata.Core.4166",
                "Metadata.Core.4167",
                "Metadata.Core.4171",
                "Metadata.Core.4178",
                "Metadata.Core.4181",
                "Metadata.Core.4182",
                "Metadata.Core.4186",
                "Metadata.Core.4187",
                "Metadata.Core.4189",
                "Metadata.Core.4190",
                "Metadata.Core.4191",
                "Metadata.Core.4192",
                "Metadata.Core.4195",
                "Metadata.Core.4196",
                "Metadata.Core.4200",
                "Metadata.Core.4201",
                "Metadata.Core.4202",
                "Metadata.Core.4204",
                "Metadata.Core.4211",
                "Metadata.Core.4213",
                "Metadata.Core.4215",
                "Metadata.Core.4217",
                "Metadata.Core.4222",
                "Metadata.Core.4223",
                "Metadata.Core.4224",
                "Metadata.Core.4226",
                "Metadata.Core.4228",
                "Metadata.Core.4232",
                "Metadata.Core.4233",
                "Metadata.Core.4234",
                "Metadata.Core.4235",
                "Metadata.Core.4238",
                "Metadata.Core.4245",
                "Metadata.Core.4247",
                "Metadata.Core.4248",
                "Metadata.Core.4249",
                "Metadata.Core.4254",
                "Metadata.Core.4257",
                "Metadata.Core.4258",
                "Metadata.Core.4259",
                "Metadata.Core.4260",
                "Metadata.Core.4261",
                "Metadata.Core.4262",
                "Metadata.Core.4264",
                "Metadata.Core.4265",
                "Metadata.Core.4267",
                "Metadata.Core.4268",
                "Metadata.Core.4269",
                "Metadata.Core.4270",
                "Metadata.Core.4271",
                "Metadata.Core.4272",
                "Metadata.Core.4273",
                "Metadata.Core.4279",
                "Metadata.Core.4285",
                "Metadata.Core.4286",
                "Metadata.Core.4287",
                "Metadata.Core.4288",
                "Metadata.Core.4289",
                "Metadata.Core.4299",
                "Metadata.Core.4300",
                "Metadata.Core.4301",
                "Metadata.Core.4306",
                "Metadata.Core.4308",
                "Metadata.Core.4309",
                "Metadata.Core.4312",
                "Metadata.Core.4313",
                "Metadata.Core.4315",
                "Metadata.Core.4317",
                "Metadata.Core.4321",
                "Metadata.Core.4322",
                "Metadata.Core.4324",
                "Metadata.Core.4326",
                "Metadata.Core.4327",
                "Metadata.Core.4328",
                "Metadata.Core.4329",
                "Metadata.Core.4330",
                "Metadata.Core.4331",
                "Metadata.Core.4332",
                "Metadata.Core.4333",
                "Metadata.Core.4334",
                "Metadata.Core.4335",
                "Metadata.Core.4336",
                "Metadata.Core.4338",
                "Metadata.Core.4340",
                "Metadata.Core.4341",
                "Metadata.Core.4343",
                "Metadata.Core.4344",
                "Metadata.Core.4345",
                "Metadata.Core.4346",
                "Metadata.Core.4348",
                "Metadata.Core.4349",
                "Metadata.Core.4351",
                "Metadata.Core.4352",
                "Metadata.Core.4353",
                "Metadata.Core.4357",
                "Metadata.Core.4358",
                "Metadata.Core.4360",
                "Metadata.Core.4363",
                "Metadata.Core.4367",
                "Metadata.Core.4368",
                "Metadata.Core.4369",
                "Metadata.Core.4370",
                "Metadata.Core.4371",
                "Metadata.Core.4380",
                "Metadata.Core.4381",
                "Metadata.Core.4382",
                "Metadata.Core.4384",
                "Metadata.Core.4385",
                "Metadata.Core.4388",
                "Metadata.Core.4389",
                "Metadata.Core.4390",
                "Metadata.Core.4392",
                "Metadata.Core.4393",
                "Metadata.Core.4394",
                "Metadata.Core.4396",
                "Metadata.Core.4398",
                "Metadata.Core.4400",
                "Metadata.Core.4402",
                "Metadata.Core.4404",
                "Metadata.Core.4406",
                "Metadata.Core.4408",
                "Metadata.Core.4410",
                "Metadata.Core.4412",
                "Metadata.Core.4416",
                "Metadata.Core.4419",
                "Metadata.Core.4420",
                "Metadata.Core.4421",
                "Metadata.Core.4423",
                "Metadata.Core.4424",
                "Metadata.Core.4425",
                "Metadata.Core.4428",
                "Metadata.Core.4429",
                "Metadata.Core.4436",
                "Metadata.Core.4437",
                "Metadata.Core.4438",
                "Metadata.Core.4441",
                "Metadata.Core.4442",
                "Metadata.Core.4444",
                "Metadata.Core.4446",
                "Metadata.Core.4447",
                "Metadata.Core.4448",
                "Metadata.Core.4449",
                "Metadata.Core.4450",
                "Metadata.Core.4451",
                "Metadata.Core.4453",
                "Metadata.Core.4455",
                "Metadata.Core.4456",
                "Metadata.Core.4461",
                "Metadata.Core.4486",
                "Metadata.Core.4492",
                "Metadata.Core.4494",
                "Metadata.Core.4496",
                "Metadata.Core.4498",
                "Metadata.Core.4504",
                "Metadata.Core.4505",
                "Metadata.Core.4506",
                "Metadata.Core.4508",
                "Metadata.Core.4511",
                "Metadata.Core.4513",
                "Metadata.Core.4517",
                "Metadata.Core.4518",
                "Metadata.Core.4519",
                "Metadata.Core.4520",
                "Metadata.Core.4521",
                "Metadata.Core.4522",
                "Metadata.Core.4534",
                "Metadata.Core.4544",
                "Metadata.Core.4546",
                "Metadata.Core.4559",
                "Metadata.Core.4560",
                "Metadata.Core.4561",
                "Metadata.Core.4564"
            };
            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Metadata.Core.4325" }, ValidationResultConstants.Warning);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Metadata, FormatConstants.FormatXml), expectedRuleList, negativeDic),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }
        #endregion

    }
}
