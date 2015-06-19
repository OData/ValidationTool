// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    #endregion

    /// <summary>
    /// The normal property class of an entity type.
    /// </summary>
    public class NormalProperty
    {
        /// <summary>
        /// Initializes the NormalProperty class.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="isKey">The flag indicates whether a property is a key or not.</param>
        /// <param name="isNullable">The flag indicates whether a property value can be set to null or not. 
        /// The default value is true.</param>
        public NormalProperty(string propertyName, string propertyType, bool isKey, bool isNullable = true)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName", "The value of the input parameter 'propertyName' MUST NOT be null or empty.");
            }

            if (string.IsNullOrEmpty(propertyType))
            {
                throw new ArgumentNullException("propertyType", "The value of the input parameter 'propertyType' MUST NOT be null or empty.");
            }

            this.propertyName = propertyName;
            this.propertyType = propertyType;
            this.isKey = isKey;
            this.isNullable = isNullable;
        }

        /// <summary>
        /// Gets or sets a property name.
        /// </summary>
        public string PropertyName
        {
            get
            {
                return propertyName;
            }
            set
            {
                propertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets a property type.
        /// </summary>
        public string PropertyType
        {
            get
            {
                return propertyType;
            }
            set
            {
                propertyType = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag which identifies a key property.
        /// </summary>
        public bool IsKey
        {
            get
            {
                return isKey;
            }
            set
            {
                isKey = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag which identifies a nullable property.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                return isNullable;
            }
            set
            {
                isNullable = value;
            }
        }

        /// <summary>
        /// The property name.
        /// </summary>
        private string propertyName;

        /// <summary>
        /// The property type.
        /// </summary>
        private string propertyType;

        /// <summary>
        /// The flag indicates whether a property is a key or not.
        /// </summary>
        private bool isKey;

        /// <summary>
        /// The flag indicates whether a property value can be set to null or not.
        /// </summary>
        private bool isNullable;
    }
}
