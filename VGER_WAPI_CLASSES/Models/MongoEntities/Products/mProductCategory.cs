using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductCategory
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductCategory_Id { get; set; }
        public string DefProductCategory_Id { get; set; }
        public string ProductCategoryName { get; set; }
        public string ParentCategory_Id { get; set; }
        public string Product_Id { get; set; }
        public bool Default { get; set; }
        public bool IsRooms { get; set; }
        public string Status { get; set; }
        public string GroupBreakfast { get; set; }
        public string GroupBreakfastMenu { get; set; }
        public string FitBreakfast { get; set; }
        public string FitBreakfastMenu { get; set; }
        public string Basis { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string GridInfo { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
