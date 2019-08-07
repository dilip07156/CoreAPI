using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentSetRes
    {
        public AgentSetRes()
        {
            ResponseStatus = new ResponseStatus();           
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string UserName { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public string CompanyCode { get; set; }
        public string EmergencyContactId { get; set; }
        public string PaymentTermsId { get; set; }
        public string TermsAndConditionsId { get; set; }
        public string PaymentDetailsId { get; set; }
        public string TaxRegId { get; set; }
        public Targets Targets { get; set; }
    }
}
