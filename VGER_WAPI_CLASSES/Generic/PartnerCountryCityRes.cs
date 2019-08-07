using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PartnerCountryCityRes
    {
        public PartnerCountryCityRes()
        {
            ResortInfo = new mResort();
            ResponseStatus = new ResponseStatus();
        }
        public mResort ResortInfo { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
