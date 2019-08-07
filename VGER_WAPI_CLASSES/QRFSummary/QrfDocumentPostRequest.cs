using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.QRFSummary
{
    public class QrfDocumentPostRequest
    {
        public string Document_Id { get; set; }
        public String FileName { get; set; }
        public String SavedFileName { get; set; }
        public String PhysicalPath { get; set; }
        public String VirtualPath { get; set; }
        public String Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public String CratedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public String ModifiedUser { get; set; }
        public string QrfId { get; set; }
    }
}
