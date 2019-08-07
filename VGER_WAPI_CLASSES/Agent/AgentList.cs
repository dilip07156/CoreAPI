using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentList
    {
        public string CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
        public string CityId { get; set; }
        public string Status { get; set; }
        public bool? IsSupplier { get; set; } 
    }
}
