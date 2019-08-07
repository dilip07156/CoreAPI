using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegrationOpportunityReq
    {
        public IntegrationOpportunityReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            OpportunityInfo = new mQuote();
            OpportunityQRFPriceInfo = new mQRFPrice();
        }

        public PartnerLoginRequest CredentialInfo { get; set; }
        public mQuote OpportunityInfo { get; set; }
        public mQRFPrice OpportunityQRFPriceInfo { get; set; }
        public string PartnerEntityCode { get; set; }
        public string SystemOpportunityType { get; set; }
        public string GroupOfCompanies { get; set; }
        public string GroupCompany { get; set; }
        public string CompanyName { get; set; }
        public string BU { get; set; }
        public string CustomerId { get; set; }
        public string ContactId { get; set; }

        public string StateCode { get; set; }
        public string Opportunitystage { get; set; }
        public string StatusCode { get; set; }

        public string POS { get; set; }//SO
        public string SBU { get; set; }//OBT
        public string ProductType { get; set; }//Holiday
        public string OwnerId { get; set; }//UserName
        public string SourceOfEnquiry { get; set; }//Online Default value
        public string CompanyMarket { get; set; }//DUBAI for time being
        public string ClientType { get; set; }//B2B
        public string Salutation { get; set; }//Title (i.e, Mr, Mrs, Ms, The)
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobile { get; set; }

    }
}
