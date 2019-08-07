using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFDateRange
    {
        public List<QRFDepartureDates> Departures { get; set; } = new List<QRFDepartureDates>();
        public string QRFID { get; set; }
    }

    public class QRFDepartureDates
    {
        public long Departure_Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Date { get; set; }
        public int NoOfDep { get; set; }
        public int PaxPerDep { get; set; }
        public string Warning { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreateUser { get; set; } = "";
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }
}
