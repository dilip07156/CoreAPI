using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    #region MIS Mapping
    public class MISMappingRes
    {
        public List<MISMappingDetails> MISMappingList { get; set; } = new List<MISMappingDetails>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class MISMappingDetails
    {
        public string ItemName { get; set; }
        public string ItemUrl { get; set; }
        public int ItemSeq { get; set; }
    }
    #endregion

    #region Sales Dashboard
    public class SalesDashboardData
    {
        public string QRFID { get; set; }
        public string Destination { get; set; }
        public string Customer { get; set; }
        public int? AdultPax { get; set; }
        public string QRFStatus { get; set; }
        public double? Budget { get; set; }
        public string SalesOfficer { get; set; }
        public string SalesOffice { get; set; }
        public string SalesOfficeID { get; set; }
        public string BusinessType { get; set; }
        public string BaseCurrency { get; set; }
        public double? SalesValue { get; set; }
        public double? CostValue { get; set; }
        public int NoOfDepartures { get; set; }
        public double? InvoiceValue { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? StatusDate { get; set; }
        public int Age { get; set; }
    }

    public class SalesDashboardSummary
    {
        public int SAPQuotes { get; set; }
        public int SAPPax { get; set; }
        public double SAPValue { get; set; }

        public int CAPQuotes { get; set; }
        public int CAPPax { get; set; }
        public double CAPValue { get; set; }

        public int AAPQuotes { get; set; }
        public int AAPPax { get; set; }
        public double AAPValue { get; set; }

        public int GAPQuotes { get; set; }
        public int GAPPax { get; set; }
        public double GAPValue { get; set; }

        public decimal SAPAge1Week { get; set; }
        public decimal SAPAge2Week { get; set; }
        public decimal SAPAge2PlusWeek { get; set; }

        public decimal CAPAge1Week { get; set; }
        public decimal CAPAge2Week { get; set; }
        public decimal CAPAge2PlusWeek { get; set; }

        public decimal AAPAge1Week { get; set; }
        public decimal AAPAge2Week { get; set; }
        public decimal AAPAge2PlusWeek { get; set; }

        public decimal GAPAge1Week { get; set; }
        public decimal GAPAge2Week { get; set; }
        public decimal GAPAge2PlusWeek { get; set; }

        public string PassengerChartJson { get; set; }
        public string QuoteChartJson { get; set; }
        public string RevenueChartJson { get; set; }
        public string BaseCurrency { get; set; }

        public List<PassengerForecastGrid> PassengerForecastGrid { get; set; } = new List<PassengerForecastGrid>();
        public List<PassengerForecastGraph> PassengerForecastGraph { get; set; } = new List<PassengerForecastGraph>();
        public List<string> FinancialYearMonths { get; set; } = new List<string>();
        //public List<string> SalesOfficeLabelList { get; set; } = new List<string>();
        public List<string> SalesOfficeList { get; set; } = new List<string>();
        //public List<MonthlyQuotePaxDetails> MonthlyQuotePaxDetails { get; set; } = new List<MonthlyQuotePaxDetails>();
    }

    public class SalesOfficeDetails
    {
        //public string SalesOffice { get; set; }
        public List<MonthlyQuotePaxDetails> PaxDetails { get; set; } = new List<MonthlyQuotePaxDetails>();
    }

    public class MonthlyQuotePaxDetails
    {
        public string SalesOffice { get; set; }
        public string Month { get; set; }
        public int Quotes { get; set; }
        public int Pax { get; set; }
        public double Value { get; set; }
        //public QuotePaxDetails PaxDetails { get; set; }
    }

    public class PassengerForecastGrid
    {
        public string SalesOffice { get; set; }
        public List<SalesOfficeWiseDetailsGrid> PaxDetails { get; set; } = new List<SalesOfficeWiseDetailsGrid>();
    }
    public class SalesOfficeWiseDetailsGrid
    {
        public string MonthYear { get; set; }
        public int Quotes { get; set; }
        public int TotalPax { get; set; }
        public double SalesValue { get; set; }
    }


    public class PassengerForecastGraph
    {
        public string MonthYear { get; set; }
        public List<SalesOfficeWiseDetailsGraph> PaxDetails { get; set; } = new List<SalesOfficeWiseDetailsGraph>();
    }
    public class SalesOfficeWiseDetailsGraph
    {
        public string SalesOffice { get; set; }
        public int Quotes { get; set; }
        public int TotalPax { get; set; }
        public double SalesValue { get; set; }
    }

    public class SalesDashboardReq
    {
        public string SalesOfficeID { get; set; }
        public string SalesOffice { get; set; }
        public string SalesPersonID { get; set; }
        public string SalesPerson { get; set; }
        public string DestinationID { get; set; }
        public string Destination { get; set; }
        public string AgentID { get; set; }
        public string Agent { get; set; }
    }

    public class SalesDashboardRes
    {
        public SalesDashboardSummary SalesDashboardSummary { get; set; } = new SalesDashboardSummary();
        ////public List<SalesOfficeDetails> MonthlyQuotePaxDetails { get; set; } = new List<SalesOfficeDetails>();
        //public List<MonthlyQuotePaxDetails> MonthlyQuotePaxDetails { get; set; } = new List<MonthlyQuotePaxDetails>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class SalesDashboardFiltersRes
    {
        public List<ChildrenCompanies> SalesOfficeList { get; set; } = new List<ChildrenCompanies>();
        public List<AgentProperties> AgentList { get; set; } = new List<AgentProperties>();
        public List<AttributeValues> SalesPersonList { get; set; } = new List<AttributeValues>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
    #endregion

    #region Bookings Dashboard
    public class BookingsDashboardSummary
    {
        public string BookingVolumeJson { get; set; }
        public string PassengerVolumeJson { get; set; }
        public string BookingRevenueJson { get; set; }
        public List<string> FinancialYearMonths { get; set; } = new List<string>();
        public List<string> SalesOfficeList { get; set; } = new List<string>();
        public List<PassengerForecastGraph> BookingVolumeGraph { get; set; }
        public List<PassengerForecastGrid> BookingVolumeGrid { get; set; }
    }

    public class BookingsDashboardRes
    {
        public BookingsDashboardSummary BookingsDashboardSummary { get; set; } = new BookingsDashboardSummary();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
    #endregion

}
