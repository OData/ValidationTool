// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The ChangeTrackingType structure.
    /// </summary>
    public struct ChangeTrackingType
    {
        public ChangeTrackingType(bool? supported, List<string> filterableProperties, List<string> expandableProperties)
        {
            this.supported = supported;
            this.filterableProperties = filterableProperties;
            this.expandableProperties = expandableProperties;
        }

        /// <summary>
        /// Gets the change-tracking supported result.
        /// </summary>
        public bool? Supported
        {
            get
            {
                return supported;
            }
        }

        /// <summary>
        /// Gets the filterable properties which support change-tracking.
        /// </summary>
        public List<string> FilterableProperties
        {
            get
            {
                return filterableProperties;
            }
        }

        /// <summary>
        /// Gets the expandable properties which support change-tracking.
        /// </summary>
        public List<string> ExpandableProperties
        {
            get
            {
                return expandableProperties;
            }
        }

        /// <summary>
        /// Indicate whether the service or the entity-set support change-tracking.
        /// </summary>
        private bool? supported;

        /// <summary>
        /// Change tracking supports filters on these properties.
        /// </summary>
        private List<string> filterableProperties;

        /// <summary>
        /// Change tracking supports these properties expanded.
        /// </summary>
        private List<string> expandableProperties;
    }
}
