// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    public static class UtilityHelper
    {
        /// <summary>
        /// Gets the nullable bool value
        /// </summary>
        /// <param name="input">The input string; could be null</param>
        /// <returns>The nullable bool value</returns>
        public static bool? ToNullableBool(this string input)
        {
            bool? result = null;
            if (!string.IsNullOrEmpty(input))
            {
                result = input.Equals("true", StringComparison.OrdinalIgnoreCase) ? (bool?)true :
                               input.Equals("false", StringComparison.OrdinalIgnoreCase) ? (bool?)false :
                               null;
            }

            return result;
        }

        /// <summary>
        /// Converts a string to an Enum type
        /// </summary>
        /// <typeparam name="T">Type parameter of the Enum type</typeparam>
        /// <param name="input">The string to be converted</param>
        /// <returns>The enum value of Type T corresponding to string literal case-insensitively</returns>
        /// <exception cref="ArgumentException">Throws when the input string does not match any T enum value</exception>
        public static T ToEnum<T>(this string input)
        {
            return (T)Enum.Parse(typeof(T), input, true);
        }

        /// <summary>
        /// Converts a string to an Enum value; falling back to null
        /// </summary>
        /// <typeparam name="T">The enum type to be converted</typeparam>
        /// <param name="input">The input string literal</param>
        /// <returns>The T value matching the literal string; or null if no macth</returns>
        public static T? ToNullable<T>(this string input)
            where T : struct
        {
            return ToNullable<T>(input, null);
        }

        /// <summary>
        /// Converts a string to a Nullable Enum type
        /// </summary>
        /// <typeparam name="T">The enum type to be converted</typeparam>
        /// <param name="input">The input string literal</param>
        /// <param name="defaultVal">The value returned in case of no match</param>
        /// <returns>The T value matching the input string; or the default value if no match</returns>
        public static T? ToNullable<T>(this string input, T? defaultVal)
            where T : struct
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultVal;
            }

            try
            {
                return ToEnum<T>(input);
            }
            catch (ArgumentException)
            {
                return defaultVal;
            }
        }
    }
}
