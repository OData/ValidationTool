// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.XPath;
    using System.Xml.Linq;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The SupportiveFeatureHelper class.
    /// </summary>
    public static class SupportiveFeatureHelper
    {
        /// <summary>
        /// Validates whether an entity-set supports batch operation or not.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsSupportBatchOperation(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            return entitySetName.IsSupportSpecifiedOperation(metadataDoc, vocDocs, SupportiveFeatureHelper.IsServiceSupportBatchOperation);
        }

        /// <summary>
        /// Validates whether an entity-set supports asynchronous operation or not.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsSupportAsynchronousOperation(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            return entitySetName.IsSupportSpecifiedOperation(metadataDoc, vocDocs, SupportiveFeatureHelper.IsSupportAsynchronousRequests);
        }

        /// <summary>
        /// Validates whether an entity-set supports specified operation or not.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents</param>
        /// <param name="func">The specified function.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsSupportSpecifiedOperation(this string entitySetName, string metadataDoc, List<string> vocDocs, Func<string, string, List<string>, bool?> func)
        {
            string xPath = string.Format("//*[local-name()='EntityContainer']/*[local-name()='EntitySet' and @Name='{0}']", entitySetName);
            XElement metadata = XElement.Parse(metadataDoc);
            var entitySet = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            if (null != entitySet.Parent && entitySet.Parent.Name.LocalName.Equals("EntityContainer"))
            {
                var entityContainer = entitySet.Parent;
                string entityContainerName = entityContainer.GetAttributeValue("Name");

                if (string.IsNullOrEmpty(entityContainerName))
                {
                    return false;
                }

                return func(entityContainerName, metadataDoc, vocDocs);
            }

            return false;
        }

        /// <summary>
        /// Validates whether an entity-set supports $top query or not.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsEntitySetSupportTopQuery(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            return entitySetName.IsEntitySetNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(entitySetName, GetTermInfo("TopSupported", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether an entity-set supports $skip query or not.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsEntitySetSupportSkipQuery(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            return entitySetName.IsEntitySetNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(entitySetName, GetTermInfo("SkipSupported", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether a service supports batch operation or not.
        /// </summary>
        /// <param name="entityContainerName">The entity-contatiner name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsServiceSupportBatchOperation(this string entityContainerName, string metadataDoc, List<string> vocDocs)
        {
            return entityContainerName.IsEntityContainerNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(entityContainerName, GetTermInfo("BatchSupported", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether a service supports asynchronous requests or not.
        /// </summary>
        /// <param name="entityContainerName">The entity-contatiner name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsSupportAsynchronousRequests(this string entityContainerName, string metadataDoc, List<string> vocDocs)
        {
            return entityContainerName.IsEntityContainerNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(entityContainerName, GetTermInfo("AsynchronousRequestsSupported", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether a service supports batch continue on error or not.
        /// </summary>
        /// <param name="entityContainerName">The entity-contatiner name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsSupportBatchContinueOnError(this string entityContainerName, string metadataDoc, List<string> vocDocs)
        {
            return entityContainerName.IsEntityContainerNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(entityContainerName, GetTermInfo("BatchContinueOnErrorSupported", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether a property is immutable or not.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsPropertyImmutable(this string propertyName, string metadataDoc, List<string> vocDocs)
        {
            return propertyName.IsPropertyNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(propertyName, GetTermInfo("Immutable", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Validates whether a property is computed or not.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the validation.</returns>
        public static bool? IsPropertyComputed(this string propertyName, string metadataDoc, List<string> vocDocs)
        {
            return propertyName.IsPropertyNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(propertyName, GetTermInfo("Computed", vocDocs), metadataDoc) :
                null;
        }

        #region Complex type.
        /// <summary>
        /// Get count restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the count restrictions.</returns>
        public static CountRestrictionsType? GetCountRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("CountRestrictions", vocDocs);
                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("CountRestrictionsType", vocDocs), metadataDoc);
                if (null != complexType && "CountRestrictionsType" == complexType.Name)
                {
                    List<string> propPathes = null;
                    List<string> navigPropPathes = null;
                    bool? countable = (bool?)term.DefaultValue;
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Countable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                countable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("NonCountableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                propPathes = new List<string>();
                                propPathes = prop.PropertyValue as List<string>;
                            }
                        }
                        else if ("NonCountableNavigationProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                navigPropPathes = new List<string>();
                                navigPropPathes = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new CountRestrictionsType(countable, propPathes, navigPropPathes);
                }
            }

            return null;
        }

        /// <summary>
        /// Get filter restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the filter restrictions.</returns>
        public static FilterRestrictionsType? GetFilterRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("FilterRestrictions", vocDocs);
                bool? filterable = (bool?)term.DefaultValue;
                bool? requiresFilter = null;
                List<string> reqProps = null;
                List<string> nonFilterableProps = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("FilterRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "FilterRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Filterable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                filterable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("RequiresFilter" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                requiresFilter = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("RequiredProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                reqProps = new List<string>();
                                reqProps = prop.PropertyValue as List<string>;
                            }
                        }
                        else if ("NonFilterableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonFilterableProps = new List<string>();
                                nonFilterableProps = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new FilterRestrictionsType(filterable, requiresFilter, reqProps, nonFilterableProps);
                }
            }

            return null;
        }

        /// <summary>
        /// Get sort restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the sort restrictions.</returns>
        public static SortRestrictionsType? GetSortRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("SortRestrictions", vocDocs);
                bool? sortable = (bool?)term.DefaultValue;
                List<string> ascendingOnlyProperties = null;
                List<string> descendingOnlyProperties = null;
                List<string> nonSortableProperties = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("SortRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "SortRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Sortable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                sortable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("AscendingOnlyProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                ascendingOnlyProperties = new List<string>();
                                ascendingOnlyProperties = prop.PropertyValue as List<string>;
                            }
                        }
                        else if ("DescendingOnlyProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                descendingOnlyProperties = new List<string>();
                                descendingOnlyProperties = prop.PropertyValue as List<string>;
                            }
                        }
                        else if ("NonSortableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonSortableProperties = new List<string>();
                                nonSortableProperties = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new SortRestrictionsType(sortable, ascendingOnlyProperties, descendingOnlyProperties, nonSortableProperties);
                }
            }

            return null;
        }

        /// <summary>
        /// Get expand restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the expand restrictions.</returns>
        public static ExpandRestrictionsType? GetExpandRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("ExpandRestrictions", vocDocs);
                bool? expandable = (bool?)term.DefaultValue;
                List<string> nonExpandableProperties = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("ExpandRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "ExpandRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Expandable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                expandable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("NonExpandableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonExpandableProperties = new List<string>();
                                nonExpandableProperties = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new ExpandRestrictionsType(expandable, nonExpandableProperties);
                }
            }

            return null;
        }

        /// <summary>
        /// Get insert restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the insert restricitons.</returns>
        public static InsertRestrictionsType? GetInsertRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("InsertRestrictions", vocDocs);
                bool? insertable = (bool?)term.DefaultValue;
                List<string> nonInsertableNavigationProperties = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("InsertRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "InsertRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Insertable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                insertable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("NonInsertableNavigationProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonInsertableNavigationProperties = new List<string>();
                                nonInsertableNavigationProperties = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new InsertRestrictionsType(insertable, nonInsertableNavigationProperties);
                }
            }

            return null;
        }

        /// <summary>
        /// Get update restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the update restricitons.</returns>
        public static UpdateRestrictionsType? GetUpdateRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("UpdateRestrictions", vocDocs);
                bool? updatable = (bool?)term.DefaultValue;
                List<string> nonUpdatableNavigationProperties = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("UpdateRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "UpdateRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Updatable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                updatable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("NonUpdatableNavigationProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonUpdatableNavigationProperties = new List<string>();
                                nonUpdatableNavigationProperties = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new UpdateRestrictionsType(updatable, nonUpdatableNavigationProperties);
                }
            }

            return null;
        }

        /// <summary>
        /// Get delete restrictions.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the delete restricitons.</returns>
        public static DeleteRestrictionsType? GetDeleteRestrictions(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("DeleteRestrictions", vocDocs);
                bool? deletable = (bool?)term.DefaultValue;
                List<string> nonDeletableNavigationProperties = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("DeleteRestrictionsType", vocDocs), metadataDoc);

                if (null != complexType && "DeleteRestrictionsType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Deletable" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                deletable = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("NonDeletableNavigationProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                nonDeletableNavigationProperties = new List<string>();
                                nonDeletableNavigationProperties = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new DeleteRestrictionsType(deletable, nonDeletableNavigationProperties);
                }
            }

            return null;
        }

        /// <summary>
        /// Get optimistic concurrency control.
        /// </summary>
        /// <param name="entitySetName">The entity-set short name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the optimistic concurrency control.</returns>
        public static OptimisticConcurrencyControlType? GetOptimisticConcurrencyControl(this string entitySetName, string metadataDoc, List<string> vocDocs)
        {
            if (entitySetName.IsEntitySetNameValid(metadataDoc))
            {
                var term = GetTermInfo("OptimisticConcurrencyControl", vocDocs);
                List<string> eTagDependsOn = null;

                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(entitySetName, term, GetComplexType("OptimisticConcurrencyControlType", vocDocs), metadataDoc);

                if (null != complexType && "OptimisticConcurrencyControlType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("ETagDependsOn" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                eTagDependsOn = new List<string>();
                                eTagDependsOn = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new OptimisticConcurrencyControlType(eTagDependsOn);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get change tracking.
        /// </summary>
        /// <param name="targetName">The target name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the change tracking.</returns>
        public static ChangeTrackingType? GetChangeTranking(this string targetName, string metadataDoc, List<string> vocDocs)
        { 
            if (targetName.IsEntitySetNameValid(metadataDoc) || targetName.IsEntityContainerNameValid(metadataDoc))
            {
                var term = GetTermInfo("ChangeTracking", vocDocs);
                bool? supported = (bool?)term.DefaultValue;
                List<string> filterableProps = null;
                List<string> expandableProps = null;
                
                var complexType = SupportiveFeatureHelper.GetSupportiveFeatureInfo(targetName, term, GetComplexType("ChangeTrackingType", vocDocs), metadataDoc);

                if (null != complexType && "ChangeTrackingType" == complexType.Name)
                {
                    foreach (var prop in complexType.GetProperties())
                    {
                        if ("Supported" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                supported = Convert.ToBoolean(prop.PropertyValue);
                            }
                        }
                        else if ("FilterableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                filterableProps = new List<string>();
                                filterableProps = prop.PropertyValue as List<string>;
                            }
                        }
                        else if ("ExpandableProperties" == prop.PropertyName)
                        {
                            if (null != prop.PropertyValue)
                            {
                                expandableProps = new List<string>();
                                expandableProps = prop.PropertyValue as List<string>;
                            }
                        }
                    }

                    return new ChangeTrackingType(supported, filterableProps, expandableProps);
                }
            }

            return null;
        }

        /// <summary>
        /// Justify the restriction is or not null
        /// </summary>
        /// <param name="restriction">restriction</param>
        /// <param name="isNeedNavProps">If need to justify the restriction has navigation property</param>
        /// <returns>Returns is the restiction is or not null</returns>
        public static bool IsRestrictionNotNull(this Tuple<string, List<NormalProperty>, List<NavigProperty>> restriction, bool isNeedNavProps = false)
        {
            bool isNotNull = true;
            if (null != restriction)
            {
                isNotNull = string.IsNullOrEmpty(restriction.Item1) && null != restriction.Item2 && restriction.Item2.Any();
                if (isNeedNavProps)
                {
                    isNotNull &= (null != restriction.Item3 && restriction.Item3.Any());
                }
            }

            return isNotNull;
        }

        #endregion

        #region Enum type.
        /// <summary>
        /// Gets conformance level of the service.
        /// </summary>
        /// <param name="entityContainerName">The entity-container short name.</param>
        /// <param name="metadataDoc">The metadata documnent.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the conformance level.</returns>
        public static List<KeyValuePair<string, int>> GetServiceConformanceLevel(this string entityContainerName, string metadataDoc, List<string> vocDocs)
        {
            return entityContainerName.IsEntityContainerNameValid(metadataDoc) ?
               GetSupportiveFeatureInfo(entityContainerName, GetTermInfo("ConformanceLevel", vocDocs), GetEnumType("ConformanceLevelType", vocDocs), metadataDoc) :
                null;
        }

        /// <summary>
        /// Gets permissions of the proerty.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the permission.</returns>
        public static List<KeyValuePair<string, int>> GetPropertyPermissions(this string propertyName, string metadataDoc, List<string> vocDocs)
        {
            return propertyName.IsPropertyNameValid(metadataDoc) ?
                GetSupportiveFeatureInfo(propertyName, GetTermInfo("Permissions", vocDocs), GetEnumType("Permission", vocDocs), metadataDoc) :
                null;
        }
        #endregion

        #region Get type structures from vocabulary document.
        /// <summary>
        /// Gets the information of a term element in metadata.
        /// </summary>
        /// <param name="termShortName">The term short name.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the term information.</returns>
        public static TermElement GetTermInfo(string termShortName, List<string> vocDocs)
        {
            XElement termElement = null;

            foreach (var doc in vocDocs)
            {
                var termRestricions = XElement.Parse(doc);
                string xPath = string.Format("//*[local-name()='Term' and @Name='{0}']", termShortName);
                termElement = termRestricions.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != termElement)
                {
                    break;
                }
            }

            return null != termElement ? TermElement.Parse(termElement, MetadataHelper.GetAliasAndNamespace) : null;
        }

        /// <summary>
        /// Gets the information of a complex type element in metadata.
        /// </summary>
        /// <param name="complexTypeShortName">The complex type short name.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the complex type information.</returns>
        public static ComplexTypeElement GetComplexType(string complexTypeShortName, List<string> vocDocs)
        {
            XElement complexTypeElement = null;

            foreach (var doc in vocDocs)
            {
                var termRestrictions = XElement.Parse(doc);
                string xPath = string.Format("//*[local-name()='ComplexType' and @Name='{0}']", complexTypeShortName);
                complexTypeElement = termRestrictions.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != complexTypeElement)
                {
                    break;
                }
            }

            return null != complexTypeElement ? ComplexTypeElement.Parse(complexTypeElement) : null;
        }

        /// <summary>
        /// Gets the information of a complex type element in metadata.
        /// </summary>
        /// <param name="enumTypeShortName">The enumeration type short name.</param>
        /// <param name="vocDocs">The vocabulary documents.</param>
        /// <returns>Returns the enumeration type information.</returns>
        public static EnumTypeElement GetEnumType(string enumTypeShortName, List<string> vocDocs)
        {
            XElement enumTypeElement = null;

            foreach (var doc in vocDocs)
            {
                var termRestrictions = XElement.Parse(doc);
                string xPath = string.Format("//*[local-name()='EnumType' and @Name='{0}']", enumTypeShortName);
                enumTypeElement = termRestrictions.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != enumTypeElement)
                {
                    break;
                }
            }

            return null != enumTypeElement ? EnumTypeElement.Parse(enumTypeElement) : null;
        }
        #endregion

        #region Private members and methods.
        /// <summary>
        /// Validates whether the entity-set short name is exist in metadata or not.
        /// </summary>
        /// <param name="entitySetShortName">The entity-set short name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        private static bool IsEntitySetNameValid(this string entitySetShortName, string metadataDoc)
        {
            return entitySetShortName.IsTargetShortNameValid("EntitySet", metadataDoc);
        }

        /// <summary>
        /// Validates whether the entity-container short name is exist in metadata or not.
        /// </summary>
        /// <param name="entityContainerShortName">The entity-container short name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        private static bool IsEntityContainerNameValid(this string entityContainerShortName, string metadataDoc)
        {
            return entityContainerShortName.IsTargetShortNameValid("EntityContainer", metadataDoc);
        }

        /// <summary>
        /// Validates whether the property name is exist in metadata or not.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        private static bool IsPropertyNameValid(this string propertyName, string metadataDoc)
        {
            return propertyName.IsTargetShortNameValid("Property", metadataDoc);
        }

        /// <summary>
        /// Validates whether the target short name is exist in metadata document or not.
        /// </summary>
        /// <param name="targetShortName">The target short name.</param>
        /// <param name="localName">The local name of an element.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        private static bool IsTargetShortNameValid(this string targetShortName, string localName, string metadataDoc)
        {
            if (string.IsNullOrEmpty(targetShortName))
            {
                return false;
            }

            if (!metadataDoc.IsXmlPayload())
            {
                throw new FormatException("The parameter 'metadataDoc' is not an valid XML format string.");
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']", localName, targetShortName);
            var target = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != target ? true : false;
        }

        /// <summary>
        /// Gets the supportive feature information of all the metadata elements.
        /// Note: This method is only used to validate the term with Core.Tag type.
        /// </summary>
        /// <param name="targetShortName">The short name of target element.</param>
        /// <param name="termElement">The term information which will be validated.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        private static bool? GetSupportiveFeatureInfo(string targetShortName, TermElement termElement, string metadataDoc)
        {
            if (string.IsNullOrEmpty(targetShortName) || null == termElement || !metadataDoc.IsXmlPayload())
            {
                return null;
            }

            var metadata = XElement.Parse(metadataDoc);
            string aliasTermName = string.Format("{0}.{1}", termElement.Alias, termElement.Name);
            string namespaceTermName = string.Format("{0}.{1}", termElement.Namespace, termElement.Name);

            for (int i = 0; i < termElement.AppliesTo.Length; i++)
            {
                // Get the annotation information from target element in metadata. (e.g. EntitySet)
                string xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']/*[local-name()='Annotation'][@Term='{2}' or @Term='{3}']",
                    termElement.AppliesTo[i],
                    targetShortName,
                    aliasTermName,
                    namespaceTermName);
                var annotationElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != annotationElement)
                {
                    return null != annotationElement.Attribute("Bool") ?
                        Convert.ToBoolean(annotationElement.Attribute("Bool").Value) :
                        Convert.ToBoolean(termElement.DefaultValue);
                }
                else
                {
                    xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']", termElement.AppliesTo[i], targetShortName);
                    var entitySetElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                    if (null != entitySetElement)
                    {
                        var localAliasNamespace = MetadataHelper.GetAliasAndNamespace(entitySetElement);

                        // Get the annotation information from Annotations element in metadata.
                        xPath = string.Format(
                            "//*[local-name()='Annotations'][@Target='{0}' or @Target='{1}' or @Target='{2}']/*[local-name()='Annotation'][@Term='{3}' or @Term='{4}']",
                            targetShortName,
                            string.Format("{0}.{1}", localAliasNamespace.Alias, targetShortName),
                            string.Format("{0}.{1}", localAliasNamespace.Namespace, targetShortName),
                            aliasTermName,
                            namespaceTermName);
                        annotationElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                        if (null != annotationElement)
                        {
                            return null != annotationElement.Attribute("Bool") ?
                                Convert.ToBoolean(annotationElement.Attribute("Bool").Value) :
                                Convert.ToBoolean(termElement.DefaultValue);
                        }
                    }
                }
            }

            return Convert.ToBoolean(termElement.DefaultValue);
        }

        /// <summary>
        /// Gets the supportive feature information of all the metadata elements.
        /// Note: This method is only used to validate all the terms with complex type.
        /// </summary>
        /// <param name="targetShortName">The short name of target element.</param>
        /// <param name="termElement">The term information which will be validated.</param>
        /// <param name="complexTypeElement">The complex type template.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        public static ODataComplexType GetSupportiveFeatureInfo(string targetShortName, TermElement termElement, ComplexTypeElement complexTypeElement, string metadataDoc)
        {
            if (string.IsNullOrEmpty(targetShortName) || null == termElement || null == complexTypeElement || !metadataDoc.IsXmlPayload())
            {
                return null;
            }

            ODataComplexType complexType = new ODataComplexType(complexTypeElement.Name);
            var metadata = XElement.Parse(metadataDoc);
            string aliasTermName = string.Format("{0}.{1}", termElement.Alias, termElement.Name);
            string namespaceTermName = string.Format("{0}.{1}", termElement.Namespace, termElement.Name);

            for (int i = 0; i < termElement.AppliesTo.Length; i++)
            {
                #region Gets the inside annotation.
                // Gets the specified annotation which is defined in entity-set element.
                string xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']/*[local-name()='Annotation'][@Term='{2}' or @Term='{3}']",
                        termElement.AppliesTo[i],
                        targetShortName,
                        aliasTermName,
                        namespaceTermName);
                var annotationInside = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                #endregion

                #region Gets the outside annotation.
                // Gets the specified annotation which is defined in annotations element.
                XElement annotationOutside = null;
                xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']", termElement.AppliesTo[i], targetShortName);
                var entitySetElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != entitySetElement)
                {
                    var localAliasNamespace = MetadataHelper.GetAliasAndNamespace(entitySetElement);

                    xPath = string.Format("//*[local-name()='Annotations'][@Target='{0}' or @Target='{1}' or @Target='{2}']/*[local-name()='Annotation'][@Term='{3}' or @Term='{4}']",
                        targetShortName,
                        string.Format("{0}.{1}", localAliasNamespace.Alias, targetShortName),
                        string.Format("{0}.{1}", localAliasNamespace.Namespace, targetShortName),
                        aliasTermName,
                        namespaceTermName);
                    annotationOutside = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                }
                #endregion

                if (null != annotationInside)
                {
                    // Get the annotation information from target element in metadata. (e.g. EntitySet)
                    xPath = ".//*[local-name()='PropertyValue']";
                    var propertyValueElements = annotationInside.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

                    if (null != propertyValueElements && 0 != propertyValueElements.Count())
                    {
                        foreach (var pV in propertyValueElements)
                        {
                            complexType.AddProperty(ODataProperty.Parse(pV, complexTypeElement));
                        }
                    }
                }

                if (null != annotationOutside)
                {
                    // Get the annotation information from Annotations element in metadata.
                    var propertyValueElements = annotationOutside.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

                    if (null != propertyValueElements)
                    {
                        foreach (var pV in propertyValueElements)
                        {
                            complexType.AddProperty(ODataProperty.Parse(pV, complexTypeElement));
                        }
                    }
                }

                if (complexType.GetProperties().Any())
                {
                    return complexType;
                }
            }

            foreach (var prop in complexTypeElement.Properties)
            {
                if (null != prop.DefaultValue && prop.Type.StartsWith("Edm."))
                {
                    complexType.AddProperty(new ODataProperty(prop.Name, prop.DefaultValue, prop.Type));
                }
            }

            return complexType;
        }

        /// <summary>
        /// Gets the supportive feature information of all the metadata elements.
        /// Note: This method is only used to validate all the terms with enumeration type.
        /// </summary>
        /// <param name="targetShortName">The short name of target element.</param>
        /// <param name="termElement">The term information which will be validated.</param>
        /// <param name="enumTypeElement">The enumeration type template.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the validation.</returns>
        public static List<KeyValuePair<string, int>> GetSupportiveFeatureInfo(string targetShortName, TermElement termElement, EnumTypeElement enumTypeElement, string metadataDoc)
        {
            if (string.IsNullOrEmpty(targetShortName) || null == termElement || null == enumTypeElement || !metadataDoc.IsXmlPayload())
            {
                return null;
            }

            var metadata = XElement.Parse(metadataDoc);
            string aliasTermName = string.Format("{0}.{1}", termElement.Alias, termElement.Name);
            string namespaceTermName = string.Format("{0}.{1}", termElement.Namespace, termElement.Name);
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

            for (int i = 0; i < termElement.AppliesTo.Length; i++)
            {
                // Get the annotation information from target element in metadata. (e.g. EntitySet)
                string xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']/*[local-name()='Annotation'][@Term='{2}' or @Term='{3}']/*[local-name()='EnumMember']",
                    termElement.AppliesTo[i],
                    targetShortName,
                    aliasTermName,
                    namespaceTermName);
                var enumMemberElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                if (null != enumMemberElement)
                {
                    string[] enumMembers = enumMemberElement.Value.Split(' ');

                    foreach (var enumMember in enumMembers)
                    {
                        foreach (var m in enumTypeElement.Members)
                        {
                            if (enumMember.Contains(m.Key))
                            {
                                result.Add(new KeyValuePair<string, int>(m.Key, m.Value));
                            }
                        }
                    }
                }
                else
                {
                    xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']", termElement.AppliesTo[i], targetShortName);
                    var entitySetElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                    if (null != entitySetElement)
                    {
                        var localAliasNamespace = MetadataHelper.GetAliasAndNamespace(entitySetElement);

                        // Get the annotation information from Annotations element in metadata.
                        xPath = string.Format(
                            "//*[local-name()='Annotations'][@Target='{0}' or @Target='{1}' or @Target='{2}']/*[local-name()='Annotation'][@Term='{3}' or @Term='{4}']/*[local-name()='EnumMember']",
                            targetShortName,
                            string.Format("{0}.{1}", localAliasNamespace.Alias, targetShortName),
                            string.Format("{0}.{1}", localAliasNamespace.Namespace, targetShortName),
                            aliasTermName,
                            namespaceTermName);
                        enumMemberElement = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

                        if (null != enumMemberElement)
                        {
                            string[] enumMembers = enumMemberElement.Value.Split(' ');

                            foreach (var enumMember in enumMembers)
                            {
                                foreach (var m in enumTypeElement.Members)
                                {
                                    if (enumMember.Contains(m.Key))
                                    {
                                        result.Add(new KeyValuePair<string, int>(m.Key, m.Value));
                                    }
                                }
                            }
                        }
                    }
                }

                if (result.Any())
                {
                    return result;
                }
            }

            return result;
        }
        #endregion
    }
}
