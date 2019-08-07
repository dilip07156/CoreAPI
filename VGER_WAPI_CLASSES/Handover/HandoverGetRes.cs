using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class HandoverGetRes
    {
        public HandoverGetRes()
        {
            Depatures = new List<Depatures>();
            ResponseStatus = new ResponseStatus();
        }

        public List<Depatures> Depatures { get; set; }
        public string QRFID { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public bool IsShowProcessing { get; set; }
    }
}