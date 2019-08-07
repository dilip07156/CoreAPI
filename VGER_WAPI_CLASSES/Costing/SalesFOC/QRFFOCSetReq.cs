using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFFOCSetReq
    {
        public string QRFID { get; set; }
        public bool StandardFOC { get; set; }
        public List<QRFFOCDetails> FOCDetails { get; set; } = new List<QRFFOCDetails>();
    }
}
