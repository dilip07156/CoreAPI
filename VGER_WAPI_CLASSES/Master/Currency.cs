using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class Currency
    {
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public Double? CurrencyExchangeRate { get; set; }
        public string SubUnit { get; set; }
    }
}
