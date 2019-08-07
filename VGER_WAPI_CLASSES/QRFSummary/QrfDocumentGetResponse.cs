using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.QRFSummary
{
   public class QrfDocumentGetResponse
    {
        public QrfDocumentGetResponse()
        {
            ResponseStatus = new ResponseStatus();
            QrfDocuments = new List<QrfDocument>();

        }
        public ResponseStatus ResponseStatus { get; set; }
        public List<QrfDocument> QrfDocuments { get; set; }
    }
}
