using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mMarkups
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();

        public string Markups_Id { get; set; }
        public string Markup { get; set; }
        public string Description { get; set; }
        public string ParentMarkUp_Id { get; set; }
        public bool? VatFree { get; set; }
        public bool IsDeleted { get; set; }
        public string Company_Id { get; set; }

        public List<MarkupDetails> MarkupDetails { get; set; }
    }

    public class MarkupDetails
    {
        public string MarkUpDetail_Id { get; set; }
        public string Markup_Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }
        public string ProductType_Id { get; set; }
        public string ProductType { get; set; }
        public string Resort_Id { get; set; }
        public string PercMarkUp { get; set; }
        public string FixedMarkUp { get; set; }
        public string Currency { get; set; }
        public string Product_Id { get; set; }
        public string Company_Id { get; set; }
        public string PARENTRESORT_ID { get; set; }
        public string REGION_ID { get; set; }
        public string MARKUPTYPE { get; set; }
        public string CURRENCY_ID { get; set; }
        public string REGION { get; set; }
    }
}
