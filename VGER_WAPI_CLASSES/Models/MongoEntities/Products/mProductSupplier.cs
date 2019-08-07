using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class mProductSupplier
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductSupplier_Id { get; set; }
        public int? Prodsuppid { get; set; }
        public string Product_Id { get; set; }
        public int? Productid { get; set; }
        public string Company_Id { get; set; }
        public int? Partnerid { get; set; }
        public string Status { get; set; }
        public string SalesManager_Id { get; set; }
        public string Fitcontact_Id { get; set; }
        public string GroupContact_Id { get; set; }
        public string AccountContact_Id { get; set; }
        public string SupplierNote { get; set; }
        public string Via { get; set; }
        public bool? DafaultSupplier { get; set; }
        public string Currency_Id { get; set; }
        public string ForAgent_Id { get; set; }
        public string PackageContact_Id { get; set; }
        public string EndSupplier_Id { get; set; }
        public string CopiedProductSupplier_Id { get; set; }
        public bool? IsPreferred { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActiveFrom { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActiveTo { get; set; }
        public string EmergencyContact_Id { get; set; }
        public string ComplaintsContact_Id { get; set; }
        public bool? IsAllOperatingMarket { get; set; }
        public bool? IsAllSalesMarket { get; set; }
        public bool? IsAllSalesAgent { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
