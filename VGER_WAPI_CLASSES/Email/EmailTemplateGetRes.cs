using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class EmailTemplateGetRes
    {
        public EmailTemplateGetRes()
        {
            ResponseStatus = new ResponseStatus();
            Attachment = new List<string>();
			DocumentPath = new List<string>();
			EmailGetReq = new EmailGetReq();
		}
        public ResponseStatusMessage ResponseStatusMessage { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
		public string UserEmail { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string PathType { get; set; }
        public string Importance { get; set; }
        public List<string> Attachment { get; set; }
		public string SendVia { get; set; }
		public string DocumentReference { get; set; }
		public List<string> DocumentPath { get; set; }

        public string Document_Id { get; set; }
        public string AlternateServiceId { get; set; }
        public string SupplierId { get; set; }
		public string Client { get; set; }
        public string SendStatus { get; set; }

        public EmailGetReq EmailGetReq { get; set; }
	}
}
