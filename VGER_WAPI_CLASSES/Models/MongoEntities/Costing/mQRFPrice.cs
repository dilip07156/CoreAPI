using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class mQRFPrice
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFPrice_Id { get; set; } // Primary Identifier
        public string QRFID { get; set; } // Identifier for QRF
        public string SystemCompany_Id { get; set; }
        public int VersionId { get; set; } //Counter : latest of this QRF + 1
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool IsCurrentVersion { get; set; }
        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public string QRFCurrency_Id { get; set; } // Selling Currency for this version of QRF Costing (Editable from Costing Page)
        public string QRFCurrency { get; set; }
        public double PercentSoldSupplement { get; set; } = 100; //Set to 100 as default
        public double PercentSoldOptional { get; set; } = 100; //Set to 100 as default

        public string SalesOfficer { get; set; }
        public string CostingOfficer { get; set; }
        public string ProductAccountant { get; set; }

        public string ValidForTravel { get; set; }
        public string ValidForAcceptance { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? FollowUpCostingOfficer { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? FollowUpWithClient { get; set; }

        public AgentInfo AgentInfo { get; set; } = new AgentInfo();
        public QRFAgentProductInfo AgentProductInfo { get; set; } = new QRFAgentProductInfo();
        public List<QRFAgentPassengerInfo> AgentPassengerInfo { get; set; } = new List<QRFAgentPassengerInfo>();
        public List<QRFAgentRoom> QRFAgentRoom { get; set; } = new List<QRFAgentRoom>();

        public List<RoutingInfo> RoutingInfo { get; set; } = new List<RoutingInfo>();
        [BsonIgnoreIfNull(true)]
        public List<RoutingDays> RoutingDays { get; set; } = new List<RoutingDays>();
        public List<QRFDepartureDates> Departures { get; set; } = new List<QRFDepartureDates>();
        public QRFPaxSlabDetails PaxSlabDetails { get; set; } = new QRFPaxSlabDetails();
        public List<QRFFOCDetails> QRFSalesFOC { get; set; } = new List<QRFFOCDetails>();
        public QRFMargins QRFMargin { get; set; } = new QRFMargins();

        public List<FollowUp> FollowUp { get; set; } = new List<FollowUp>();

        public ExchangeRateSnapshot ExchangeRateSnapshot { get; set; } = new ExchangeRateSnapshot();
        public List<QRFExchangeRates> QRFExchangeRates { get; set; } = new List<QRFExchangeRates>();
        public mGuesstimate Guesstimate { get; set; } = new mGuesstimate();

        [BsonIgnoreIfNull(true)]
        public List<EmailDetails> EmailDetails { get; set; } = new List<EmailDetails>();

        [BsonIgnoreIfNull(true)]
        public List<TourEntities> TourEntities { get; set; } = new List<TourEntities>();

        //this field is taken for Storing Meal Info like If QRFID has Lunch,dinner etc details then it will save MealType & its PositionID.In case of Included In Hotel positionId is blank
        [BsonIgnoreIfNull(true)]
        public Meals Meals { get; set; } = new Meals();

        [BsonIgnoreIfNull(true)]
        public string DefaultMealPlan { get; set; }

        public bool RegenerateItinerary { get; set; }

        public CommsPurchasing Commercial { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<QuoteMappings> Mappings { get; set; } = new List<QuoteMappings>();
    }

    public class EmailDetails
    {
        public string EmailDetailsId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public List<string> BCC { get; set; }

        public List<string> CC { get; set; }

        public string Subject { get; set; }

        public string MailType { get; set; }

        public string MailStatus { get; set; }

        public string EmailHtml { get; set; }

        public string Remarks { get; set; }
        
        public List<string> PDFPath { get; set; }

        public DateTime MailSentOn { get; set; }

        public string MailSentBy { get; set; }

        public string MailSent { get; set; }

        public string EditUser { get; set; } = "";

        public DateTime? EditDate { get; set; } = null;

        public bool IsDeleted { get; set; }
    }
}
