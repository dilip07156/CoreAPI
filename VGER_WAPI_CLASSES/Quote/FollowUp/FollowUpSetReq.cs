using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FollowUpSetReq
    {
        public List<FollowUp> FollowUp { get; set; }
        public string QRFID { get; set; }

        public FollowUpSetReq()
        {
            FollowUp = new List<FollowUp>();
        }
    }
}
