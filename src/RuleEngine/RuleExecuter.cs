// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class to execute validation rules and feed results to resule consumer
    /// </summary>
    public class RuleExecuter
    {
        /// <summary>
        /// The result provider to consume validation test results
        /// </summary>
        private IResultProvider resultProvider;

        /// <summary>
        /// The logger to log significant runtime data (such as runtime exceptions)
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates the object representing the validation execution
        /// </summary>
        /// <param name="provider">result consumer object</param>
        public RuleExecuter(IResultProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider", Resource.NullResultProvider);
            }

            this.resultProvider = provider;
            this.logger = null;
        }

        /// <summary>
        /// Creates a new instance of RuleExecuter from result provider and logger
        /// </summary>
        /// <param name="provider">Result provider to consume validation results.</param>
        /// <param name="logger">logger to log runtime data.</param>
        public RuleExecuter(IResultProvider provider, ILogger logger)
            : this(provider)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Executes one rule to validate the current interop request contecxt
        /// </summary>
        /// <param name="context">the current interop request context</param>
        /// <param name="rule">the rule to be validated</param>
        /// <returns>TestResult object of the validation</returns>
        /// <exception cref="Exception">Throws various exception when rule engine encounters unrecoverable errors like bad dynamic rules</exception>
        public static TestResult ExecuteRule(ServiceContext context, Rule rule)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            bool passed = true;
            TestResult result;

            TestResult dummy;

            //Set the test service's current verified rule
            if (DataService.serviceInstance != null)
            {
                DataService.serviceInstance.SwitchRule(rule.Name);
            }

            if (rule.Condition == null || rule.Condition.Verify(context, out dummy))
            {
                passed = rule.Action.Verify(context, out result);
            }
            else
            {
                result = new TestResult();
            }

            result.JobId = context.JobId;
            result.SetProperties(rule, passed);

            return result;
        }

        /// <summary>
        /// Executes specified rules to validate the current interop request context
        /// </summary>
        /// <param name="context">current interop request context</param>
        /// <param name="rules">rules to be validated</param>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public void Execute(ServiceContext context, IEnumerable<Rule> rules)
        {
            if (context == null)
            {
                this.resultProvider.JobCompleted(true);
                var e = new RuntimeException(new ArgumentNullException("context"), Resource.NullServiceContext);
                this.LogRuntimeError(e);
                return;
            }

            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            bool errorOccurred = false;
            try
            {
                foreach (var rule in rules)
                {
                    TestResult result;
                    try
                    {
                        result = RuleExecuter.ExecuteRule(context, rule);
                    }
                    catch (RuntimeException e)
                    {
                        errorOccurred = true;
                        result = TestResult.CreateAbortedResult(rule, context.JobId);
                        e.JobId = context.JobId;
                        e.RuleName = rule.Name;
                        e.DestinationEndpoint = context.Destination.AbsolutePath;
                        this.LogRuntimeError(e);
                    }
                    catch (Exception ex)
                    {
                        if (!ExceptionHelper.IsCatchableExceptionType(ex))
                        {
                            throw;
                        }

                        errorOccurred = true;
                        result = TestResult.CreateAbortedResult(rule, context.JobId);

                        var e = new RuntimeException(ex, null);
                        e.JobId = context.JobId;
                        e.RuleName = rule.Name;
                        e.DestinationEndpoint = context.Destination.AbsolutePath;
                        this.LogRuntimeError(e);
                    }

                    this.resultProvider.Accept(result);
                }

                this.resultProvider.JobCompleted(errorOccurred);
            }
            catch (Exception ex)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    throw;
                }

                this.resultProvider.JobCompleted(true);
            }
        }

        /// <summary>
        /// Logs runtime failures
        /// </summary>
        /// <param name="e">The runtime exception generated by interop rule engine.</param>
        private void LogRuntimeError(RuntimeException e)
        {
            if (this.logger != null)
            {
                this.logger.Log(e);
            }
        }
    }
}
