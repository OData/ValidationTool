// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    #endregion

    /// <summary>
    /// The navigation property (only be used in v4 odata service) class of an entity type.
    /// </summary>
    public class NavigProperty
    {
        /// <summary>
        /// Initializes the NavigProperty class.
        /// </summary>
        /// <param name="navigPropName"></param>
        /// <param name="navigPropType"></param>
        /// <param name="navigPropPartner"></param>
        public NavigProperty(string navigPropName, string navigPropType, string navigPropPartner)
        {
            if (string.IsNullOrEmpty(navigPropName))
            {
                throw new ArgumentNullException("navigPropName", "The value of the input parameter 'navigPropName' MUST NOT be null or empty.");
            }

            if (string.IsNullOrEmpty(navigPropType))
            {
                throw new ArgumentNullException("navigPropType", "The value of the input parameter 'navigPropType' MUST NOT be null or empty.");
            }

            this.NavigationPropertyName = navigPropName;
            this.NavigationPropertyType = navigPropType;
            this.NavigationPropertyPartner = navigPropPartner;
            this.NavigationRoughType = this.NavigationPropertyType.RemoveCollectionFlag() == this.NavigationPropertyType ?
                NavigationRoughType.SingleValued : NavigationRoughType.CollectionValued;
        }

        /// <summary>
        /// Gets or sets the navigation property name.
        /// </summary>
        public string NavigationPropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the navigation property type.
        /// </summary>
        public string NavigationPropertyType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the navigation property partner.
        /// </summary>
        public string NavigationPropertyPartner
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the rough-type of a navigation property.
        /// </summary>
        public NavigationRoughType NavigationRoughType
        {
            get;
            private set;
        }
    }
}
