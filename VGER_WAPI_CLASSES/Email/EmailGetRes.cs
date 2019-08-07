using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class EmailGetRes
    {
        public EmailGetRes()
        {
            ResponseStatus = new ResponseStatus();
            EmailTemplateGetRes = new List<EmailTemplateGetRes>();
        }
        public ResponseStatus ResponseStatus { get; set; }        
        public DateTime EmailSentOn { get; set; }
        public List<EmailTemplateGetRes> EmailTemplateGetRes { get; set; }
    }
}
