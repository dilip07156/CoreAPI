using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductFreePlacePolicy
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string ProductFreePlacePolicy_Id { get; set; }
        public string Product_Id { get; set; }
        public string ProductContract_Id { get; set; }
        public string ProductTemplate_Id { get; set; }
        public string Subprod { get; set; }
        public DateTime DateMin { get; set; }
        public DateTime DateMax { get; set; }
        public int MinPers { get; set; }
        public int Quantity { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
