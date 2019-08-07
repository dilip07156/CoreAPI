using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductTemplates
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string VoyagerProductTemplate_Id { get; set; }
        public int? Subprod_Id { get; set; }
        public string SubProd { get; set; }
        public string Name { get; set; }
        public string Busitypes { get; set; }
        public string Persmax { get; set; }
        public bool? HOTELROOM { get; set; }
        public bool? IsStandardRoom { get; set; }
        public string ProductType_Id { get; set; }
        public string ParentProductTemplate_Id { get; set; }
        public string ParentSubProd { get; set; }
        public string ParentName { get; set; }
        public bool? Busferry { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
    public class ProductTemplatesGetReq
    {
        public string VoyagerProductTemplate_Id { get; set; }
    }

    public class ProductTemplatesGetRes
    {
        public List<mProductTemplates> ProductTemplates { get; set; } = new List<mProductTemplates>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
