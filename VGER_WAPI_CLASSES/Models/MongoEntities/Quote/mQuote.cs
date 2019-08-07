using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class mQuote
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFID { get; set; }

        public string Parent_QRFID { get; set; }
        public string LatestChild_QRFID { get; set; }
        public string SystemCompany_Id { get; set; }

        public string CurrentPipeline { get; set; }
        public string CurrentPipelineStep { get; set; }
        public string CurrentPipelineSubStep { get; set; }
        public string Status { get; set; }
        public string QuoteResult { get; set; }
        public string Remarks { get; set; }
        public string SalesPerson { get; set; }
        public string SalesPersonUserName { get; set; }
        public string SalesPersonCompany { get; set; }
        public bool StandardFOC { get; set; }
        public string CostingOfficer { get; set; }

        public AgentInfo AgentInfo { get; set; } = new AgentInfo();
        public AgentProductInfo AgentProductInfo { get; set; } = new AgentProductInfo();
        public List<AgentPassengerInfo> AgentPassengerInfo { get; set; } = new List<AgentPassengerInfo>();
        public List<AgentRoom> AgentRoom { get; set; } = new List<AgentRoom>();
        public AgentPaymentInfo AgentPaymentInfo { get; set; } = new AgentPaymentInfo();

        public List<RoutingInfo> RoutingInfo { get; set; } = new List<RoutingInfo>();
        [BsonIgnoreIfNull(true)]
        public List<RoutingDays> RoutingDays { get; set; } = new List<RoutingDays>();
        public List<DepartureDates> Departures { get; set; } = new List<DepartureDates>();
        public PaxSlabDetails PaxSlabDetails { get; set; } = new PaxSlabDetails();
        public List<FOCDetails> FOCDetails { get; set; } = new List<FOCDetails>();
        public Margins Margins { get; set; } = new Margins();

        public List<FollowUp> FollowUp { get; set; } = new List<FollowUp>();
        [BsonIgnoreIfNull(true)]
        public List<QrfDocument> QrfDocuments { get; set; } = new List<QrfDocument>();

        public ExchangeRateSnapshot ExchangeRateSnapshot { get; set; } = new ExchangeRateSnapshot();

        [BsonIgnoreIfNull(true)]
        public List<TourEntities> TourEntities { get; set; } = new List<TourEntities>();

        //this field is taken for Storing Meal Info like If QRFID has Lunch,dinner etc details then it will save MealType & its PositionID.In case of Included In Hotel positionId is blank
        [BsonIgnoreIfNull(true)]
        public Meals Meals { get; set; }

        [BsonIgnoreIfNull(true)]
        public string DefaultMealPlan { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<QuoteMappings> Mappings { get; set; } = new List<QuoteMappings>();

        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        // [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string ValidForTravel { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string ValidForAcceptance { get; set; }
        public bool RegenerateItinerary { get; set; }
        public bool HandoverWithoutEmail { get; set; }
    }

    public class QuoteMappings
    {
        public string Application_Id { get; set; }
        public string Application { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string PartnerEntityType { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string AdditionalInfoType { get; set; }
        public string AdditionalInfo { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
    public class QrfDocument
    {
        public string Document_Id { get; set; }
        public String FileName { get; set; }
        public String SavedFileName { get; set; }
        public String PhysicalPath { get; set; }
        public String VirtualPath { get; set; }
        public String Status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public String CratedUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
        public String ModifiedUser { get; set; }
    }
}