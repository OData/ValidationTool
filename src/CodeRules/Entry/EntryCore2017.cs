// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Abstract Base Class of entension rule for Entry.Core.2017
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2017 : ExtensionRule
    {
        /// <summary>
        /// Gets Categpry property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Entry.Core.2017";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"Each NavigationProperty defined on the EntityType MUST be formatted according to the Navigation Property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.3";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Gets the flag whether it requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verify the rule
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out paramater to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            var qs = HttpUtility.ParseQueryString(context.Destination.Query);
            string qExpand = qs["$expand"];
            var branches = ResourcePathHelper.GetBranchedSegments(qExpand);
            var brHeaders = from br in branches select br.FirstOrDefault();

            // to get collection of navigatio properties which is not expanded
            List<string> navProps = new List<string>();
            XElement meta = XElement.Parse(context.MetadataDocument);
            const string fmtXPathToTypeProperty = "//*[local-name()='Schema' and @Namespace='{0}']/*[local-name()='EntityType' and @Name='{1}']/*[local-name()='NavigationProperty' and @Name]";
            string xpath = string.Format(fmtXPathToTypeProperty, ResourcePathHelper.GetNamespaceName(context.EntityTypeFullName), ResourcePathHelper.GetBaseName(context.EntityTypeFullName));
            var nodes = meta.XPathSelectElements(xpath);
            foreach (var n in nodes)
            {
                var name = ResourcePathHelper.GetBaseName(n.Attribute("Name").Value);
                if (!brHeaders.Any(x => x.Equals(name, StringComparison.Ordinal)))
                {
                    navProps.Add(name);
                }
            }

            if (navProps.Count() > 0)
            {
                JObject jo = JObject.Parse(context.ResponsePayload);
                var version = JsonParserHelper.GetPayloadODataVersion(jo);

                var propSchemas = navProps.Select(x => string.Format(fmtCore, x));
                string coreProperties = string.Join("\r\n,", propSchemas);
                string CoreObject = string.Format(fmtObj, coreProperties);
                string jSchema = JsonSchemaHelper.WrapJsonSchema(CoreObject, version);

                TestResult result;
                passed = JsonParserHelper.ValidateJson(jSchema, context.ResponsePayload, out result);
                if (!passed.HasValue)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result != null ? result.LineNumberInError : -1);
                }
            }

            return passed;
        }

        private const string fmtCore = @"
""{0}"" : 
{{ ""type"" : ""object"", ""required"" : true, 
   ""properties"" : {{ 
        ""__deferred"" : {{ ""type"" : ""object"", ""required"" : true, ""properties"" : {{ ""uri"" : {{ ""type"" : ""string"", ""required"" : true }} }} }} 
   }} 
}}";

        private const string fmtObj = @"
{{
			""type"":""object""
			,""properties"" : {{
                {0} 
            }}
}}";
    }
}

