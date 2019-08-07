using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductPrice
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductPrice_Id { get; set; }
        public int Priceid { get; set; }
        public string PricePeriod_Id { get; set; }
        public int Pricperiid { get; set; }
        public string Product_Id { get; set; }
        public int Productid { get; set; }
        public string ProductRange_Id { get; set; }
        public string Sub2prodid { get; set; }
        public string Prictype { get; set; }
        public string Price { get; set; }
        public string Currency { get; set; }
        public string Condid { get; set; }
        public string Currency_Id { get; set; }
        public string Status { get; set; }
        public string MealPlan_Id { get; set; }
        public string ProductHub_Id { get; set; }
        public string ContractSlab_Id { get; set; }
        public string ProductStarRating_Id { get; set; }
        public string ChargeBasis { get; set; }
        public bool OptionalYn { get; set; }
        public string InclusiveTaxYn { get; set; }
        public string NetNetPrice { get; set; }
        public string BuyProductPrice_Id { get; set; }
        public string MinPax { get; set; }
        public string MaxPax { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
