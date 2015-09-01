// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using ODataValidator.RuleEngine;
    using System.ComponentModel.DataAnnotations;
    #endregion

    /// <summary>
    /// Service Implementation Category Class
    /// </summary>
    public class ServiceImplCategory
    {
        public ServiceImplCategory(ServiceImplCategoryName name, ServiceImplCategory parent = null)
        {
            this.CategoryName = name;
            this.ParentCategory = parent;
        }

        public ServiceImplCategoryName CategoryName
        {
            get;
            protected set;
        }

        public ServiceImplCategory ParentCategory
        {
            get;
            private set;
        }

        public string CategoryFriendlyName
        {
            get
            {
                var fieldInfo = this.CategoryName.GetType().GetField(this.CategoryName.ToString());
                var displayAttr = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
                if (null == displayAttr)
                {
                    return string.Empty;
                }

                return displayAttr.Length > 0 ? displayAttr[0].Name : this.CategoryName.ToString();
            }
        }

        public string CategoryFullName
        {
            get
            {
                var temp = this;
                string result = temp.CategoryFriendlyName;
                while (null != temp.ParentCategory)
                {
                    result = temp.ParentCategory.CategoryFriendlyName + "," + result;
                    temp = temp.ParentCategory;
                }

                return result;
            }
        }
    }


    public abstract class ServiceImplExtensionRule : ExtensionRule
    {
        /// <summary>
        /// Gets Category property - Default is "ServiceImpl"
        /// </summary>
        public override string Category
        {
            get
            {
                return "ServiceImpl";
            }
        }

        /// <summary>
        /// Gets the V4 specification - Default is "ODataV4SpecificationUriForProtocol"
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "ODataV4SpecificationUriForProtocol";
            }
        }

        /// <summary>
        /// Gets the version - Default is "V4"
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V_All;
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
                return null;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies - Default is "JsonLight"
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Service implementation feature types
        /// </summary>
        public abstract ServiceImplCategory CategoryInfo { get; }
    }
}
