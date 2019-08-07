using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QrfFollowUpResponse
    {
        public string QRFID { get; set; }
        public string Status { get; set; }
        public List<FollowUpItem> FollowUpItems { get;set; }

        public QrfFollowUpResponse()
        {
            FollowUpItems = new List<FollowUpItem>();
        }
    }
}
