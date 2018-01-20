// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class CapabilitiesModelHelper
    {
        public static IEdmModel GetModelInline(string annotation)
        {
            return GetEdmModel("", annotation);
        }

        public static IEdmModel GetModelOutline(string annotation)
        {
            return GetEdmModel(annotation, "");
        }

        public static IEdmModel GetEdmModel(string inline, string outline)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Calendar"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""RelatedEvents"" Type=""Collection(NS.Event)"" />
      </EntityType>
      <EntityType Name=""Event"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""RelatedCalendar"" Type=""NS.Calendar"" />
      </EntityType>
      <EntityContainer Name=""Default"">
        <EntitySet Name=""Calendars"" EntityType=""NS.Calendar"" >
          <NavigationPropertyBinding Path=""RelatedEvents"" Target=""Events"" />
          {0}
        </EntitySet>
        <EntitySet Name=""Events"" EntityType=""NS.Event"" >
          <NavigationPropertyBinding Path=""RelatedCalendar"" Target=""Calendars"" />
        </EntitySet>
      </EntityContainer>
      {1}
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, inline, outline);

            return CsdlReader.Parse(XElement.Parse(modelText).CreateReader());
        }
    }
}
