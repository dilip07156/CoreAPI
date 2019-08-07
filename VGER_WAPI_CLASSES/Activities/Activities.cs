using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Activities
    {
        public List<ActivitiesProperties> ActivitiesDetails { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }

    }

    public class ActivitiesProperties
    {
        public long ActivityID { get; set; }
        public string DayID { get; set; }
        public string CityID { get; set; }
        public string CityName { get; set; }
        public string StartTime { get; set; }
        public string TypeOfExcursion { get; set; }
        public string TypeOfExcursion_Id { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string TourType { get; set; }
        public string TourType_Id { get; set; }
        public string TicketType { get; set; }
        public string TicketType_Id { get; set; }
        public int NoOfPaxAdult { get; set; }
        public int NoOfPaxChild { get; set; }
        public int NoOfPaxInfant { get; set; }
        public string KeepAs { get; set; }
        public object Price { get; set; }
        public object FOC { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; } 
        public string SupplementID { get; set; }

        public bool IsDeleted { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }

        public ActivitiesProperties()
        {
            
        }
    }
}
