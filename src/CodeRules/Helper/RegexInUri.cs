// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    /// <summary>
    /// Helper class of simple URI syntax
    /// </summary>
    static class RegexInUri
    {
        /// <summary>
        /// Opening part of reggular expression
        /// </summary>
        public const string reBegin = "^(";

        /// <summary>
        /// Closing part of regular expression
        /// </summary>
        public const string reEnd = ")$";

        /// <summary>
        /// Regular expression: Digits
        /// </summary>
        public const string reDigits = "[0-9]+";

        /// <summary>
        /// Regular expression: single-quoted string (URL-encoded)
        /// </summary>
        public const string reQuoted = @"'([0-9a-zA-Z\-\._~]|(%[0-9A-Fa-f]{2}))+'";

        /// <summary>
        /// Regular expression: single key predicate
        /// </summary>
        public static readonly string reSingle = string.Format(@"({0})|({1})", RegexInUri.reDigits, RegexInUri.reQuoted);

        /// <summary>
        /// Regular expression: property name
        /// </summary>
        public const string reProperty = @"[_a-zA-Z]([_a-zA-Z0-9]*)";

        /// <summary>
        /// Regular expression: complex key predicate
        /// </summary>
        public static readonly string reComplex = string.Format(@"({0})\s*=\s*({1})(\s*,\s*({0})\s*=\s*({1}))*", RegexInUri.reProperty, RegexInUri.reSingle);

        /// <summary>
        /// Checks whether the input string is a single key predicate
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>Returns true if input is of a single key predicate; false otherwise</returns>
        public static bool IsSingleKeyPredicate(string input)
        {
            return Regex.IsMatch(input, RegexInUri.reBegin + RegexInUri.reSingle + RegexInUri.reEnd, RegexOptions.Singleline);
        }

        /// <summary>
        /// Checks whether the input string is a complex key predicate
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>Returns true if it is of a complex key predicate; false otherwise</returns>
        public static bool IsComplexKeyPredicate(string input)
        {
            return Regex.IsMatch(input, RegexInUri.reBegin + RegexInUri.reComplex + RegexInUri.reEnd, RegexOptions.Singleline);
        }

        /// <summary>
        /// Checks whether the input string is a key predicate
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>Returns true if input is of a key predicate; false otherwise</returns>
        public static bool IsKeyPredicate(string input)
        {
            return RegexInUri.IsSingleKeyPredicate(input) || RegexInUri.IsComplexKeyPredicate(input);
        }

        /// <summary>
        /// Checks whether a relative path is of URI-2
        /// </summary>
        /// <param name="path">The relative path</param>
        /// <param name="meta">The metadata document</param>
        /// <returns>Returns true if it is a URI-2; false otherwise</returns>
        public static bool IsURI2(string path, XElement meta)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(path))
            {
                int posLP = path.IndexOf('(');
                if (posLP > 0)
                {
                    int posRP = path.LastIndexOf(')');
                    if (posRP > posLP)
                    {
                        string esPath = path.Substring(0, posLP);
                        string keyPredicat = path.Substring(posLP + 1, posRP - posLP - 1).Trim();

                        // check it is a real URI2 or not based on metadata document
                        string xpath = string.Format("//*[local-name()='EntitySet' and @Name='{0}']", esPath);
                        bool isEntitySet = meta.XPathSelectElement(xpath) != null;
                        if (isEntitySet)
                        {
                            result = RegexInUri.IsKeyPredicate(keyPredicat);
                        }
                    }
                }
            }

            return result;
        }
    }
}
