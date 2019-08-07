using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VGER_WAPI_CLASSES
{
    public class mMealType
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string def_MealType_Id { get; set; }
        public string MealType { get; set; }
        public string MealType_Name { get; set; }
        public string Status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Create_date { get; set; }
        public string Create_User { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Edit_date { get; set; }
        public string Edit_User { get; set; }
        public bool? ForLunch { get; set; }
        public bool? ForDinner { get; set; }
        public bool? ForBreakfast { get; set; }
    }
}
