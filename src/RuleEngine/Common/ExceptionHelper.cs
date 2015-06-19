// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    #endregion Namespaces

    /// <summary>
    /// Class with utility methods to work with exceptions
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>Type of OutOfMemoryException.</summary>
        private static readonly Type OutOfMemoryType = typeof(System.OutOfMemoryException);

        /// <summary>Type of StackOverflowException.</summary>
        private static readonly Type StackOverflowType = typeof(System.StackOverflowException);

        /// <summary>Type of ThreadAbortException.</summary>
        private static readonly Type ThreadAbortType = typeof(System.Threading.ThreadAbortException);

        /// <summary>
        /// Determines whether the specified exception can be caught and 
        /// handled, or whether it should be allowed to continue unwinding.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to test.</param>
        /// <returns>
        /// true if the specified exception can be caught and handled; 
        /// false otherwise.
        /// </returns>
        public static bool IsCatchableExceptionType(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Type type = exception.GetType();

            // a 'catchable' exception is defined by what it is not.
            return ((type != StackOverflowType) &&
                     (type != OutOfMemoryType) &&
                     (type != ThreadAbortType));
        }
    }
}
