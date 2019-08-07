using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.QRFSummary
{
   public  class QrfDocumentPostResponse
    {
        public QrfDocumentPostResponse()
        {
            Responsestatus = new ResponseStatus();

        }
        public ResponseStatus Responsestatus { get; set; }

        public string QrfId { get; set; }
        public string DocumentId { get; set; }
    }
}
