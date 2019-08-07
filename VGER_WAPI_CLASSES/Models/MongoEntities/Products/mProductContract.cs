using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductContract
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductContract_Id { get; set; }
        public string ProductSupplier_Id { get; set; }
        public string Supplier_Id { get; set; }
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public string Product_Id { get; set; }
        public string BusinessType { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FromDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ToDate { get; set; }
        public string Currency_Id { get; set; }
        public string ContractType { get; set; }
        public string Agent_Id { get; set; }
        public string Agent { get; set; }
        public string BuySellContractType { get; set; }
        public string BuyContract_Id { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
