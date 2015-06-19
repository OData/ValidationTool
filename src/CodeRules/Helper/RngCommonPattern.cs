// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    internal class RngCommonPattern
    {
        public const string CommonPatterns = @"
<define name=""anyAttribute"">
      <attribute>
        <anyName />
      </attribute>
    </define>

    <define name=""anyAttributes"">
      <zeroOrMore>
        <ref name=""anyAttribute"" />
      </zeroOrMore>
    </define>

    <define name=""anyContent"">
      <ref name=""anyAttributes"" />
      <mixed>
        <ref name=""anyElements""/>
      </mixed>
    </define>

    <define name=""anyElement"">
      <element>
        <anyName />
        <ref name=""anyAttributes"" />
        <mixed>
          <ref name=""anyElements""/>
        </mixed>
      </element>
    </define>

    <define name=""anyElements"">
      <zeroOrMore>
        <ref name=""anyElement"" />
      </zeroOrMore>
    </define>
"; 
    }
}
