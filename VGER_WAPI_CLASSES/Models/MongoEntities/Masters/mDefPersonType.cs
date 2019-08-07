using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mDefPersonType
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string defPersonType_Id { get; set; }
        public string AlternateType_Id { get; set; }
        public int? PERSTYPEID { get; set; }
        public int? ORDERNR { get; set; }
        public string PersonType { get; set; }
        public string PersonType_Name { get; set; }
        public string Alaernate_PersonType { get; set; }
        public string Alaernate_PersonType_Name { get; set; }
        public bool? PRICEAGE { get; set; }
        public int? ALTPTYPEID { get; set; }
        public bool? FULLPAYING { get; set; }
        public bool? CHILD { get; set; }
        public bool? INFANT { get; set; }
        public DateTime? CREA_DT { get; set; }
        public string CREA_TI { get; set; }
        public string CREA_US { get; set; }
        public DateTime? MODI_DT { get; set; }
        public string MODI_TI { get; set; }
        public string MODI_US { get; set; }
        public string STATUS { get; set; }
        public DateTime? STATUS_DT { get; set; }
        public string STATUS_TI { get; set; }
        public string STATUS_US { get; set; }
    }
}
