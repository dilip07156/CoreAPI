using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFExchangeRates
    {
        public string QRFExchangeRatesID { get; set; }
        public string QRFID { get; set; }

        public string ExchamgeRateDetailID { get; set; }
        public DateTime AsOnDate { get; set; }

        public string FromCurrencyId { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrencyId { get; set; }
        public string ToCurrency { get; set; }
        public double ExchangeRate { get; set; }
        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
