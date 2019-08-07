using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mItinerary
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFID { get; set; }
        public string ItineraryID { get; set; }
        public int Version { get; set; }      
        public List<ItineraryDaysInfo> ItineraryDays { get; set; } = new List<ItineraryDaysInfo>();

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;

    }

    public class ItineraryDaysInfo
    {
        public string ItineraryDaysId { get; set; }
        public string Day { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Date { get; set; }
        public string DayOfWeek { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ItineraryName { get; set; }
        public string ToCityName { get; set; } = "";
        public string ToCountryName { get; set; } = "";
        public string RoutingMatrix { get; set; }
        public List<Meal> Meal { get; set; } = new List<Meal>();
        public List<Hotel> Hotel { get; set; } = new List<Hotel>();
        public List<ItineraryDescriptionInfo> ItineraryDescription { get; set; } = new List<ItineraryDescriptionInfo>();
        public string Desc { get; set; }
    }

    public class ItineraryDescriptionInfo
    {
        public string City { get; set; }
        public string PositionId { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string Type { get; set; } //If coming from position, set to Service/If coming from Extra button, set to Extra
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int? NumberOfPax { get; set; }
        public string KeepAs { get; set; } = "";
        public string ProductDescription { get; set; }
        public int? Duration { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; }
		public string ItineraryRemarks { get; set; }
		public string Supplier { get; set; }
        public string Allocation { get; set; }
        public List<RoomInfo> RoomDetails { get; set; } = new List<RoomInfo>();

        public string CreateUser { get; set; } = "";

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public bool IsRoutingMatrix { get; set; } = false;
    }

    public class Meal
    {
        public string MealType { get; set; }
        public string MealTime { get; set; }
        public string PositionId { get; set; }
        public string ProductID { get; set; }
        //public string ProdDesc { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public string Telephone { get; set; }
        //public string Lat { get; set; }
        //public string Long { get; set; }
        public string Mail { get; set; }
        public bool IsDeleted { get; set; } = false;
        //public List<ArrProductFacilities> ProdFacilities { get; set; }
        //public List<ArrProductResources> ProdResources { get; set; }
    }

    public class Hotel
    {
        public string PositionId { get; set; }
        public string HotelId { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public string ProdCategory { get; set; }
        public string Location { get; set; }
        public int Stars { get; set; }
        public int Duration { get; set; }
        public string ProdDesc { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public string Telephone { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public string HotelMapURL { get; set; }
        public string Mail { get; set; }
        public string TotalNumberOfRooms { get; set; }
        public bool? Placeholder { get; set; }
        public bool IsDeleted { get; set; }
        public List<ArrProductFacilities> ProdFacilities { get; set; }
        public List<ArrProductResources> ProdResources { get; set; }
        public List<AlternateServices> AlternateHotels { get; set; } = new List<AlternateServices>();
        public AlternateServiesParameter AlternateHotelsParameter { get; set; } = new AlternateServiesParameter();
    }
}
