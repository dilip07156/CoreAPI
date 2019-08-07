using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductRange
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductRange_Id { get; set; }
        public string ProductRangeName { get; set; }
        public string ProductRangeCode { get; set; }
        public string Product_Id { get; set; }
        public string ProductType_Id { get; set; }
        public string PersonType_Id { get; set; }
        public string PersonType { get; set; }
        public string Agemin { get; set; }
        public string Agemax { get; set; }
        public string MaxOccupancy { get; set; }
        public string MaxAdults { get; set; }
        public string MaxChildren { get; set; }
        public string ChildrenSameBed { get; set; }
        public string PriceLike_Id { get; set; }
        public string AllocateBy_Id { get; set; }
        public DateTime? Datemin { get; set; }
        public DateTime? Datemax { get; set; }
        public string DefCategory_Id { get; set; }
        public string ProductCategory_Id { get; set; }
        public int? Quantity { get; set; }
        public bool IsMarkUpChargeable { get; set; }
        public bool? AdditionalYn { get; set; }
        public string Descriptions { get; set; }
        public string ChargeBasis { get; set; }
        public string ProductMenu_Id { get; set; }
        public string ProductMenu { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
