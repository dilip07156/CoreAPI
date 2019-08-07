using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PaxSetRequest
    {
        public PaxSlabDetails PaxSlabDetails { get; set; }
        public string QRFID { get; set; }
        public string UserEmail { get; set; }
        public string VoyagerUserId { get; set; }

        public PaxSetRequest()
        {
            PaxSlabDetails = new PaxSlabDetails();
        }
    }
}