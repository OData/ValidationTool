// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namesapces
    using System;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.IO;
    using System.Linq;
    using Microsoft.CSharp;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// String Helper class
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Generates full literal representation of a string 
        /// </summary>
        /// <param name="input">The string input, like "hello,\t\".NET\""</param>
        /// <returns>The full literal of the string, like "\"hello,\\t\\\".NET\\\"\""</returns>
        public static string ToLiteral(string input)
        {
            var writer = new StringWriter();
            var codeGen = new CSharpCodeProvider();
            codeGen.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
            return writer.ToString();
        }

        /// <summary>
        /// Judges whether the target string contains any element in the collection of strings or not.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="collection">The collection of string.</param>
        /// <returns>Return the result of comparision.</returns>
        public static bool ContainsIn(this string target, List<string> collection)
        {
            foreach (var str in collection)
            {
                if (target.Contains(str))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Judges whether the target string is contained any element in the collection of strings or not.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="collection">The collection of string.</param>
        /// <returns>Return the result of comparision.</returns>
        public static bool ContainedIn(this string target, List<string> collection)
        {
            foreach (var str in collection)
            {
                if (str.Contains(target))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a specified string from target string.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="removedStr">The specified string.</param>
        /// <returns>Returns a string has been removed the specified string from the end of the target string.</returns>
        public static string RemoveEnd(this string target, string removedStr)
        {
            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(removedStr))
            {
                int index = target.LastIndexOf(removedStr);
                return target.Remove(index, removedStr.Length);
            }
            else
            {
                return target;
            }
        }

        /// <summary>
        /// Split the string to two parts by the first splitted char.
        /// </summary>
        /// <param name="target">The target string will be splitted.</param>
        /// <param name="splittedChar">The splitted char used to split the target string.</param>
        /// <returns>Returns the two parts of target string does not contain the splitted char.</returns>
        public static string[] Split(this string target, char splittedChar)
        {
            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

            int index = target.IndexOf(splittedChar);

            return new string[] { target.Remove(index, target.Length - index), target.Remove(0, index + 1) };
        }

        /// <summary>
        /// Filter the specified information from a string data.
        /// </summary>
        /// <param name="data">A string data.</param>
        /// <param name="information">A specified information which is contained in a string data.</param>
        /// <param name="terminators">The specified terminators.</param>
        /// <param name="ignores">Ignore the head string of the information.</param>
        /// <returns>Returns all the related strings which contain specified information.</returns>
        public static string Filtration(this string data, string information, List<char> terminators, string ignores = null)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(information) || null == terminators || 0 == terminators.Count)
            {
                return string.Empty;
            }

            int offset = 0;
            string result = string.Empty;

            while (data.Length > offset)
            {
                int index = data.IndexOf(information, offset);

                if (-1 == index)
                {
                    break;
                }

                int loop = index;

                while (data.Length > loop)
                {
                    if (!string.IsNullOrEmpty(ignores))
                    {
                        for (int i = 0; i < ignores.Length; i++)
                        {
                            if (ignores[i] == data[index])
                            {
                                index++;
                                loop++;
                            }
                        }
                    }

                    if (terminators.Contains(data[loop]))
                    {
                        offset = loop + 1;
                        break;
                    }

                    loop++;
                }

                result += data.Substring(index, loop - index + 1);
            }

            return result;
        }

        /// <summary>
        /// Filter the specified informations from a string data.
        /// </summary>
        /// <param name="data">A string data.</param>
        /// <param name="informations">Some specified informations which are contained in a string data.</param>
        /// <param name="terminators">The specified terminators.</param>
        /// <param name="ignores">Ignore the head string of the information.</param>
        /// <returns>Returns all the related strings which contain specified informations.</returns>
        public static string Filtration(this string data, List<string> informations, List<char> terminators, string ignores = null)
        {
            if (string.IsNullOrEmpty(data) || null == informations || 0 == informations.Count || null == terminators || 0 == terminators.Count)
            {
                return string.Empty;
            }

            int offset = 0;
            string result = string.Empty;

            while (data.Length > offset)
            {
                List<int> indexes = new List<int>();
                informations.ForEach(info =>
                {
                    indexes.Add(data.IndexOf(info, offset));
                });

                int index = indexes.OrderBy(n => n, new IntegerCompare(SortedType.ASC)).First();

                if (-1 == index)
                {
                    break;
                }

                int loop = index;

                while (data.Length > loop)
                {
                    if (!string.IsNullOrEmpty(ignores))
                    {
                        for (int i = 0; i < ignores.Length; i++)
                        {
                            if (ignores[i] == data[index])
                            {
                                index++;
                                loop++;
                            }
                        }
                    }

                    if (terminators.Contains(data[loop]))
                    {
                        offset = loop;
                        break;
                    }

                    loop++;
                }

                result += data.Substring(index, loop - index + 1);
            }

            return result;
        }

        /// <summary>
        /// Filter the specified information from a string data.
        /// </summary>
        /// <param name="data">A string data.</param>
        /// <param name="information">A specified information which is contained in a string data.</param>
        /// <returns>Returns all the related strings which contain specified informations.</returns>
        public static string Filtration(this string data, string information)
        {
            return StringHelper.Filtration(data, information, new List<char> { '\n' }, "\n");
        }

        /// <summary>
        /// Filter the specified informations from a string data.
        /// </summary>
        /// <param name="data">A string data.</param>
        /// <param name="informations">Some specified informations which are contained in a string data.</param>
        /// <returns>Returns all the related strings which contain specified informations.</returns>
        public static string Filtration(this string data, List<string> informations)
        {
            return StringHelper.Filtration(data, informations, new List<char> { '\n' }, "\n");
        }

        /// <summary>
        /// Remove the 'Collection(x)' flag from the type string.
        /// </summary>
        /// <param name="target">The target type string which starts with 'Collection(' and ends with ')'.</param>
        /// <returns>Returns the fundamental type string.</returns>
        public static string RemoveCollectionFlag(this string target)
        {
            return target.RemoveCollectionFlag("Collection(", ")");
        }

        /// <summary>
        /// Remove the collection flag from the collected-value type.
        /// </summary>
        /// <param name="target">The target type string which contains the collection flag.</param>
        /// <param name="startFlag">The start flag of a collection flag.</param>
        /// <param name="endFlag">The end flag of a collection flag.</param>
        /// <returns>Return the fundamental type string.</returns>
        public static string RemoveCollectionFlag(this string target, string startFlag, string endFlag)
        {
            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(startFlag) || string.IsNullOrEmpty(endFlag)
                || !target.StartsWith(startFlag) || !target.EndsWith(endFlag))
            {
                return target;
            }

            string temp = target.Remove(0, startFlag.Length);
            return temp.Remove(temp.Length - endFlag.Length, endFlag.Length);
        }

        /// <summary>
        /// Remove the 'Edm.' from the build-in type.
        /// </summary>
        /// <param name="target">The type name which contains the prefix 'Edm.'.</param>
        /// <returns>If the target string contains a prefix 'Edm.', it will remove the prefix and return a string without 'Edm.'. 
        /// Otherwise it returns the original string.</returns>
        public static string RemoveEdmDotPrefix(this string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

            return target.StartsWith("Edm.") ? target.Remove(0, "Edm.".Length) : target;
        }

        public static string MergeHeaders(string acceptHeader, IEnumerable<KeyValuePair<string, string>> headers)
        {
            string totalHeaders = string.Empty;
            if (acceptHeader != string.Empty)
                totalHeaders = "Accept:" + acceptHeader + "\r\n";

            if (null != headers)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    totalHeaders += pair.Key + ":" + pair.Value + "\r\n";
                }
            }

            return totalHeaders;
        }

        /// <summary>
        /// Whether the string is a primitive type name.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool IsPrimitiveTypeName(this string typeName)
        {
            switch (typeName)
            {
                case "Edm.Binary":
                case "Edm.Boolean":
                case "Edm.Byte":
                case "Edm.Date":
                case "Edm.DateTimeOffset":
                case "Edm.Decimal":
                case "Edm.Double":
                case "Edm.Duration":
                case "Edm.Guid":
                case "Edm.Int16":
                case "Edm.Int32":
                case "Edm.Int64":
                case "Edm.SByte":
                case "Edm.Single":
                case "Edm.Stream":
                case "Edm.String":
                case "Edm.TimeOfDay":
                case "Edm.Geography":
                case "Edm.GeographyPoint":
                case "Edm.GeographyLineString":
                case "Edm.GeographyPolygon":
                case "Edm.GeographyMultiPoint":
                case "Edm.GeographyMultiLineString":
                case "Edm.GeographyMultiPolygon":
                case "Edm.GeographyCollection":
                case "Edm.Geometry":
                case "Edm.GeometryPoint":
                case "Edm.GeometryLineString":
                case "Edm.GeometryPolygon":
                case "Edm.GeometryMultiPoint":
                case "Edm.GeometryMultiLineString":
                case "Edm.GeometryMultiPolygon":
                case "Edm.GeometryCollection":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether the string is a built-in abstarct type name.
        /// </summary>
        /// <param name="isVocabularyTerm">Specify whether the type name ia a vocabulary term type name, since it supports more types.</param>
        /// <returns></returns>
        public static bool IsBuiltinAbstarctTypeName(this string typeName, bool isVocabularyTerm = false)
        {
            bool result;
            switch (typeName)
            {
                case "Edm.PrimitiveType":
                case "Edm.ComplexType":
                case "Edm.EntityType":
                    result = true; break;
                default:
                    result = false; break;
            }

            if (isVocabularyTerm)
            {
                switch (typeName)
                {
                    case "Edm.AnnotationPath":
                    case "Edm.PropertyPath":
                    case "Edm.NavigationPropertyPath":
                        result = true; break;
                    default:
                        result = false; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the slash character number from a target string.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the number of all the slash characters.</returns>
        public static int GetSlashNum(this string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return 0;
            }

            int counter = 0;
            foreach (var slash in target)
            {
                if (slash == '/')
                {
                    counter++;
                }
            }

            return counter;
        }

        /// <summary>
        /// Verify whether there have some empty segments in the segments' array or not.
        /// </summary>
        /// <param name="segments">The segments' array.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool ContainsEmptySegment(this string[] segments)
        {
            if (null == segments || !segments.Any())
            {
                return false;
            }

            foreach (var s in segments)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
