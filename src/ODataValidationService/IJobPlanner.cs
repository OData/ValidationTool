// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Eucritta
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using ODataValidator.RuleEngine;

    #endregion

    interface IJobPlanner
    {
        IEnumerable<ServiceContext> GetPlannedJobs(out List<KeyValuePair<string, string>> failedTargets);
    }
}
