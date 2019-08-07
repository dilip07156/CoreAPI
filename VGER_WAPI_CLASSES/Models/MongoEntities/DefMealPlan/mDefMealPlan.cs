using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mDefMealPlan
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string MealPlan_Id { get; set; }
        public string MealPlan { get; set; }
        public string Description { get; set; }
        public DateTime CREA_DT { get; set; }
        public string CREA_US { get; set; }
        public DateTime MODI_DT { get; set; }
        public string MODI_US { get; set; }
        public string Status { get; set; }
    }
}
