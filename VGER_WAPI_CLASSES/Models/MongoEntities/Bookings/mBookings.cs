using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mBookings
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string BookingId { get; set; }
        [BsonIgnoreIfNull]
        public string BookingNumber { get; set; } // BOOKING
        [BsonIgnoreIfNull]
        public string CUSTREF { get; set; } //CUSTREF
        [BsonIgnoreIfNull]
        public string AgentId { get; set; } // Partner_Id
        [BsonIgnoreIfNull]
        public string AgentCode { get; set; }
        [BsonIgnoreIfNull]
        public string AgentName { get; set; }
        [BsonIgnoreIfNull]
        public string AgentContactId { get; set; } // Contact_Id
        [BsonIgnoreIfNull]
        public string AgentContactName { get; set; } // CUSTCONT
        [BsonIgnoreIfNull]
        public string AgentContactEmail { get; set; } // SENDADDR
        [BsonIgnoreIfNull]
        public string CurrencyId { get; set; } // Currency_Id
        [BsonIgnoreIfNull]
        public string Currency { get; set; }
        [BsonIgnoreIfNull]
        public string ExchangeRateId { get; set; }
        [BsonIgnoreIfNull]
        public string ExchangeRateDetailId { get; set; }
        [BsonIgnoreIfNull]
        public string ExchangeRate { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OPTIONDATE { get; set; } // OPTIONDATE
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DEPARTUREDATE { get; set; } // STARTDATE
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ARRIVALDATE { get; set; } // ENDDATE
        [BsonIgnoreIfNull]
        public long Duration { get; set; } //TotalNights
        [BsonIgnoreIfNull]
        public string Status { get; set; } //Status
        [BsonIgnoreIfNull]
        public string StatusDesc { get; set; } //Status Full Name
        [BsonIgnoreIfNull]
        public string NationalityID { get; set; } //Nationality_ID
        [BsonIgnoreIfNull]
        public string Nationality { get; set; }
        [BsonIgnoreIfNull]
        public string TravelFor { get; set; } //TravelReason
        [BsonIgnoreIfNull]
        public string LangugaeID { get; set; } //GroupLeanguage_ID
        [BsonIgnoreIfNull]
        public string Language { get; set; } //GroupLeanguage
        [BsonIgnoreIfNull]
        public string BudgetCategoryID { get; set; } //Category_ID
        [BsonIgnoreIfNull]
        public string BudgetCategory { get; set; } //Category
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreaDT { get; set; }
        [BsonIgnoreIfNull]
        public string CreaUS { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODIDT { get; set; }
        [BsonIgnoreIfNull]
        public string MODIUS { get; set; }
        
        public bool ISEHSupp { get; set; } = false;

        [BsonIgnoreIfNull]
        public string Operator_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Operator { get; set; }
        [BsonIgnoreIfNull]
        public string OperatorEmail { get; set; }
        [BsonIgnoreIfNull]
        public string TourLeader { get; set; }
        [BsonIgnoreIfNull]
        public string TourLeader_Contact { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BookingGoAheadDT { get; set; }
        [BsonIgnoreIfNull]
        public string PRIORITY { get; set; }
        [BsonIgnoreIfNull]
        public string PRIORITYDesc { get; set; }
        [BsonIgnoreIfNull]
        public string Suppliers { get; set; }
    }
}
