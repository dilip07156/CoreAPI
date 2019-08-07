using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mGenericImages
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string ImageType { get; set; }
        public string ImageSubType { get; set; }
        public string ImageName { get; set; }
        public string ImageURL { get; set; }
        public string ImageExt { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
