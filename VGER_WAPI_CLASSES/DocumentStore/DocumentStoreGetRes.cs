using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DocumentStoreGetRes
    {
        public DocumentStoreGetRes()
        {
            DocumentStoreList = new List<mDocumentStore>();
            ResponseStatus = new ResponseStatus();
        }
        public List<mDocumentStore> DocumentStoreList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class DocumentStoreInfoGetRes
    {
        public DocumentStoreInfoGetRes()
        { 
            ResponseStatus = new ResponseStatus();
        }

        public string QRFID { get; set; }

        public string AgentTourName { get; set; }

        public string BookingNumber { get; set; }

        public List<DocumentStoreList> DocumentStoreList { get; set; } = new List<DocumentStoreList>();

        public DocumentStoreInfo DocumentStoreInfo { get; set; } = new DocumentStoreInfo();

        public ResponseStatus ResponseStatus { get; set; }
    }

    public class DocumentStoreInfo
    {
        public string Body { get; set; } = "";

        public string From { get; set; } = "";

        public string To { get; set; } = "";

        public DateTime SendDate { get; set; }

        public string Subject { get; set; } = "";

        public string DocumentPath { get; set; } = "";

        public string SendStatus { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    public class DocumentStoreList
    {
        public string DocumentId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string SendStatus { get; set; }  

        public DateTime SendDate { get; set; }
    }
}