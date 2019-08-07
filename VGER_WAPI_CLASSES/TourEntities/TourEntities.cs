using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{ 
    public class TourEntities
    {
        public string TourEntityID { get; set; }
        public string Type { get; set; }
        public int PaxSlabID { get; set; }
        public string PaxSlab { get; set; }
        public bool IsLunch { get; set; }
        public bool IsDinner { get; set; }
        public string HowMany { get; set; }
        public string RoomType { get; set; }
        public string RoomTypeID { get; set; }
        public string PositionID { get; set; }
        public string PersonType { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Flag { get; set; }
        public bool IsDeleted { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class DynamicTourEntity
    {
        public string PositionID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public bool IsOther { get; set; }
        public string ProductType { get; set; }
        public string ProductTypeID { get; set; }
        public string CityName { get; set; }
        public string StartDay { get; set; }
        public string Duration { get; set; }
        public bool Status { get; set; }
        public string CreateUser { get; set; }
        public string EditUser { get; set; }
        public bool IsTourEntity { get; set; }
    }
}

