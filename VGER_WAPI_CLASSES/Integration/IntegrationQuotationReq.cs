using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegrationQuotationReq
    {
        public IntegrationQuotationReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            QuotationInfo = new mQuote();
            QuotationQRFPriceInfo = new mQRFPrice();
            ProductLineItemList = new List<ProductLineItemQuotation>();
        }

        public PartnerLoginRequest CredentialInfo { get; set; }
        public mQuote QuotationInfo { get; set; }
        public mQRFPrice QuotationQRFPriceInfo { get; set; }
        public List<ProductLineItemQuotation> ProductLineItemList { get; set; }
        public string CRM_OpportunityId { get; set; }
        public string PartnerEntityCode { get; set; }
        public string SystemOpportunityType { get; set; }
        public string GroupOfCompanies { get; set; }
        public string GroupCompany { get; set; }
        public string CompanyName { get; set; }
        public string BookingEngine { get; set; }
        public string BU { get; set; }
        public string CustomerId { get; set; }
        public string ContactId { get; set; }

        public string StateCode { get; set; }
        public string Opportunitystage { get; set; }
        public string StatusCode { get; set; }

        public string POS { get; set; }//SO
        public string SBU { get; set; }//OBT
        public string ProductType { get; set; }//Holiday
        public string TypeOfProduct { get; set; }//Holiday
        public string HolidayType { get; set; }//Customized
        public string OwnerId { get; set; }//UserName
        public string SourceOfEnquiry { get; set; }//Online Default value
        public string CompanyMarket { get; set; }//DUBAI for time being
        public string BookingSource { get; set; }//Offline i.e, 120840001
        public string QuotationType { get; set; }//Offline i.e, 120840001
        public string ClientType { get; set; }// Default it's B2B
    }

    public class ProductLineItemQuotation
    {
        public string RoomTypeName { get; set; }
        public string NoOfDays { get; set; }
        public string NoofNights { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NoOfAdults { get; set; }
        public int NoOfChildren { get; set; }
        public int NoOfRooms { get; set; }
        public string ProductLineItemNumber { get; set; }
        public decimal CurrencyConversionRate { get; set; }//Not Clear
        public decimal BillingAmount { get; set; }//Not Clear
        public string ROE { get; set; }
    }

    public static class OccupancyType
    {
        public const int Single = 1;
        public const int Double = 2;
        public const int Quad = 4;
        public const int Triple = 3;
        public const int TSU = 1;
        public const int Twin = 2;
    }

    public enum OccupancyTypeEnum
    {
        Single = 1,
        Double = 2,
        Quad = 4,
        Triple = 3,
        TSU = 1,
        Twin = 1
    }
}
