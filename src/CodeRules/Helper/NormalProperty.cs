// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Xml.Linq;
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
        /// <param name="isValueNull">The flag indicates whether a property value is null or not. 
        /// The default value is true.</param>
        /// <param name="srid">The SRID attribute.</param>
        public NormalProperty(string propertyName, string propertyType, bool isKey, bool isNullable = true, bool isValueNull = false, string srid = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName", "The value of the input parameter 'propertyName' MUST NOT be null or empty.");
            }

            if (string.IsNullOrEmpty(propertyType))
            {
                throw new ArgumentNullException("propertyType", "The value of the input parameter 'propertyType' MUST NOT be null or empty.");
            }

            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.IsKey = isKey;
            this.IsNullable = isNullable;
            this.IsValueNull = isValueNull;
            this.SRID = srid;
        }

        /// <summary>
        /// Gets or private sets a property name.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a property type.
        /// </summary>
        public string PropertyType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a flag which identifies a key property.
        /// </summary>
        public bool IsKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a flag which identifies a null property.
        /// </summary>
        public bool IsValueNull
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a flag which identifies a nullable property.
        /// </summary>
        public bool IsNullable
        {
            get;
            private set;
        }

        public string SRID
        {
            get;
            private set;
        }
    }
}
