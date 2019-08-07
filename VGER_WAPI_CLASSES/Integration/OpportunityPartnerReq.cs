using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OpportunityPartnerReq
    {
        public OpportunityPartnerReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            OpportunityInfo = new OpportunityInfo();
        }

        /// <summary>
        /// CredentialInfo is to pass the credential to perform any operation
        /// </summary>
        public PartnerLoginRequest CredentialInfo { get; set; }

        /// <summary>
        /// OpportunityInfo(Booking Opportunity) Info where partner will need to pass there data to the system.
        /// </summary>
        public OpportunityInfo OpportunityInfo { get; set; }
    }

    public class OpportunityInfo
    {
        /// <summary>
        /// OpportunityId is the part of Unique id of partner to identify the Opportunity(Booking Opportunity) in the system (i,e. Voyager).
        /// </summary>
        //[Required]
        //public string OpportunityId { get; set; }

        /// <summary>
        /// OpportunityId is the part of Unique id Opportunity in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string OpportunityId { get; set; }

        /// <summary>
        /// Status is to update or move to reject pipe line against the Opportunity(Booking Opportunity) in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Reason is to update or move to reject pipe line against the Opportunity(Booking Opportunity) in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string Reason { get; set; }
    }
}
