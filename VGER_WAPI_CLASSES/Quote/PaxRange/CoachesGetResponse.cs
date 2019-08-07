using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CoachesGetResponse
    {
        public List<string> CategoryName { get; set; }
        public List<string> CoachType { get; set; }
        public List<string> BrandName { get; set; }
        public string Status { get; set; }

        public CoachesGetResponse()
        {
            CategoryName = new List<string>();
            CoachType = new List<string>();
            BrandName = new List<string>();
        }
    }
}
