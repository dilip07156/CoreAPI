using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mProducts_Lite
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string VoyagerProduct_Id { get; set; }
        public string ProductType_Id { get; set; }
        public bool? Placeholder { get; set; }
        public string ProdName { get; set; }
        public string ProductCode { get; set; }
        public string DefaultSupplierId { get; set; }
        public string DefaultSupplier { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string Status { get; set; }
        public string ProdDesc { get; set; }
        public string Location { get; set; }
        public string StarRating { get; set; }
        public string BdgPriceCategory { get; set; }
        public string Chain { get; set; }
        public string HotelType { get; set; }
        public string HotelImageURL { get; set; }
        public List<Def_Facilities> ProductFacilities { get; set; }
        public List<ProductRoomTypes> Rooms { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
