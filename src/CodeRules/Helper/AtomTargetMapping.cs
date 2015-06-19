// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System.Collections.Generic;

    /// <summary>
    /// Help class to map OData keyword to Atom specific target path
    /// </summary>
    public static class AtomTargetMapping
    {
        static readonly Dictionary<string, string> mapAtomTarget = new Dictionary<string, string>()
        {
            {"SyndicationAuthorName", "atom:author/atom:name"},
            {"SyndicationAuthorEmail", "atom:author/atom:email"},
            {"SyndicationAuthorUri", "atom:author/atom:uri"},
            {"SyndicationPublished", "atom:published"},
            {"SyndicationRights", "atom:rights"},
            {"SyndicationTitle", "atom:title"},
            {"SyndicationUpdated", "atom:updated"},
            {"SyndicationContributorName", "atom:contributor/atom:name"}, 
            {"SyndicationContributorEmail", "atom:contributor/atom:email"},
            {"SyndicationContributorUri", "atom:contributor/atom:uri"},
            {"SyndicationSource", "atom:source"},
            {"SyndicationSummary", "atom:summary"}
        };

        static public bool IsAtomSpecificTarget(string input)
        {
            return mapAtomTarget.ContainsKey(input);
        }

        static public bool TryGetTarget(string input, out string target)
        {
            return mapAtomTarget.TryGetValue(input, out target);
        }
    }
}
