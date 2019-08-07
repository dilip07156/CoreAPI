using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FOCSetRequest
    {
        public string QRFID { get; set; }
        public bool StandardFOC { get; set; }
        public string VoyagerUserId { get; set; }
        public List<FOCDetails> FOCDetails { get; set; } = new List<FOCDetails>();
    }
}
