using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mTransportMaster
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerTransportMaster_Id { get; set; }
        public List<Coaches> Coaches { get; set; }
        public List<Brand> Brand { get; set; }
    }

    public class Coaches
    {
        public string Type { get; set; }
    }

    public class Brand
    {
        public string BrandName { get; set; }
    }
}
