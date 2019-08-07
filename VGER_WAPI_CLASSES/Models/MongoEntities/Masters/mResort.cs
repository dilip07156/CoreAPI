using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class mResort
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string Voyager_Resort_Id { get; set; }
        public string ResortName { get; set; }
        public string ResortCode { get; set; }
        public string Voyager_Parent_Resort_Id { get; set; }
        public string ParentResortName { get; set; }
        public string ParentResortCode { get; set; }
        public string Voyager_ResortType_Id { get; set; }
        public string ResortType { get; set; }
        public string Lookup { get; set; }
        public string Nation { get; set; }
        public string Language { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string BuyCurrency_Id { get; set; } 
        public int? Slvatid { get; set; }
        public string SAGEVAT { get; set; }
        public string SAGEVATNAME { get; set; }
        public decimal? VATRATE { get; set; }
        public bool? KeyResort { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
        public string ImageURL { get; set; }
    }
}
