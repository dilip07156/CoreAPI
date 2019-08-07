using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OpportunityPartnerRes
    {
        public OpportunityPartnerRes()
        {
            //OpportunityInfo = new mQuote();
            ResponseStatus = new ResponseStatus();
        }

        /// <summary>
        /// mQuote info is set after record is updated or saved as return.
        /// </summary>
        //public mQuote OpportunityInfo { get; set; }

        /// <summary>
        /// Response Status for Success, Failure Duplicate etc with Message.
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
    }
}
