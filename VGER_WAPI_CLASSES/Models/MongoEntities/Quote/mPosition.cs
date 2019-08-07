using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mPosition
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public int PositionSequence { get; set; }
        public string ProductType { get; set; }
        public string ProductTypeId { get; set; }
        public int DayNo { get; set; }
        public string RoutingDaysID { get; set; }

        [BsonIgnoreIfNull(true)]
        public string StartingFrom { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ProductAttributeType { get; set; }

        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string CityID { get; set; }

        public string ProductName { get; set; }
        public string ProductID { get; set; }

        public string BudgetCategoryId { get; set; }
        public string BudgetCategory { get; set; }

        public string SupplierId { get; set; }
        public string SupplierName { get; set; }

        public string StartTime { get; set; } = "18:30";
        public string EndTime { get; set; } = "09:00";
        public int Duration { get; set; } = 1;

        public string KeepAs { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; }
        public string Status { get; set; }

        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        //Accomodation
        [BsonIgnoreIfNull(true)]
        public string StarRating { get; set; }

        [BsonIgnoreIfNull(true)]
        public string Location { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ChainName { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ChainID { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MealPlan { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EarlyCheckInDate { get; set; }

        [BsonIgnoreIfNull(true)]
        public string EarlyCheckInTime { get; set; }

        [BsonIgnoreIfNull(true)]
        public int? InterConnectingRooms { get; set; }

        [BsonIgnoreIfNull(true)]
        public int? WashChangeRooms { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? LateCheckOutDate { get; set; }

        [BsonIgnoreIfNull(true)]
        public string LateCheckOutTime { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<RoomDetailsInfo> RoomDetailsInfo { get; set; } = new List<RoomDetailsInfo>();

        [BsonIgnoreIfNull(true)]
        public string DeletedFrom { get; set; }

        //Activities
        [BsonIgnoreIfNull(true)]
        public string TypeOfExcursion { get; set; }

        [BsonIgnoreIfNull(true)]
        public string TypeOfExcursion_Id { get; set; }

        [BsonIgnoreIfNull(true)]
        public int? NoOfPaxAdult { get; set; }

        [BsonIgnoreIfNull(true)]
        public int? NoOfPaxChild { get; set; }

        [BsonIgnoreIfNull(true)]
        public int? NoOfPaxInfant { get; set; }
        //Activities 

        //Meals start here 
        [BsonIgnoreIfNull(true)]
        public string MealType { get; set; }

        [BsonIgnoreIfNull(true)]
        public string TransferDetails { get; set; }

        [BsonIgnoreIfNull(true)]
        public bool ApplyAcrossDays { get; set; }
        //Meals End here

        //Bus & Transfer start here 
        [BsonIgnoreIfNull(true)]
        public string FromPickUpLoc { get; set; }

        [BsonIgnoreIfNull(true)]
        public string FromPickUpLocID { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ToCityName { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ToCityID { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ToCountryName { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ToDropOffLoc { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ToDropOffLocID { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ForPositionId { get; set; }

        [BsonIgnoreIfNull(true)]
        public bool IsCityPermit { get; set; }

        [BsonIgnoreIfNull(true)]
        public bool IsParkingCharges { get; set; }

        [BsonIgnoreIfNull(true)]
        public bool IsRoadTolls { get; set; }
        //Bus & Transfer End here         

        public string BuyCurrencyId { get; set; }
        public string BuyCurrency { get; set; }

        public bool? StandardPrice { get; set; }
        public bool? StandardFOC { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<AlternateServices> AlternateHotels { get; set; } = new List<AlternateServices>();
        [BsonIgnoreIfNull(true)]
        public AlternateServiesParameter AlternateHotelsParameter { get; set; } = new AlternateServiesParameter();

        //if IsTourEntity = true then it will check the Position is created from TourEntity else not
        public bool IsTourEntity { get; set; }
    }

    public class RoomDetailsInfo
    {
        public string RoomId { get; set; }
        public int RoomSequence { get; set; }

        public string ProductCategoryId { get; set; }
        public string ProductRangeId { get; set; }
        public string ProductCategory { get; set; }
        public string ProductRange { get; set; }
        public string CrossPositionId { get; set; }
        public bool IsSupplement { get; set; }

        [BsonIgnoreIfNull(true)]
        public string ProdDesc { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }

    public class AlternateServiesParameter
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Location { get; set; }
        public string StarRating { get; set; }
        public string BdgCategary { get; set; }
    }
}