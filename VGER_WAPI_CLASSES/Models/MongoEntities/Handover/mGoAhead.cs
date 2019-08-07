using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mGoAhead
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFID { get; set; }

        public string GoAheadId { get; set; } = "";

        public string QRFPriceId { get; set; } = "";

        public int VersionId { get; set; }

        public string Remarks { get; set; } = "";

        public bool IsTemplate { get; set; }

        public bool? IsTemplateCreated { get; set; }

        public string TemplateBookingNumber { get; set; }

        public bool IsQRF { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ConfirmationDeadline { get; set; }

        public string OperationUserName { get; set; } = "";

        public string OperationUserID { get; set; } = "";

        public List<Depatures> Depatures { get; set; } = new List<Depatures>();

        public string CreateUser { get; set; } = "";

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string EditUser { get; set; } = "";

        public DateTime? EditDate { get; set; } = null;

        public bool IsDeleted { get; set; } = false; 
    }

    public class Depatures
    {
        public long DepatureId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DepatureDate { get; set; }
        public bool IsCreate { get; set; }
        public bool IsMaterialised { get; set; }
        public List<ChildInfo> ChildInfo { get; set; } = new List<ChildInfo>();
        public List<PassengerRoomInfo> PassengerRoomInfo { get; set; } = new List<PassengerRoomInfo>();
        public bool IsDeleted { get; set; } = false;


        public bool ConfirmStatus { get; set; } = false;
        public string ConfirmMessage { get; set; }
        public string OpsBookingNumber { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Confirmed_Date { get; set; } = DateTime.Now;
        public string Confirmed_User { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CreateUser { get; set; }
        [BsonIgnoreIfNull(true)]
        public DateTime CreateDate { get; set; }

    }

    public class ChildInfo
    {
        public string ChildInfoId { get; set; }
        public string Type { get; set; }
        public int Number { get; set; }
        public int Age { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; } = null;
    }

    public class PassengerRoomInfo
    {
        public string RoomTypeID { get; set; }
        public string RoomTypeName { get; set; }
        public int? RoomCount { get; set; }
        public int? PaxCount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; } = null;
    }
}
