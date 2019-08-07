using System;
using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class Commercial
    { 
        public string QRFID { get; set; } // Identifier for QRF
        public string Mode { get; set; }
        public bool IsCurrentVersion { get; set; }
        public string BaseCurrency { get; set; }
        public string InvoiceCurrency { get; set; }
        public List<CommercialDepartures> Departures { get; set; } = new List<CommercialDepartures>();

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now; 
    } 

    public class CommercialDepartures
    {
        public long DepartureId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CommercialSlabs> Slabs { get; set; } = new List<CommercialSlabs>();
    }

    public class CommercialSlabs
    {
        public long PaxSlabId { get; set; }
        public string PaxSlab { get; set; }
        public CommercialReport TotalCalculation { get; set; } = new CommercialReport();
        public List<TotalForProductType> Consolidated { get; set; } = new List<TotalForProductType>();
        public List<CommsPositions> Breakup { get; set; } = new List<CommsPositions>();
    }

    public class TotalForProductType
    {
        public string ProductType { get; set; }
        public CommsProfitability Total { get; set; } = new CommsProfitability();
    }

    public class CommsPositions
    { 
        public string PositionId { get; set; }
        public string ProductType { get; set; }
        public string PositionType { get; set; }
        public string GridInfo { get; set; }
        public CommercialReport Calculation { get; set; } = new CommercialReport();
    }

    public class CommercialReport
    {
        public CommsPurchasing Purchasing { get; set; } = new CommsPurchasing();
        public CommsProformaInvoice ProformaInvoice { get; set; }
        public CommsFinalInvoice FinalInvoice { get; set; }
        public CommsProfitability Profitability { get; set; } = new CommsProfitability();

        public CommercialReport()
        {
            ProformaInvoice = new CommsProformaInvoice();
            FinalInvoice = new CommsFinalInvoice();
        }
    }

    public class CommsPurchasing
    {
        public string Currency { get; set; }
        public double Budget { get; set; }
        public double Actual { get; set; }
    }

    public class CommsProformaInvoice
    {
        public string Currency { get; set; } 
        public DateTime InvoiceDate { get; set; }
        public double Amount { get; set; }
    }

    public class CommsFinalInvoice
    {
        public string Currency { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double Amount { get; set; } 
    }

    public class CommsProfitability
    {
        public string Currency { get; set; }
        public double Purchase { get; set; }
        public double Sell { get; set; }
        public double Margin { get; set; }
        public double ProfitPercentage { get; set; }        
    }
} 