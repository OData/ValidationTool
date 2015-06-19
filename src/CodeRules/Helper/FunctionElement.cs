// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The FunctionElement class.
    /// </summary>
    public class FunctionElement
    {
        /// <summary>
        /// Parse an element with XElement type to a FunctionElement instance.
        /// </summary>
        /// <param name="xElem">An element with XElement type.</param>
        /// <returns>Returns a FunctionElement instance.</returns>
        public static FunctionElement Parse(XElement xElem)
        {
            if (null == xElem && "Function" == xElem.Name.LocalName)
            {
                return null;
            }

            string name = string.Empty;
            bool isBound = false;
            string bindingParamName = string.Empty;
            List<Parameter> parameters = new List<Parameter>();
            string returnType = string.Empty;

            if (null != xElem.Attribute("Name"))
            {
                name = xElem.GetAttributeValue("Name");
            }

            if (null != xElem.Attribute("IsBound"))
            {
                isBound = Convert.ToBoolean(xElem.GetAttributeValue("IsBound"));
            }

            string xPath = "./*[local-name()='Parameter']";
            var paramElems = xElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != paramElems && paramElems.Any())
            {
                bool once = true;
                foreach (var paramElem in paramElems)
                {
                    if (isBound && once)
                    {
                        once = false;
                        if (null != paramElem.Attribute("Name"))
                        {
                            bindingParamName = paramElem.GetAttributeValue("Name");
                        }

                        continue;
                    }

                    if (null != paramElem.Attribute("Name") && null != paramElem.Attribute("Type"))
                    {
                        var parameter = new Parameter(paramElem.GetAttributeValue("Name"), paramElem.GetAttributeValue("Type"));
                        parameters.Add(parameter);
                    }
                }
            }

            xPath = "./*[local-name()='ReturnType']";
            var returnTypeElem = xElem.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != returnTypeElem && null != returnTypeElem.Attribute("Type"))
            {
                returnType = returnTypeElem.GetAttributeValue("Type");
            }

            var functionElement = new FunctionElement(name, isBound, bindingParamName, parameters, returnType);

            return functionElement;
        }

        /// <summary>
        /// Verify whether the FunctionElement instance is unique in a set of FunctionElement instance.
        /// </summary>
        /// <param name="functionElement">A FunctionElement instance.</param>
        /// <param name="functionElements">A set of FunctionElement instances.</param>
        /// <param name="type">The type of verified parameter type (Distinguish with an unordered set Of parameter names and an ordered set of parameter types.).</param>
        /// <returns>Returns the verification result.</returns>
        public static bool Unique(FunctionElement functionElement, IEnumerable<FunctionElement> functionElements, VerifiedParamType type)
        {
            int counter = 0;
            foreach (var functionElem in functionElements)
            {
                if (functionElement.name != functionElem.name)
                {
                    continue;
                }

                if (functionElement.isBound != functionElem.isBound)
                {
                    continue;
                }

                if (functionElement.bindingParamName != functionElem.bindingParamName)
                {
                    continue;
                }

                if (functionElement.parameters.Count != functionElem.parameters.Count)
                {
                    continue;
                }

                // Verify the unordered set of non-binding parameter names and the ordered set of parameter types.
                bool flag = true;
                for (int i = 0; i < functionElement.parameters.Count; i++)
                {
                    if (type == VerifiedParamType.UnorderedSetOfParamNames || type == VerifiedParamType.All)
                    {
                        if (!functionElem.parameters.Contains(functionElement.parameters[i]))
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (type == VerifiedParamType.OrderedSetOfParamTypes || type == VerifiedParamType.All)
                    {
                        if (functionElement.parameters[i] != functionElem.parameters[i])
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (!flag)
                {
                    continue;
                }

                if (functionElement.returnType != functionElem.returnType)
                {
                    continue;
                }

                counter++;
            }

            return counter == 1;
        }

        /// <summary>
        /// Get the name of the function.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the verification result whether the function is bound function or not.
        /// </summary>
        public bool IsBound
        {
            get
            {
                return this.isBound;
            }
        }

        /// <summary>
        /// Gets the binding parameter name of the function.
        /// </summary>
        public string BindingParamName
        {
            get
            {
                return this.bindingParamName;
            }
        }

        /// <summary>
        /// Gets the parameters of the funciton.
        /// </summary>
        public List<Parameter> Parameter
        {
            get
            {
                return this.parameters;
            }
        }

        /// <summary>
        /// Gets the return type of the function.
        /// </summary>
        public string ReturnType
        {
            get
            {
                return this.returnType;
            }
        }

        /// <summary>
        /// The name of the function.
        /// </summary>
        private string name;

        /// <summary>
        /// Indicate whether the function is a bound function or not.
        /// </summary>
        private bool isBound;

        /// <summary>
        /// The binding parameter name of the funciton.
        /// </summary>
        private string bindingParamName;

        /// <summary>
        /// The parameter information of the function.
        /// </summary>
        private List<Parameter> parameters;

        /// <summary>
        /// The return type of the function.
        /// </summary>
        private string returnType;

        /// <summary>
        /// The private constructor of the FunctionElement class.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="isBound">The IsBound attribute of the function.</param>
        /// <param name="bindingParamName">The binding parameter name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        private FunctionElement(string name, bool isBound, string bindingParamName, List<Parameter> parameters, string returnType)
        {
            this.name = name;
            this.isBound = isBound;
            this.bindingParamName = bindingParamName;
            this.parameters = parameters;
            this.returnType = returnType;
        }
    }

    /// <summary>
    /// The Parameter class.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// The constructor of the Parameter class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public Parameter(string name, string type)
        {
            this.name = name;
            this.type = type;
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public string Type
        {
            get
            {
                return this.type;
            }
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        private string name;

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        private string type;
    }

    /// <summary>
    /// The VerifiedParamType enumeration.
    /// </summary>
    [Flags]
    public enum VerifiedParamType
    {
        None = 0x0,
        UnorderedSetOfParamNames = 0x1,
        OrderedSetOfParamTypes = 0x2,
        All = 0x3
    }
}
