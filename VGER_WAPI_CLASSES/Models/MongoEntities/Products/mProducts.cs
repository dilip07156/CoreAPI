using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProducts
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProduct_Id { get; set; }
        public string ProductType_Id { get; set; }
        public string ProductCode { get; set; }
        public bool? Placeholder { get; set; }
        public string ProdName { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string ProductType { get; set; }
        public string CurrencyName { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string PostCodeExt { get; set; }
        public string Resort_Id { get; set; }
        public string ParentResort_Id { get; set; }
        public string Region_Id { get; set; }
        public string Currency_Id { get; set; }
        public string Status { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public string Supptel { get; set; }
        public string Suppfax { get; set; }
        public string Suppmail { get; set; }
        public string Suppweb { get; set; }
        public string ProdDesc { get; set; }
        public string InternalDesc { get; set; }
        public string CustomerDesc { get; set; }
        public string ItinearyDesc { get; set; }
        public string QuotationDesc { get; set; }
        public string Area { get; set; }
        public string RestaurantInfo { get; set; }
        public string MeetingRooms { get; set; }
        public string Location { get; set; }
        public string StarRating { get; set; }
        public string BdgPriceCategory { get; set; }
        public string Chain { get; set; }
        public string HotelType { get; set; }
        public string NoOfRooms { get; set; }
        public string FullAddress { get; set; }
        public List<ProductAttributes> ProductAttributes { get; set; }
        public List<ArrProductFacilities> ProductFacilities { get; set; }
        public List<ArrProductResources> ProductResources { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
        public string EzeegoId { get; set; }
    }

    public class ProductAttributes
    {
        public string AttributeGroupId { get; set; }
        public string AttributeGroupName { get; set; }
        public List<ProdAttributeValues> AttributeValues { get; set; }
    }

    public class ProdAttributeValues
    {
        public string AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValueId { get; set; }
        public string AttributeValue { get; set; }
    }
}

