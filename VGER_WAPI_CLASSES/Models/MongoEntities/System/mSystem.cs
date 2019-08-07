using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mSystem
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerSystem_Id { get; set; }
        public string SystemName { get; set; }
        public string SystemEmail { get; set; }
        public string GroupDefaultMarkup_Id { get; set; }
        public string FitdefaultMarkup_Id { get; set; }
        public double? GroupDefaultMarkUp { get; set; }
        public double? GroupMinGrossMargin { get; set; }
        public string CoreCompany_Id { get; set; }
        public string EmergencyPhoneFit { get; set; }
        public string EmergencyPhoneGroups { get; set; }
        public int? SingleAdults { get; set; }
        public int? TwinAdults { get; set; }
        public int? DoubleAdults { get; set; }
        public int? TripleAdults { get; set; }
        public int? QuadAdults { get; set; }
        public string FitsenderEmail { get; set; }
        public string GroupSenderEmail { get; set; }
        public string BaseCurrency_Id { get; set; }
        public int DefaultSupplierTerm { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<SystemMapping> Mappings { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; } 
    }

    public class SystemMapping
    {
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
