using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegrationBookingReq
    {
        public IntegrationBookingReq()
        {
            ProductLineItemList = new List<ProductLineItemBooking>();
            ProductLineItemCityCountryList = new List<ProductLineItemCityCountryBooking>();
            QuotationQRFPriceInfo = new mQRFPrice();
            CredentialInfo = new PartnerLoginRequest();
            BookingInfo = new Bookings();
        }

        public PartnerLoginRequest CredentialInfo { get; set; }
        public Bookings BookingInfo { get; set; }
        public mQRFPrice QuotationQRFPriceInfo { get; set; }

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
        public string BookingType { get; set; }//Offline i.e, 120840001
        public string ClientType { get; set; }// Default it's B2B

        public List<ProductLineItemBooking> ProductLineItemList { get; set; }

        public List<ProductLineItemCityCountryBooking> ProductLineItemCityCountryList { get; set; }
    }

    public class ProductLineItemBooking
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

    public class ProductLineItemCityCountryBooking
    {
        public string Country { get; set; }
        public string City { get; set; }
    }
}
