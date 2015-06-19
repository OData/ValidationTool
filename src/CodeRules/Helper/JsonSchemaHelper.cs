// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    /// <summary>
    /// Helper class to process Json schema
    /// </summary>
    static class JsonSchemaHelper
    {
        /// <summary>
        /// Gets Json schema for specific node along the path segment sequence
        /// </summary>
        /// <param name="paths">The path segment sequence</param>
        /// <param name="i">The index of segment under concern</param>
        /// <param name="version">OData version for which the schema to generate</param>
        /// <param name="jsCore">The Json schema defintion for the indexed segement</param>
        /// <param name="flagOfArray">The supplementary collection of flags indicating segment is array or object</param>
        /// <returns>The Json schema generated for the segments from top till the indexed segment</returns>
        public static string GetJsonSchema(string[] paths, int i, ODataVersion version, string jsCore, bool[] flagOfArray)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                if (!flagOfArray[j])
                {
                    jsCore = string.Format(JsonSchemaHelper.fmtContainerAsObject, paths[j], jsCore);
                }
                else
                {
                    jsCore = string.Format(JsonSchemaHelper.fmtContainerAsArray, paths[j], jsCore);
                }
            }

            return WrapObjectWithProperties(jsCore, version);
        }

        /// <summary>
        /// Wraps Json schema for object from the properties' constraints
        /// </summary>
        /// <param name="jsProperties">The properties' constraints</param>
        /// <param name="version">OData version for this Json schema</param>
        /// <returns>The Json schema generated</returns>
        static string WrapObjectWithProperties(string jsProperties, ODataVersion version)
        {
            string jsObj = string.Format(schemaOfObject, jsProperties);
            return WrapJsonSchema(jsObj, version);
        }

        /// <summary>
        /// Wraps Json schema with proper version specific data
        /// </summary>
        /// <param name="js">The Json schema to be wrapped</param>
        /// <param name="version">The OData version</param>
        /// <returns>The wrapped Json schema</returns>
        public static string WrapJsonSchema(string js, ODataVersion version)
        {
            string result = null;
            switch (version)
            {
                case ODataVersion.V1:
                    result = string.Format(wrapperV1, js);
                    break;
                case ODataVersion.V2:
                    result = string.Format(wrapperV2, js);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Gets the Json schema (without the version wrapping) for the specified entity type
        /// </summary>
        /// <param name="entityType">The entity type name</param>
        /// <param name="metadata">The metadata document</param>
        /// <returns>The JSon schema generated</returns>
        public static string GetSchemaForEntityType(string entityType, string metadata)
        {
            const string fmtCore = @"""{0}"" : {{""type"":""any"", ""required"" : true }}";
            var props = XmlHelper.GetProperties(metadata, entityType);
            var schemaForProps = props.Select(x => string.Format(fmtCore, x));
            return string.Format(JsonSchemaHelper.schemaOfObject, string.Join("\r\n,", schemaForProps));
        }

        /// <summary>
        /// Get specified property name from an entry.
        /// </summary>
        /// <param name="entry">An entry.</param>
        /// <param name="partialPropertyName">A partial property name.</param>
        /// <returns>A list of results.</returns>
        public static IEnumerable<string> GetAllSpecifiedPropertyNameFromEntry(JObject entry, string partialPropertyName)
        {
            List<string> result = new List<string>();

            if (entry != null && entry.Type == JTokenType.Object)
            {
                // Get all the properties in current entry.
                var jProps = entry.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (jProp.Name.Contains(partialPropertyName))
                    {
                        result.Add(jProp.Name);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Judge whether the name is an annotation name.
        /// </summary>
        /// <param name="propName">Indicate the entity's property name.</param>
        /// <returns>Returns the boolean result.</returns>
        public static bool IsAnnotation(string propName)
        {
            return propName.Contains(".") ? true : false;
        }

        /// <summary>
        /// Judge whether the property name is a JSON array or primitive annotation.
        /// </summary>
        /// <param name="propName">Indicate the entity's property name.</param>
        /// <returns>Returns a boolean result.</returns>
        public static bool IsJSONArrayOrPrimitiveAnnotation(string propName)
        {
            return IsAnnotation(propName) && propName.Contains(Constants.V4OdataType) && !propName.StartsWith("@") ? true : false;
        }

        /// <summary>
        /// Whether the property is built-in primitive types
        /// </summary>
        /// <param name="jProp">The JProperty</param>
        /// <param name="context">The Interop service context</param>
        public static bool IsBuiltInPrimitiveTypes(JProperty jProp, ServiceContext context)
        {
            bool isBuiltInPrimitiveTypes = false;
            List<string> appropriateProperty = new List<string>();
            string primitiveTypeXpath = @"//*[local-name()='EntityType']/*[local-name()='Property']";

            // Use the XPath query language to access the metadata document and get all specified value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var properties = metadata.XPathSelectElements(primitiveTypeXpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                // Whether the specified property exist.
                if (property.Attribute("Type") != null && property.Attribute("Type").Value.Contains("Edm."))
                {
                    // Get the Type attribute value and convert its value to string.
                    string specifiedValue = property.Attribute("Name").Value;
                    appropriateProperty.Add(specifiedValue);
                }
            }

            if (appropriateProperty.Count != 0)
            {
                // Verify the annoatation start with namespace.
                foreach (string currentvalue in appropriateProperty)
                {
                    // Whether the property is built-in type.
                    if (jProp.Name.Contains(currentvalue))
                    {
                        isBuiltInPrimitiveTypes = true;
                    }
                }
            }

            return isBuiltInPrimitiveTypes;
        }
        #region Boilplates of Json schema

        const string wrapperV1 = @"
{{
	""type"" : ""object""
	,""patternProperties"" : {{
		"".+"": {0}	
    }}
}}";
        const string wrapperV2 = @"
{{
    ""type"": ""object"",
    ""patternProperties"": {{
        "".+"": {{
            ""type"": ""object"",
            ""properties"": {{
                ""result"": {0}
            }}
        }}
    }}
}}";

        const string schemaOfObject = @"
{{
    ""type"": ""object"",
	""properties"" : {{ {0} }}
}}";

        const string fmtContainerAsObject = @"""{0}"" : {{
	""type"": ""object""
	,""properties"" : {{
        {1}
	}}
}}";

        const string fmtContainerAsArray = @"""{0}"" : {{
	""type"": ""array""
    ,""items"" : {{
        ""type"" : ""object""
	    ,""properties"" : {{
            {1}
        }}
	}}
}}";

        public const string fmtCoreIsArray = @"""{0}"" : {{""type"":""array"", ""required"" : true }}";

        public const string jsonV2pivot = @"{
  ""type"": ""object"",
  ""properties"": {
        ""d"": {
            ""type"": ""object"",
            ""properties"": { ""result"": { ""type"": ""any"", ""required"" : true } }
        }
   }
}";
        #endregion
    }
}
