using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductLevelAttribute
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string ProductAttribute_id { get; set; }
        public string Product_Id { get; set; }
        public string Attribute_id { get; set; }
        public string AttributeValues_Id { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
