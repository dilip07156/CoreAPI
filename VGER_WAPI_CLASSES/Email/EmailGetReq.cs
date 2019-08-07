using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class EmailGetReq
    {
		//For Booking, REQUIRED FIELDS: BookingNo,PositionId,SupplierId,DocumentType,PlacerUserId,UserEmail
		public string BookingNo { get; set; }         
        public string PositionId { get; set; }
		public string SupplierId { get; set; }
		public string DocumentType { get; set; }
        public string AlternateServiceId { get; set; }
        public string QrfId { get; set; }
        public string PlacerUserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public bool IsSendEmail { get; set; } = true;
		public string GoAheadId { get; set; }
		public string WebURLInitial { get; set; }
		public string DepartureId { get; set; }
        public string QRFPriceId { get; set; }
        public string EnquiryPipeline { get; set; }
        public string CurrentPipelineStep { get; set; }        
        public string Remarks { get; set; }
        public bool IsApproveQuote { get; set; }
        public string EmailHtml { get; set; }
        public string MailStatus { get; set; }
        public string SystemCompany_Id { get; set; }
		public bool IsUI { get; set; }
		public bool IsSaveDocStore { get; set; } = false;
        //below field used when we click on Send Hotel Request then Mail should fire to Positions supplier & its should be added in position->AlternateServices
        public string PosAlternateServiceId { get; set; }
    
        public string FollowUpId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string Subject { get; set; }
        public string Header { get; set; }
        public string Module { get; set; } = "";
        public string Importance { get; set; } = "";

        /// <summary>
        /// selected ToCC 
        /// </summary>
        public string ToCC { get; set; }

        public bool IsLog { get; set; }

        public List<string> PositionIds { get; set; }
        public string Source { get; set; }
    }
}
