using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class CurrencyResponse
    {

        public List<Currency> CurrencyList { get; set; }

        public CurrencyResponse()
        {
            CurrencyList = new List<Currency>();
        }

    }
}
