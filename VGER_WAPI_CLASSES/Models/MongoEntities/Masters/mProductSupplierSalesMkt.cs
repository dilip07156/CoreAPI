using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mProductSupplierSalesMkt
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string ProductSupplierSalesMkt_Id { get; set; }
        public string Product_Id { get; set; }
        public string ProductSupplier_Id { get; set; }
        public string BusinessRegion_Id { get; set; }
    }
}
