using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateGetRes
    {
        public int LastVersionId { get; set; }
        public mGuesstimate Guesstimate { get; set; }
        public bool IsStandardPrice { get; set; }

        public ResponseStatus ResponseStatus { get; set; }

        public GuesstimateGetRes()
        {
            Guesstimate = new mGuesstimate();
            ResponseStatus = new ResponseStatus();
        }
    }
}
