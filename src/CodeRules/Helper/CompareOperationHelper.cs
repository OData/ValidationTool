// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// Compare operation helper.
    /// </summary>
    public static class CompareOperationHelper
    {
        /// <summary>
        /// Compares a JToken value whether is greater than or equal with an double value or not.
        /// </summary>
        /// <param name="jVal">A JToken value.</param>
        /// <param name="val">A number using to compare.</param>
        /// <param name="primitiveType">The primitive data type of a JToken value.</param>
        /// <returns>Returns true if the JToken value is greater than or equal with the integer, otherwise false.</returns>
        public static bool GreaterThanOrEquals(this JToken jVal, object val, string primitiveType)
        {
            bool result = false;

            if (jVal == null || string.IsNullOrEmpty(primitiveType))
            {
                return result;
            }

            try
            {
                switch (primitiveType)
                {
                    case PrimitiveDataTypes.Byte:
                        result = (byte)jVal >= Convert.ToByte(val);
                        break;
                    case PrimitiveDataTypes.Int16:
                        result = (short)jVal >= Convert.ToInt16(val);
                        break;
                    case PrimitiveDataTypes.Int32:
                        result = (int)jVal >= Convert.ToInt32(val);
                        break;
                    case PrimitiveDataTypes.Int64:
                        result = (long)jVal >= Convert.ToInt64(val);
                        break;
                    case PrimitiveDataTypes.Decimal:
                        result = (decimal)jVal >= Convert.ToDecimal(val);
                        break;
                    case PrimitiveDataTypes.Double:
                        result = (double)jVal >= Convert.ToDouble(val);
                        break;
                    case PrimitiveDataTypes.Single:
                        result = (Single)jVal >= Convert.ToSingle(val);
                        break;
                    case PrimitiveDataTypes.SByte:
                        result = (sbyte)jVal >= Convert.ToSByte(val);
                        break;
                }
            }
            catch (FormatException e)
            {
                throw new FormatException(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Compare the property value.
        /// </summary>
        /// <param name="val1">The property value.</param>
        /// <param name="val2">The property value.</param>
        /// /// <param name="propType">The property type.</param>
        /// <param name="compareType">The compare type.</param>
        /// <returns>Return the result.</returns>
        public static bool Compare(this JToken val1, JToken val2, string propType, ComparerType compareType)
        {
            if (0 != string.Compare("Edm.Int16", propType, true) &&
                0 != string.Compare("Edm.Int32", propType, true) &&
                0 != string.Compare("Edm.Int64", propType, true) &&
                0 != string.Compare("Edm.String", propType, true))
            {
                throw new Exception("Invalid value of property type, please use the 'Edm.Int16', 'Edm.Int32', 'Edm.Int64' or 'Edm.String' as the value of the input parameter 'propType'.");
            }

            var result = false;
            if (0 == string.Compare("Edm.Int16", propType, true) ||
                0 == string.Compare("Edm.Int32", propType, true) ||
                0 == string.Compare("Edm.Int64", propType, true))
            {
                result = CompareOperationHelper.Int64Compare(val1, val2, compareType);
            }
            else if (0 == string.Compare("Edm.String", propType, true))
            {
                result = CompareOperationHelper.StringCompare(val1, val2, compareType);
            }

            return result;
        }

        /// <summary>
        /// Compare the property value with int type.
        /// </summary>
        /// <param name="val1">The property value.</param>
        /// <param name="val2">The property value.</param>
        /// <param name="compareType">The compare type.</param>
        /// <returns>Return the result.</returns>
        private static bool Int64Compare(this JToken val1, JToken val2, ComparerType compareType)
        {
            var result = false;
            var value1 = (Int64)val1;
            var value2 = (Int64)val2;
            switch ((byte)compareType)
            {
                // Equal.
                case 0x01:
                    result = value1 == value2;
                    break;
                // Less Than.
                case 0x02:
                    result = value1 < value2;
                    break;
                // Less Than or Equal.
                case 0x03:
                    result = value1 <= value2;
                    break;
                // Greater Than.
                case 0x04:
                    result = value1 > value2;
                    break;
                // Greater Than or Equal.
                case 0x05:
                    result = value1 >= value2;
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Compare the property value with string type.
        /// </summary>
        /// <param name="val1">The property value.</param>
        /// <param name="val2">The property value.</param>
        /// <param name="compareType">The compare type.</param>
        /// <returns>Return the result.</returns>
        private static bool StringCompare(this JToken val1, JToken val2, ComparerType compareType)
        {
            var result = false;
            var value1 = val1.ToString();
            var value2 = val2.ToString();
            switch ((byte)compareType)
            {
                // Equal.
                case 0x01:
                    result = 0 == string.Compare(value1, value2);
                    break;
                // Less Than.
                case 0x02:
                    result = -1 == string.Compare(value1, value2);
                    break;
                // Less Than or Equal.
                case 0x03:
                    result = 0 == string.Compare(value1, value2) || -1 == string.Compare(value1, value2);
                    break;
                // Greater Than.
                case 0x04:
                    result = 1 == string.Compare(value1, value2);
                    break;
                // Greater Than or Equal.
                case 0x05:
                    result = 0 == string.Compare(value1, value2) || 1 == string.Compare(value1, value2);
                    break;
                default:
                    break;
            }

            return result;
        }
    }

    /// <summary>
    /// The ComparerType enumeration.
    /// Note: (LessThanOrEqual = 0x03, GreaterThanOrEqual = 0x05)
    /// </summary>
    [Flags]
    public enum ComparerType : byte
    {
        /// <summary>
        /// Undefine.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Equal.
        /// </summary>
        Equal = 0x01,

        /// <summary>
        /// Less Than.
        /// </summary>
        LessThan = 0x02,

        /// <summary>
        /// Greater Than.
        /// </summary>
        GreaterThan = 0x04
    }
}
