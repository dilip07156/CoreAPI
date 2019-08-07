using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FollowUpGetRes
    {
        public List<FollowUp> FollowUp { get; set; } = new List<FollowUp>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
