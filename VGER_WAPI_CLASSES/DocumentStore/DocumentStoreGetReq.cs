using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DocumentStoreGetReq
    {
        public string QRFID { get; set; }
        public string QRFPriceID { get; set; }
        public List<string> DocumentIdList { get; set; }
        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string BookingNumber { get; set; }
        public string Position_Id { get; set; }
        public string AlternateService_Id { get; set; }
        public string Supplier_Id { get; set; }
        public string Client_Id { get; set; }
        public string UserEmailId { get; set; }
        public string SendStatus { get; set; }
        public string ModuleType { get; set; }
    }
}