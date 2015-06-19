// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    #endregion

    /// <summary>
    /// Singleton class of Interop rule engine settings
    /// </summary>
    public class RuleEngineSetting
    {
        /// <summary>
        /// The unique instance of this class.
        /// </summary>
        private static RuleEngineSetting instance = new RuleEngineSetting();

        /// <summary>
        /// The data member of maximum payload size
        /// </summary>
        private int defaultMaximumPayloadSize = 1024 * 1024;

        /// <summary>
        /// Creates an instahnce of RuleEngineSetting.
        /// Being private to thward attempts to create objects other than the singleton one.
        /// </summary>
        private RuleEngineSetting()
        {
        }

        /// <summary>
        /// Gets/Sets the default payload size of response payload.
        /// </summary>
        public int DefaultMaximumPayloadSize
        {
            get
            {
                return this.defaultMaximumPayloadSize;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(Resource.ArgumentNegativeOrZero);
                }

                this.defaultMaximumPayloadSize = value;
            }
        }

        /// <summary>
        /// Returns the singleton object of RuleEngineSetting.
        /// </summary>
        /// <returns>The singleton object</returns>
        public static RuleEngineSetting Instance()
        {
            return RuleEngineSetting.instance;
        }
    }
}
