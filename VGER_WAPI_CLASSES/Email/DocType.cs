using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public static class DocType
	{
        //Email DocType
        public const string BOOKXX = "BOOK-XX";
		public const string BOOKKK = "BOOK-KK";
		public const string BOOKPROV = "BOOK-PROV";
		public const string BOOKOPTXX = "BOOK-OPTXX";
		public const string HOTELNOTAVAILABLE = "HOTEL-NOT-AVAILABLE";
		public const string HOTELAVAILABLE = "HOTEL-AVAILABLE";
		public const string FOLLOWUP = "FOLLOWUP";
		public const string FOLLOWUPEXPENSIVE = "FOLLOWUP-EXPENSIVE";
		public const string BOOKOPTEXT = "BOOK-OPTEXT";
		public const string REMIND = "BOOK-REQ-REMIND";
		public const string TESTEMAIL = "BOOK-REQ-TEST";
		public const string BOOKREQ = "BOOK-REQ";
		public const string GOAHEAD = "GO-AHEAD";
        public const string SALESSUBMITQUOTE = "SALES-SUBMITTOCO";
        public const string COAPPROVAL = "SALES-SUBMITTOPA";
        public const string COREJECT = "SALES-SUBMITTOSO";
        public const string CAPAPPROVAL = "SALES-PAAPPROVE";
        public const string CAPREJECT = "SALES-PAREJECT";
        public const string SENDTOCLIENT = "SALES-QUOTATION";
        public const string MAILAGENTACCEPT = "SALES-AGENTACCEPT";
        public const string ACCEPTWITHOUTPROPOSAL = "SALES-AGENTACCEPT_AWP";
        public const string MAILAGENTREJECT = "SALES-AGENTAMEND";
        public const string PWDRECOVER = "PWD-RECOVER";
        public const string QUOTEFOLLOWUP = "QUOTE-FOLLOWUP";
        public const string ERRORREPORT = "ERROR-REPORT";
        public const string OPSHOTELCONFIRM = "CONFIRMATION";
        public const string OPSPOSAMEND= "BOOK-AMEND";

        //PDF DocType
        public const string OPSVOUCHER = "VOUCHER"; 
        public const string OPSROOMING = "OPS-ROOMING";
		public const string OPSFULLITINERARY = "FULL-ITINERARY";
	} 

    public class DocTypeDetails
    {
        public string DocType { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsSaveDocStore { get; set; }
    }
}
