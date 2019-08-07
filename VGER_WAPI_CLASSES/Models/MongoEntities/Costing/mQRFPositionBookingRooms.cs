using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace VGER_WAPI_CLASSES
{
    public class mQRFPositionBookingRooms
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string PositionBookingRoomsId { get; set; }
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public string ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }
        public string ProductRangeId { get; set; }
        public string ProductRange { get; set; }
        public string ProductRangeCode { get; set; }
        public string RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public long Age { get; set; }
        public long Count { get; set; }
        public string ChargeBasis { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
