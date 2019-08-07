using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class RoutingInfo
    {
        public long RouteID { get; set; }
        public string FromCityID { get; set; }
        public string FromCityName { get; set; }
        public string ToCityID { get; set; } 
        public string ToCityName { get; set; } 
        public int Days { get; set; } 
        public int Nights { get; set; } 
        public bool IsLocalGuide { get; set; }
        public string PrefStarRating { get; set; }
        public int RouteSequence { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null; 
    }
}