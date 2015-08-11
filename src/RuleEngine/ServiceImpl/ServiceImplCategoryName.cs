// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespace.
    using System.ComponentModel.DataAnnotations;
    #endregion

    /// <summary>
    /// The Service Implemantation Category Name enum class.
    /// </summary>
    public enum ServiceImplCategoryName : byte
    {
        [Display(Name = "None")]
        None = 0x00,

        #region Level 1
        [Display(Name = "Asynchronous Requests")]
        AsynchronousRequests,

        [Display(Name = "Batch Requests")]
        BatchRequests,

        [Display(Name = "Data Modification")]
        DataModification,

        [Display(Name = "Metadata Requests")]
        MetadataRequests,

        [Display(Name = "Operations")]
        Operations,

        [Display(Name = "Requesting Changes")]
        RequestingChanges,

        [Display(Name = "Requesting Data")]
        RequestingData,

        [Display(Name = "Response Headers")]
        ResponseHeaders,
        #endregion

        #region Level 2
        [Display(Name = "Managing Media Entities")]
        ManagingMediaEntities,

        [Display(Name = "Managing Stream Properties")]
        ManagingStreamProperties,

        [Display(Name = "Managing Values and Properties Directly")]
        ManagingValues_Properties,

        [Display(Name = "Modifying Relationships between Entities")]
        ModifyingRelationships,

        [Display(Name = "System Query Option")]
        SystemQueryOption,
        #endregion

        #region Level 3
        [Display(Name = "Arithmetic Operators")]
        ArithmeticOperators,

        [Display(Name = "Canonical Functions")]
        CanonicalFunctions,

        [Display(Name = "Lambda Operators")]
        LambdaOperators,

        [Display(Name = "Logical Operators")]
        LogicalOperators,

        [Display(Name = "Operators")]
        Operators
        #endregion
    }
}
