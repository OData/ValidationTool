// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// The DefaultValueAccessor class.
    /// </summary>
    public class AnnotationDVsManager
    {
        /// <summary>
        /// Return a singleton accessor for accessing the default value.
        /// </summary>
        /// <returns>Returns the default value accessor instance.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static AnnotationDVsManager Instance()
        {
            return null != defaultValAccessor ? defaultValAccessor : null;
        }

        /// <summary>
        /// Return a singleton accessor for accessing the default value.
        /// </summary>
        /// <param name="svcRootURL">The service root URL.</param>
        /// <returns>Returns the default value accessor instance.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static AnnotationDVsManager Instance(string svcRootURL)
        {
            if (null == defaultValAccessor)
            {
                defaultValAccessor = new AnnotationDVsManager(svcRootURL);
            }

            return defaultValAccessor;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>Returns the default value.</returns>
        public object GetDefaultValue(string propertyName)
        {
            JObject dvs = null;

            dvs = defaultValAccessor.defaultValueData.ContainsKey(this.serviceRootURL) ?
                defaultValAccessor.defaultValueData[this.serviceRootURL] : defaultValAccessor.defaultValueData["Default"];

            if ("SearchRestrictions_UnsupportedExpressions" == propertyName)
            {
                return (int)dvs[propertyName];
            }
            else
            {
                if (null != dvs[propertyName])
                {
                    return (bool)dvs[propertyName];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The data path of the config file which is store the default value.
        /// </summary>
        private static readonly string DataPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + @"\config\DefaultValueRecords.json";

        /// <summary>
        /// The accessor of default value.
        /// </summary>
        private static AnnotationDVsManager defaultValAccessor;

        /// <summary>
        /// The service root URL.
        /// </summary>
        private string serviceRootURL;

        /// <summary>
        /// The default value data.
        /// </summary>
        private Dictionary<string, JObject> defaultValueData;

        /// <summary>
        /// The default value set.
        /// </summary>
        private DefaultValue defaultVal;

        /// <summary>
        /// The constructor of the class DefaultValueAccessor.
        /// </summary>
        /// <param name="svcRootURL">The servie root URL.</param>
        private AnnotationDVsManager(string svcRootURL)
        {
            var records = Read();
            this.defaultValueData = new Dictionary<string, JObject>();

            foreach (var record in records)
            {
                if (null != record["ServiceRoot"] && JTokenType.Object == record["DefaultValue"].Type)
                {
                    this.defaultValueData.Add(record["ServiceRoot"].ToString(), record["DefaultValue"] as JObject);
                }
            }

            if (Uri.IsWellFormedUriString(svcRootURL, UriKind.Absolute))
            {
                this.serviceRootURL = RemoveSession(svcRootURL);
                
                if (!this.defaultValueData.Any())
                {
                    throw new Exception("The default value config file is corrupt.");
                }

                JObject appropriateData = this.defaultValueData.ContainsKey(svcRootURL) ? 
                    this.defaultValueData[svcRootURL] : this.defaultValueData["Default"];

                if (null != appropriateData)
                {
                    this.Initialize(appropriateData);
                }
            }
        }

        /// <summary>
        /// Initialize the default value data using the config file context.
        /// </summary>
        /// <param name="appropriateData">The JSON object which stores the default value data.</param>
        private void Initialize(JObject appropriateData)
        {
            JObject defaultData = this.defaultValueData["Default"];

            try
            {
                this.defaultVal.IsLanguageDependent = null != appropriateData["IsLanguageDependent"] ?
                    (bool)appropriateData["IsLanguageDependent"] : (bool)defaultData["IsLanguageDependent"];
                this.defaultVal.DereferenceableIDs = null != appropriateData["DereferenceableIDs"] ?
                    (bool)appropriateData["DereferenceableIDs"] : (bool)defaultData["DereferenceableIDs"];
                this.defaultVal.ConventionalIDs = null != appropriateData["ConventionalIDs"] ?
                    (bool)appropriateData["ConventionalIDs"] : (bool)defaultData["ConventionalIDs"];
                this.defaultVal.Immutable = null != appropriateData["Immutable"] ?
                    (bool)appropriateData["Immutable"] : (bool)defaultData["Immutable"];
                this.defaultVal.Computed = null != appropriateData["Computed"] ?
                    (bool)appropriateData["Computed"] : (bool)defaultData["Computed"];
                this.defaultVal.IsURL = null != appropriateData["IsURL"] ?
                    (bool)appropriateData["IsURL"] : (bool)defaultData["IsURL"];
                this.defaultVal.IsMediaType = null != appropriateData["IsMediaType"] ?
                    (bool)appropriateData["IsMediaType"] : (bool)defaultData["IsMediaType"];
                this.defaultVal.AsynchronousRequestsSupported = null != appropriateData["AsynchronousRequestsSupported"] ?
                    (bool)appropriateData["AsynchronousRequestsSupported"] : (bool)defaultData["AsynchronousRequestsSupported"];
                this.defaultVal.BatchContinueOnErrorSupported = null != appropriateData["BatchContinueOnErrorSupported"] ?
                    (bool)appropriateData["BatchContinueOnErrorSupported"] : (bool)defaultData["BatchContinueOnErrorSupported"];
                this.defaultVal.CrossJoinSupported = null != appropriateData["CrossJoinSupported"] ?
                    (bool)appropriateData["CrossJoinSupported"] : (bool)defaultData["CrossJoinSupported"];
                this.defaultVal.IndexableByKey = null != appropriateData["IndexableByKey"] ?
                    (bool)appropriateData["IndexableByKey"] : (bool)defaultData["IndexableByKey"];
                this.defaultVal.TopSupported = null != appropriateData["TopSupported"] ?
                    (bool)appropriateData["TopSupported"] : (bool)defaultData["TopSupported"];
                this.defaultVal.SkipSupported = null != appropriateData["SkipSupported"] ?
                    (bool)appropriateData["SkipSupported"] : (bool)defaultData["SkipSupported"];
                this.defaultVal.BatchSupported = null != appropriateData["BatchSupported"] ?
                    (bool)appropriateData["BatchSupported"] : (bool)defaultData["BatchSupported"];
                this.defaultVal.ChangeTrackingType_Supported = null != appropriateData["ChangeTrackingType_Supported"] ?
                    (bool)appropriateData["ChangeTrackingType_Supported"] : (bool)defaultData["ChangeTrackingType_Supported"];
                this.defaultVal.CountRestrictionsType_Countable = null != appropriateData["CountRestrictionsType_Countable"] ?
                    (bool)appropriateData["CountRestrictionsType_Countable"] : (bool)defaultData["CountRestrictionsType_Countable"];
                this.defaultVal.FilterRestrictionsType_Filterable = null != appropriateData["FilterRestrictionsType_Filterable"] ?
                    (bool)appropriateData["FilterRestrictionsType_Filterable"] : (bool)defaultData["FilterRestrictionsType_Filterable"];
                this.defaultVal.SortRestrictionsType_Sortable = null != appropriateData["SortRestrictionsType_Sortable"] ?
                    (bool)appropriateData["SortRestrictionsType_Sortable"] : (bool)defaultData["SortRestrictionsType_Sortable"];
                this.defaultVal.ExpandRestrictionsType_Expandable = null != appropriateData["ExpandRestrictionsType_Expandable"] ?
                    (bool)appropriateData["ExpandRestrictionsType_Expandable"] : (bool)defaultData["ExpandRestrictionsType_Expandable"];
                this.defaultVal.SearchRestrictionsType_Searchable = null != appropriateData["SearchRestrictionsType_Searchable"] ?
                    (bool)appropriateData["SearchRestrictionsType_Searchable"] : (bool)defaultData["SearchRestrictionsType_Searchable"];
                this.defaultVal.SearchRestrictionsType_UnsupportedExpressions = null != appropriateData["SearchRestrictionsType_UnsupportedExpressions"] ?
                    (SearchExpression)(int)appropriateData["SearchRestrictionsType_UnsupportedExpressions"] : (SearchExpression)(int)defaultData["SearchRestrictionsType_UnsupportedExpressions"];
                this.defaultVal.InsertRestrictionsType_Insertable = null != appropriateData["InsertRestrictionsType_Insertable"] ?
                    (bool)appropriateData["InsertRestrictionsType_Insertable"] : (bool)defaultData["InsertRestrictionsType_Insertable"];
                this.defaultVal.UpdateRestrictionsType_Updatable = null != appropriateData["UpdateRestrictionsType_Updatable"] ?
                    (bool)appropriateData["UpdateRestrictionsType_Updatable"] : (bool)defaultData["UpdateRestrictionsType_Updatable"];
                this.defaultVal.DeleteRestrictionsType_Deletable = null != appropriateData["DeleteRestrictionsType_Deletable"] ?
                    (bool)appropriateData["DeleteRestrictionsType_Deletable"] : (bool)defaultData["DeleteRestrictionsType_Deletable"];
            }
            catch (Exception ex)
            {
                throw new Exception("Convert to the specifed type failed.", ex);
            }
        }

        /// <summary>
        /// Read the default value data from a config file.
        /// </summary>
        /// <returns>Return the default value data records.</returns>
        private JArray Read()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(DataPath))
                {
                    string data = streamReader.ReadToEnd();
                    return JArray.Parse(data);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Please check whether providing an invalid config file or configuring some invalid values in the config file.", ex);
            }
        }

        /// <summary>
        /// Remove the Session segment from the service root URL. 
        /// </summary>
        /// <param name="svcRootURL">The service root URL.</param>
        /// <returns>Returns the service root URL without session ID segment.</returns>
        private string RemoveSession(string svcRootURL)
        {
            if (string.IsNullOrEmpty(svcRootURL))
            {
                return string.Empty;
            }

            int startIndex;
            string sessionSegment = GetSessionSegment(svcRootURL, out startIndex);

            if (0 > startIndex || string.IsNullOrEmpty(sessionSegment))
            {
                return svcRootURL;
            }

            return svcRootURL.Remove(startIndex, sessionSegment.Length);
        }

        /// <summary>
        /// Get the session ID segment from service root URL.
        /// </summary>
        /// <param name="svcRootURL">The service root URL.</param>
        /// <param name="startIndex">The start index of the session ID segment.</param>
        /// <returns>Return the session ID segment as a string.</returns>
        private string GetSessionSegment(string svcRootURL, out int startIndex)
        {
            startIndex = -1;
            string result = string.Empty;

            if (string.IsNullOrEmpty(svcRootURL) || !svcRootURL.Contains("/(S(") || !svcRootURL.Contains("))"))
            {
                return result;
            }

            startIndex = svcRootURL.IndexOf("/(S(");
            int endIndex = svcRootURL.IndexOf("))") + 2;

            if (0 > startIndex || 0 > endIndex)
            {
                return svcRootURL;
            }

            return svcRootURL.Substring(startIndex, endIndex - startIndex);
        }
    }

    /// <summary>
    /// Expressions supported in $search.
    /// </summary>
    public enum SearchExpression : int
    {
        none = 0,
        AND = 1,
        OR = 2,
        NOT = 4,
        phrase = 8,
        group = 16
    }

    /// <summary>
    /// The default value structure.
    /// </summary>
    public struct DefaultValue
    {
        /// <summary>
        /// Properties and terms annotated with this term are language-dependent.
        /// </summary>
        public bool IsLanguageDependent
        {
            get;
            set;
        }

        /// <summary>
        /// Entity-ids are URLs that locate the identified entity.
        /// </summary>
        public bool DereferenceableIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Entity-ids follow OData URL conventions.
        /// </summary>
        public bool ConventionalIDs
        {
            get;
            set;
        }

        /// <summary>
        /// A value for this non-key property can be provided on insert and remains unchanged on update.
        /// </summary>
        public bool Immutable
        {
            get;
            set;
        }

        /// <summary>
        /// A value for this property is generated on both insert and update.
        /// </summary>
        public bool Computed
        {
            get;
            set;
        }

        /// <summary>
        /// Properties and terms annotated with this term MUST contain a valid URL.
        /// </summary>
        public bool IsURL
        {
            get;
            set;
        }

        /// <summary>
        /// Properties and terms annotated with this term MUST contain a valid MIME type.
        /// </summary>
        public bool IsMediaType
        {
            get;
            set;
        }

        /// <summary>
        /// Service supports the asynchronous request preference.
        /// </summary>
        public bool AsynchronousRequestsSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Service supports the continue on error preference.
        /// </summary>
        public bool BatchContinueOnErrorSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Supports cross joins for the entity sets in this container.
        /// </summary>
        public bool CrossJoinSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Supports key values according to OData URL conventions.
        /// </summary>
        public bool IndexableByKey
        {
            get;
            set;
        }

        /// <summary>
        /// Supports $top.
        /// </summary>
        public bool TopSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Supports $skip.
        /// </summary>
        public bool SkipSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Supports $batch requests.
        /// </summary>
        public bool BatchSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Change tracking capabilities of this service or entity set. 
        /// This entity set supports the odata.track-changes preference
        /// </summary>
        public bool ChangeTrackingType_Supported
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on /$count path suffix and $count=true system query option.
        /// Entities can be counted.
        /// </summary>
        public bool CountRestrictionsType_Countable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on $filter expressions.
        /// $filter is supported.
        /// </summary>
        public bool FilterRestrictionsType_Filterable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on $orderby expressions.
        /// $orderby is supported.
        /// </summary>
        public bool SortRestrictionsType_Sortable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on $expand expressions.
        /// $expand is supported.
        /// </summary>
        public bool ExpandRestrictionsType_Expandable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on $search expressions.
        /// $search is supported.
        /// </summary>
        public bool SearchRestrictionsType_Searchable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on $search expressions.
        /// Expressions supported in $search.
        /// </summary>
        public SearchExpression SearchRestrictionsType_UnsupportedExpressions
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on insert operations.
        /// Entities can be inserted.
        /// </summary>
        public bool InsertRestrictionsType_Insertable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on update operations.
        /// Entities can be updated.
        /// </summary>
        public bool UpdateRestrictionsType_Updatable
        {
            get;
            set;
        }

        /// <summary>
        /// Restrictions on delete operations.
        /// Entities can be deleted.
        /// </summary>
        public bool DeleteRestrictionsType_Deletable
        {
            get;
            set;
        }
    }
}
