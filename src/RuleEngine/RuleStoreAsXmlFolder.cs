// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class that aggregate all xml files under the specified folder
    /// </summary>
    public class RuleStoreAsXmlFolder : IRuleStore
    {
        /// <summary>
        /// Path of the folder where all the xml files reside 
        /// </summary>
        private string folder;

        /// <summary>
        /// Logger object to log runtime errors
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates an instance of RuleStoreAsXmlFolder class from the folder path.
        /// </summary>
        /// <param name="path">The file system folder containing all the validation rule XML files</param>
        public RuleStoreAsXmlFolder(string path)
            : this(path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of RuleStoreAsXmlFolder class from the folder path and ILogger object.
        /// </summary>
        /// <param name="path">The file system folder containing all the validation rule XML files</param>
        /// <param name="logger">The ILogger object to log runtime errors</param>
        public RuleStoreAsXmlFolder(string path, ILogger logger)
        {
            this.folder = path;
            this.logger = logger;
        }

        /// <summary>
        /// Gets all rules from the rule store
        /// </summary>
        /// <returns>Collection of rules</returns>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public IEnumerable<Rule> GetRules()
        {
            List<Rule> rules = new List<Rule>();

            try
            {
                DirectoryInfo di = new DirectoryInfo(this.folder);
                FileInfo[] fis = di.GetFiles("*.xml");
                foreach (var fi in fis)
                {
                    var rulesInFile = new RuleStoreAsXmlFile(fi.FullName, this.logger).GetRules();
                    if (rulesInFile != null)
                    {
                        rules.AddRange(rulesInFile);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    throw;
                }

                RuntimeException.WrapAndLog(ex, Resource.InvalidFolderPath + ":" + this.folder, this.logger);
            }

            return rules;
        }
    }
}
