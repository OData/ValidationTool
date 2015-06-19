// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System.Diagnostics;
    using System;
    #endregion

    /// <summary>
    /// Helper class that preprocesses macros in xslt instructions
    /// </summary>
    internal static class XsltRulePreprocessor
    {
        /// <summary>
        /// Substitudes macros in xslt instructions with corresponding property values of the current interop request context
        /// </summary>
        /// <param name="context">The interop request context</param>
        /// <param name="xslt">xslt instructions</param>
        /// <returns>xslt instructions with all the macroes substituded</returns>
        public static string Preprocess(ServiceContext context, string xslt)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(xslt))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "xslt");
            }

            return xslt.Replace("$ENTITYTYPE$", context.EntityTypeShortName)
                .Replace("$NSENTITYTYPE$", context.EntityTypeFullName)
                .Replace("$URI$", context.DestinationBasePath)
                .Replace("$LSURI$", context.DestinationBaseLastSegment);
        }
    }
}
