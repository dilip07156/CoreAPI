using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Generic EmailConfig defined for Sending the Email Struture
    /// </summary>
    public class EmailConfig
    {        
        /// <summary>
        /// From Mail Id
        /// </summary>
        public String FromAddress { get; set; }

        /// <summary>
        /// To Mail Id
        /// </summary>
        public String ToAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public String MailServerAddress { get; set; }
        public String MailServerPort { get; set; }

        public String ToCc { get; set; }
        public String ToBcc{ get; set; }

        public List<string> Attachment { get; set; }
        public string Type { get; set; }

        public string MailStatus { get; set; }

        public string Remarks { get; set; }

        public bool IsSave { get; set; }

        public string QRFID { get; set; }

        public string QRFPriceID { get; set; }

        public string MailSentBy { get; set; }

        public string EmailDetailsId { get; set; }

        public string PathType { get; set; }

        //public String LocalDomain { get; set; }
        //public String UserId { get; set; }
        //public String UserPassword { get; set; }
    }
}
