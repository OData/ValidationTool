// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// The JTokenCompare class inherit the ICompare interface.
    /// Usage: Sort the JToken objects.
    /// </summary>
    public class JTokenCompare : IComparer<JToken>
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="propertyName">The property's name.</param>
        /// <param name="propertyType">The property's primitive type.</param>
        /// <param name="sortedType">The sorted type.</param>
        public JTokenCompare(string propertyName, string propertyType, SortedType sortedType = SortedType.ASC)
        {
            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyType))
            {
                throw new ArgumentNullException();
            }

            this.propName = propertyName;
            this.propType = propertyType;
            this.sortedType = sortedType;
        }

        /// <summary>
        /// Compare the two JToken objects.
        /// </summary>
        /// <param name="x">The first JToken object for compare.</param>
        /// <param name="y">The second JToken object for compare.</param>
        /// <returns>Returns the comparative result.</returns>
        public int Compare(JToken x, JToken y)
        {
            int result = 0;

            try
            {
                switch (this.propType)
                {
                    case PrimitiveDataTypes.Binary:
                    case PrimitiveDataTypes.Guid:
                    case PrimitiveDataTypes.String:
                        result = x[this.propName].ToString().CompareTo(y[this.propName].ToString());
                        break;
                    case PrimitiveDataTypes.Boolean:
                        result = ((bool)x[this.propName]).CompareTo((bool)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Byte:
                        result = ((byte)x[this.propName]).CompareTo((byte)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Decimal:
                        result = ((decimal)x[this.propName]).CompareTo((decimal)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Double:
                        result = ((double)x[this.propName]).CompareTo((double)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Single:
                        result = ((Single)x[this.propName]).CompareTo((Single)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Int16:
                        result = ((short)x[this.propName]).CompareTo((short)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Int32:
                        result = ((int)x[this.propName]).CompareTo((int)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.Int64:
                        result = ((long)x[this.propName]).CompareTo((long)y[this.propName]);
                        break;
                    case PrimitiveDataTypes.SByte:
                        result = ((sbyte)x[this.propName]).CompareTo((sbyte)y[this.propName]);
                        break;
                }
            }
            catch (FormatException e)
            {
                throw new FormatException(e.Message);
            }

            return this.sortedType == SortedType.ASC ? result : -result;
        }

        /// <summary>
        /// The property's name.
        /// </summary>
        private string propName;

        /// <summary>
        /// The property's primitive type.
        /// </summary>
        private string propType;

        /// <summary>
        /// The sorted type.
        /// Indicate that sort the objects order by ascending or descending.
        /// </summary>
        private SortedType sortedType;
    }
}
