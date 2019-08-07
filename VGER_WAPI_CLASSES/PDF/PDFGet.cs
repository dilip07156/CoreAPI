using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class PDFGetReq
    {   
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string MailPath { get; set; }
        public string Html { get; set; }
        public bool IsSendEmail { get; set; } = true;
        public string UserEmail { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Module { get; set; }
        public string MailStatus { get; set; }

        public string BookingNo { get; set; }
        public string QRFID { get; set; }
        public string SystemCompany_Id { get; set; }
        public List<string> PositionIds { get; set; } 
    }

    public class PDFGetRes
    {
        public PDFGetRes()
        {
            ResponseStatusMessage = new ResponseStatusMessage();
            PDFTemplateGetRes = new List<PDFTemplateGetRes>();
        }
        public string BookingNo { get; set; }
        public ResponseStatusMessage ResponseStatusMessage { get; set; }
        public List<PDFTemplateGetRes> PDFTemplateGetRes { get; set; }
    }

    public class PDFTemplateGetRes
    {
        public PDFTemplateGetRes()
        {
            ResponseStatusMessage = new ResponseStatusMessage();
        }

        public ResponseStatusMessage ResponseStatusMessage { get; set; }  
        public DocumentDetails DocumentDetails { get; set; }
    
        public string To { get; set; }
        public string From { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Importance { get; set; } 
        public string SendVia { get; set; }
        public string UserEmail { get; set; }
        public string PathType { get; set; }

        public string Document_Id { get; set; }
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public string SupplierId { get; set; } 
        public string AlternateServiceId { get; set; } 
        public string Client { get; set; }
        public string QRFPriceId { get; set; }
        public string SystemCompany_Id { get; set; }
    }

    public class PDFGenerateGetRes
    {
        public PDFGenerateGetRes()
        {
            ResponseStatusMessage = new ResponseStatusMessage();
        }
        public string PDFPath { get; set; }
        public string FullPDFPath { get; set; }
        public ResponseStatusMessage ResponseStatusMessage { get; set; }
    }

    public class MailGenerateRes
    {
        public MailGenerateRes()
        {
            ResponseStatusMessage = new ResponseStatusMessage();
        }
        public string DocPath { get; set; }
        public string Body { get; set; }
        public ResponseStatusMessage ResponseStatusMessage { get; set; }
    }
}