using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class mQRFPositionFOC
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        //Unique Id for mPositionFOC
        public string PositionFOCId { get; set; }

        /// <summary>
        /// Take all following properties from mQuote and mPosition collections
        /// </summary>
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public long DepartureId { get; set; }
        public DateTime? Period { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ContractPeriod { get; set; }
        public long PaxSlabId { get; set; }
        public string PaxSlab { get; set; }
        public string Type { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string RoomId { get; set; }
        public bool IsSupplement { get; set; }
        public string SupplierId { get; set; }
        public string Supplier { get; set; }
        public string ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }
        public string ProductRangeId { get; set; }
        public string ProductRange { get; set; }

        //Take ContractId from mPositionPrice collection
        public string ContractId { get; set; }

        //min pax slab
        public int Quantity { get; set; }
        //Take FOCQty from mProductFreePlacePolicy collection
        public int FOCQty { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
