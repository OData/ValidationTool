// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System.Collections.Generic;

    public class IntegerCompare : IComparer<int>
    {
        public IntegerCompare(SortedType sortedType)
        {
            this.type = sortedType;
        }

        public int Compare(int x, int y)
        {
            x = 0 > x ? int.MaxValue : x;
            y = 0 > y ? int.MaxValue : y;
            return SortedType.ASC == this.type ? x.CompareTo(y) : -x.CompareTo(y);
        }

        private SortedType type;
    }
}
