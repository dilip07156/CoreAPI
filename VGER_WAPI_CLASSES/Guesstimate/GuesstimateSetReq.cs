using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateSetReq
    {
        public bool IsNewVersion { get; set; } = false;
        public long DepartureId { get; set; }
        public long PaxSlabId { get; set; }
        public mGuesstimate Guesstimate { get; set; } = new mGuesstimate();
        public string Qrfid { get; set; }
        public string VoyagerUserId { get; set; }
    }
}
