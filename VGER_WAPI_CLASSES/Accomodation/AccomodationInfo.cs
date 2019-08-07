using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AccomodationInfo
    {
        public string AccomodationId { get; set; }
        public int AccomodationSequence { get; set; }
        public string ProductType { get; set; }

        public string CityName { get; set; }
        public string CityID { get; set; }

        public string StartingFrom { get; set; }
        public string NoOfNight { get; set; }
        public string Category { get; set; }
        public string StarRating { get; set; }
        public string Location { get; set; }

        public string ChainName { get; set; }
        public string ChainID { get; set; }

        public string HotelName { get; set; }
        public string HotelID { get; set; }

        public string SupplierId { get; set; }

        public List<RoomDetailsInfo> RoomDetailsInfo { get; set; }

        //Requirements
        public string MealPlan { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EarlyCheckInDate { get; set; }
        public string EarlyCheckInTime { get; set; }
        public int? NumberOfEarlyCheckInRooms { get; set; }

        public int? NumberofInterConnectingRooms { get; set; }
        public int? NumberOfWashChangeRooms { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? LateCheckOutDate { get; set; }
        public string LateCheckOutTime { get; set; }
        public int? NumberOfLateCheckOutRooms { get; set; }

        public string Supplement { get; set; }
        public string SupplementID { get; set; }
        //Requirements End

        public string KeepAs { get; set; }
        public string RemarksForTL { get; set; }
        public string RemarksForOPS { get; set; }

        public string CreateUser { get; set; } = "";
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;

        public AccomodationInfo()
        {
            RoomDetailsInfo = new List<RoomDetailsInfo>();
        }
    }

    public class RoomDetailsInfo
    {
        public string RoomId { get; set; }
        public int RoomSequence { get; set; }

        public int? Rooms { get; set; }
        public string RoomTypeID { get; set; }
        public string RoomType { get; set; }

        public bool IsSupplement { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;

    }
}
