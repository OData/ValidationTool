// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public static class URL_SrvDocConstants
    {
        public const string URL_SrvDoc_TripPin = @"http://services.odata.org/v4/TripPinService";
        public const string URL_SrvDoc_OData = @"http://services.odata.org/V4/OData/OData.svc";
        public const string URL_SrcDoc_Conformance = @"http://odatasampleservices.azurewebsites.net/V4/OData/(S(woo55oie4mkbqdjser242w4q))/OData.svc";
    }

    public static class URL_PathConstants
    {
        public const string URL_Path_JsonFormat = @"?$format=application/json";
        public const string URL_Path_AtomFormat = @"?$format=application/atom+xml";
        public const string URL_Path_AtomAbbrFormat = @"?$format=atom";
        public const string URL_Path_XmlFormat = @"?$format=application/xml";
        public const string URL_Path_XmlAbbrFormat = @"?$format=xml";

        public const string URL_Path_Metadata = @"/$metadata";
        public const string URL_Path_Delta = @"/Delta";
        public const string URL_Path_IndividualProperty_Primitive = @"/IndividualProperty_Primitive";
        public const string URL_Path_IndividualProperty_Collection = @"/IndividualProperty_Collection";
        public const string URL_Path_IndividualProperty_Complex = @"/IndividualProperty_Complex";
        public const string URL_Path_EntityReference = @"/EntityReference";
        public const string URL_Path_Error = @"/Error";

        public const string URL_Path_MetadataFull = @";odata.metadata=full";
        public const string URL_Path_MetadataMinimal = @";odata.metadata=minimal";
        public const string URL_Path_MetadataNone = @";odata.metadata=none";
        public const string URL_Path_Entity_IEEE754CompatibleFalse = @";IEEE754Compatible=false";
        public const string URL_Path_Entity_IEEE754CompatibleTrue = @";IEEE754Compatible=true";
        public const string URL_Path_AcceptHeader_StreamingTrue = @"application/json;odata.streaming=true";
        public const string URL_Path_StreamingTrue = @";odata.streaming=true";

        public const string URL_Path_EntitySet_Empty = @"/Empty";
        public const string URL_Path_Entity_Value = @"/$value";
        public const string URL_Path_Entity_Expand = @"?$expand=";
        public const string URL_Path_Entity_AndExpand = @"&$expand=";
        public const string URL_Path_EntitySet_Count = @"?$count=true";
        public const string URL_Path_Count = @"/$count";
        public const string URL_Path_EntitySet_Skip = @"?$skiptoken=";
        public const string URL_Path_EntitySet_Deltatoken = @"?$deltatoken=";
    }

    public static class AtomDataSvc_URLConstants
    {
        public const string URL_ServiceDocument = URL_SrvDocConstants.URL_SrvDoc_OData;

        public const string EntitySetName_Products = "/Products";
        public const string EntityName_Product = EntitySetName_Products + "(0)";
        public const string EntitySetName_PersonDetails = "/PersonDetails";
        public const string EntityName_PersonDetail = EntitySetName_PersonDetails + "(0)";
        public const string EntityName_FeaturedProduct = "/Products/ODataDemo.FeaturedProduct(9)";
        public const string EntityName_Person = "/Persons(3)";
        public const string EntityReference_SupplierInProduct = "/Products(0)/Supplier/$ref";
        public const string EntityReference_CategoriesInProduct = "/Products(1)/Categories/$ref";

        public const string IndividualProperty_NameInProduct = "/Products(0)/Name";
        public const string IndividualProperty_DiscontinuedDateInProduct = "/Products(0)/DiscontinuedDate";
        public const string IndividualProperty_ReleaseDateInProduct = "/Products(0)/ReleaseDate";
        public const string IndividualProperty_CollectionPrimitive = "/Suppliers(0)/Telephones";
        public const string IndividualProperty_CollectionDerivedComplex = "/AllAddresses";

        public const string URL_Metadata = URL_ServiceDocument + URL_PathConstants.URL_Path_Metadata;

        public const string URL_ServiceDocumentWithXMLFormat = URL_SrvDocConstants.URL_SrvDoc_OData + URL_PathConstants.URL_Path_XmlFormat;
        public const string URL_ServiceDocumentWithAtomAbbrFormat = URL_SrvDocConstants.URL_SrvDoc_OData + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_ServiceDocumentWithAtomFormat = URL_SrvDocConstants.URL_SrvDoc_OData + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_Entity_Product_ExpandAll = URL_ServiceDocument + EntityName_Product + URL_PathConstants.URL_Path_Entity_Expand + "Categories,Supplier,ProductDetail";
        public const string URL_Entity_Product = URL_ServiceDocument + EntityName_Product + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_Entity_Product_AtomFormat = URL_ServiceDocument + EntityName_Product + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_Entity_Product_XmlAbbrFormat = URL_ServiceDocument + EntityName_Product + URL_PathConstants.URL_Path_XmlAbbrFormat;
        public const string URL_Entity_Product_XmlFormat = URL_ServiceDocument + EntityName_Product + URL_PathConstants.URL_Path_XmlFormat;
        public const string URL_Entity_PersonDetail = URL_ServiceDocument + EntityName_PersonDetail + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_Entity_FeaturedProduct = URL_ServiceDocument + EntityName_FeaturedProduct + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_Entity_Person = URL_ServiceDocument + EntityName_Person + URL_PathConstants.URL_Path_AtomAbbrFormat;

        public const string URL_EntitySet_Products = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_EntitySet_ProductsWithAtomFormat = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_EntitySet_ProductsWithXmlFormat = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_XmlFormat;
        public const string URL_EntitySet_ProductsWithXmlAbbrFormat = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_XmlAbbrFormat;
        public const string URL_EntitySet_ProductsSkip = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_EntitySet_Skip + "11";
        public const string URL_EntitySet_ProductsCount = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_Count;
        public const string URL_EntitySet_ProductsDelta = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_EntitySet_Deltatoken + "1234";

        public const string URL_IndividualProperty_Primitive = URL_ServiceDocument + URL_PathConstants.URL_Path_IndividualProperty_Primitive + URL_PathConstants.URL_Path_XmlAbbrFormat;
        public const string URL_IndividualProperty_PrimitiveString = URL_ServiceDocument + IndividualProperty_NameInProduct;
        public const string URL_IndividualProperty_PrimitiveStringWithAtomAbbrFormat = URL_ServiceDocument + IndividualProperty_NameInProduct + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_IndividualProperty_PrimitiveStringWithAtomFormat = URL_ServiceDocument + IndividualProperty_NameInProduct + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_IndividualProperty_PrimitiveStringWithXmlFormat = URL_ServiceDocument + IndividualProperty_NameInProduct + URL_PathConstants.URL_Path_XmlFormat;
        public const string URL_IndividualProperty_NullPrimitiveString = URL_ServiceDocument + IndividualProperty_DiscontinuedDateInProduct;
        public const string URL_IndividualProperty_PrimitiveNonString = URL_ServiceDocument + IndividualProperty_ReleaseDateInProduct;

        public const string URL_IndividualProperty_CollectionPrimitive = URL_ServiceDocument + IndividualProperty_CollectionPrimitive;
        public const string URL_IndividualProperty_CollectionDerivedComplex = URL_ServiceDocument + IndividualProperty_CollectionDerivedComplex;
        public const string URL_EntityReferenceSingle = URL_ServiceDocument + EntityReference_SupplierInProduct + URL_PathConstants.URL_Path_XmlAbbrFormat;
        public const string URL_EntityReferenceSingleWithAtomFormat = URL_ServiceDocument + EntityReference_SupplierInProduct + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_EntityReferenceSingleWithAtomAbbrFormat = URL_ServiceDocument + EntityReference_SupplierInProduct + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_EntityReferenceCollectionWithAtomFormat = URL_ServiceDocument + EntityReference_CategoriesInProduct + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_EntityReferenceCollection = URL_ServiceDocument + EntityReference_CategoriesInProduct + URL_PathConstants.URL_Path_AtomAbbrFormat;

        public const string URL_Error = URL_ServiceDocument + URL_PathConstants.URL_Path_Error;
        public const string URL_ErrorWithAtomFormat = URL_ServiceDocument + URL_PathConstants.URL_Path_Error + URL_PathConstants.URL_Path_AtomFormat;

        public const string URL_Delta = URL_ServiceDocument + URL_PathConstants.URL_Path_Delta;
    }

    public static class ODataSvc_URLConstants
    {
        public const string EntitySetName_Products = "/Products";
        public const string EntityName_Products = EntitySetName_Products + "(0)";
        public const string EntitySetName_PersonDetails = "/PersonDetails";
        public const string EntityName_PersonDetails = EntitySetName_PersonDetails + "(0)";
        public const string EntityName_FeaturedProduct = "/Products/ODataDemo.FeaturedProduct(9)";
        public const string EntityName_Persons = "/Persons(3)";

        public const string URL_ServiceDocument = URL_SrvDocConstants.URL_SrvDoc_OData;

        public const string URL_Entity_Products_Full = URL_ServiceDocument + EntityName_Products + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_PersonDetails_Full = URL_ServiceDocument + EntityName_PersonDetails + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_FeaturedProduct_Full = URL_ServiceDocument + EntityName_FeaturedProduct + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;

        public const string URL_Entity_Persons_Full = URL_ServiceDocument + EntityName_Persons + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_Persons_Full_IEEE754CompatibleFalse = URL_ServiceDocument + EntityName_Persons + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleFalse;
        public const string URL_Entity_Persons_Full_IEEE754CompatibleTrue = URL_ServiceDocument + EntityName_Persons + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue;

        public const string URL_EntitySet_Products_Full = URL_ServiceDocument + EntitySetName_Products + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;

    }

    public static class TripPinSvc_URLConstants
    {
        public const string EntitySetName_People = "/People";
        public const string EntityName_People = EntitySetName_People + "('russellwhyte')";
        public const string EntitySetName_Photos = "/Photos";
        public const string EntityName_Photos = EntitySetName_Photos + "(1)";
        public const string ExpandCollectionName_Friends = "Friends";
        public const string ExpandCollectionName_Photo = "Photo";
        public const string ExpandCollectionName_Trips = "Trips";
        public const string EntityName_People_ExpandNull = EntitySetName_People + "('clydeguess')";
        public const string EntityName_Airports = "/Airports('KSFO')";

        public const string URL_ServiceDocument = URL_SrvDocConstants.URL_SrvDoc_TripPin;
        public const string URL_ServiceDocumentWithJsonFormat = URL_ServiceDocument + URL_PathConstants.URL_Path_JsonFormat;

        public const string URL_Metadata = URL_ServiceDocument + URL_PathConstants.URL_Path_Metadata;

        public const string URL_Entity_People_Full = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_People_MinimalWithFormat = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_JsonFormat;
        public const string URL_Entity_People_AtomFormat = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_AtomFormat;
        public const string URL_Entity_People_AtomAbbrFormat = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_AtomAbbrFormat + URL_PathConstants.URL_Path_Entity_AndExpand + ExpandCollectionName_Trips;
        public const string URL_Entity_People_Minimal_WithUncomputedLink = URL_ServiceDocument + EntityName_People;
        public const string URL_Entity_People_Minimal = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataMinimal;
        public const string URL_Entity_People_None = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataNone;


        public const string URL_Entity_Photos_Full = URL_ServiceDocument + EntityName_Photos + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_Photos_AtomAbbrFormat = URL_ServiceDocument + EntityName_Photos + URL_PathConstants.URL_Path_AtomAbbrFormat;
        public const string URL_Entity_Airports_Full = URL_ServiceDocument + EntityName_Airports + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_Entity_Airports_AtomAbbrFormat = URL_ServiceDocument + EntityName_Airports + URL_PathConstants.URL_Path_AtomAbbrFormat;

        public const string URL_Entity_People_IEEE754CompatibleFalse = URL_Entity_People_Full + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleFalse;
        public const string URL_Entity_People_IEEE754CompatibleTrue = URL_Entity_People_Full + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue;
        public const string URL_Entity_People_FullWithAllParams = URL_Entity_People_Full + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue + URL_PathConstants.URL_Path_StreamingTrue;
        public const string URL_Entity_People_Value = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_Entity_Value;
        public const string URL_Entity_People_Expand_MultiEntity = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_Entity_Expand + ExpandCollectionName_Friends;
        public const string URL_Entity_People_Expand_OneEntity = URL_ServiceDocument + EntityName_People + URL_PathConstants.URL_Path_Entity_Expand + ExpandCollectionName_Photo;
        public const string URL_Entity_People_Expand_NullEntity = URL_ServiceDocument + EntityName_People_ExpandNull + URL_PathConstants.URL_Path_Entity_Expand + ExpandCollectionName_Photo;
        public const string URL_Entity_People_Expand_NullEntity_Array = URL_ServiceDocument + EntityName_People_ExpandNull + URL_PathConstants.URL_Path_Entity_Expand + ExpandCollectionName_Friends;
        public const string URL_Entity_People_Expand_Friends = URL_ServiceDocument + EntityName_People + "/" + ExpandCollectionName_Friends;
        public const string URL_Entity_People_Expand_Trips = URL_ServiceDocument + EntityName_People + "/" + ExpandCollectionName_Trips;
        public const string URL_Entity_People_Expand_Photo = URL_ServiceDocument + EntityName_People + "/" + ExpandCollectionName_Photo;

        public const string URL_EntitySet_People_Full = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;
        public const string URL_EntitySet_People_Minimal = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataMinimal;
        public const string URL_EntitySet_People_IEEE754CompatibleFalse = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleFalse;
        public const string URL_EntitySet_People_IEEE754CompatibleTrue = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue;
        public const string URL_EntitySet_People_JsonFormat = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_JsonFormat;
        public const string URL_EntitySet_People_Count = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_EntitySet_Count;
        public const string URL_EntitySet_People_Skip8 = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_EntitySet_Count + URL_PathConstants.URL_Path_EntitySet_Skip + "8";
        public const string URL_EntitySet_People_Skip16 = URL_ServiceDocument + EntitySetName_People + URL_PathConstants.URL_Path_EntitySet_Count + URL_PathConstants.URL_Path_EntitySet_Skip + "16";
        public const string URL_EntitySet_Empty = URL_ServiceDocument + URL_PathConstants.URL_Path_EntitySet_Empty;
        public const string URL_EntitySet_Photos = URL_ServiceDocument + EntitySetName_Photos + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull;

        public const string URL_IndividualProperty_Primitive = URL_ServiceDocument + URL_PathConstants.URL_Path_IndividualProperty_Primitive;
        public const string URL_IndividualProperty_Collection = URL_ServiceDocument + URL_PathConstants.URL_Path_IndividualProperty_Collection;
        public const string URL_IndividualProperty_Complex = URL_ServiceDocument + URL_PathConstants.URL_Path_IndividualProperty_Complex;
        public const string URL_EntityReference = URL_ServiceDocument + URL_PathConstants.URL_Path_EntityReference;
        public const string URL_Error = URL_ServiceDocument + URL_PathConstants.URL_Path_Error;
        public const string URL_Delta = URL_ServiceDocument + URL_PathConstants.URL_Path_Delta;

        public const string URL_IndividualPropertyWithAllParams = URL_IndividualProperty_Primitive + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue + URL_PathConstants.URL_Path_StreamingTrue;
        public const string URL_EntityReferenceWithAllParams = URL_EntityReference + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue + URL_PathConstants.URL_Path_StreamingTrue;
        public const string URL_ErrorWithAllParams = URL_Error + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue + URL_PathConstants.URL_Path_StreamingTrue;
        public const string URL_DeltaWithAllParams = URL_Delta + URL_PathConstants.URL_Path_JsonFormat + URL_PathConstants.URL_Path_MetadataFull + URL_PathConstants.URL_Path_Entity_IEEE754CompatibleTrue + URL_PathConstants.URL_Path_StreamingTrue;
    }
}
