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
    }
}
