using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class OPSVoucher
    {
        public string BookingNumber { get; set; }
        public string PositionNumber { get; set; }
        public string DocumentNumber { get; set; }

        #region Header Section
        public string SysCompanyName { get; set; }
        public string SysCompanyAddress1 { get; set; }
        public string SysCompanyAddress2 { get; set; }
        public string SysCompanyCity { get; set; }
        public string SysCompanyCountry { get; set; }
        public string SysCompanyPostalCode { get; set; }
        public string CustRef { get; set; }
        public string FileHandlerName { get; set; }
        public string FileHandlerContactNo { get; set; }
        public string FileHandlerEmail { get; set; }
        public string VoucherDate { get; set; }
        #endregion

        #region Supplier Details Section
        public string PosSupplierName { get; set; }
        public string PosSupplierAddress1 { get; set; }
        public string PosSupplierAddress2 { get; set; }
        public string PosSupplierCity { get; set; }
        public string PosSupplierPostalCode { get; set; }
        public string PosSupplierCountry { get; set; }
        public string PosSupplierConfirmationNo { get; set; }
        public string PosSupplierContactName { get; set; }
        public string PosSupplierContactNo { get; set; }
        public string PosSupplierContactEmail { get; set; }
        #endregion

        #region ProductName and Booking Section
        public string PosProductName { get; set; }
        public List<string> PassengerBreakdown { get; set; }
        public string Nationality { get; set; }
        #endregion

        #region Service Timings
        public string PosStartDate { get; set; }
        public string PosEndDate { get; set; }
        public string PosStartTime { get; set; }
        public string PosEndTime { get; set; }
        public string PosStartLocation { get; set; }
        public string PosEndLocation { get; set; }
        #endregion

        #region Voucher Note
        public string VoucherNote { get; set; }
        #endregion

        #region Service Details - Service Item
        public List<RoomsAndPriceDetails> RoomsAndPriceDetails { get; set; }
        public string Menu { get; set; } 
        #endregion

        #region Service Details - Option
        public List<ServiceDetailsOption> ServiceDetailsOption { get; set; }

        //Hotel
        public string PosBreakFastType { get; set; }
        public string PosBoardBasis { get; set; }

        //Meals
        public string PosIncWater { get; set; }
        public string PosIncBread { get; set; }
        public string PosIncDessert { get; set; }
        public string PosIncTeaOrCoffee { get; set; }

        //Attraction
        public string PosTicketLocation { get; set; }
        public string PosGuidePurchaseTictet { get; set; }

        //Coach / LDC
        public string PosParking { get; set; }
        public string PosRoadTolls { get; set; }
        public string PosCityPermit { get; set; }
        public string PosDriverName { get; set; }
        public string PosDriverContactNo { get; set; }
        public string PosRegistrationNo { get; set; }
        #endregion

        #region Emergency Contact Details
        public List<EmergencyContacts> EmergencyContacts { get; set; }
        #endregion

        #region Terms and Conditions
        public List<string> TermsAndConditions { get; set; }
        #endregion
    }

    public class RoomsAndPriceDetails
    {
        public string RoomsAndPricesCategoryName { get; set; }
        public string RoomsAndPricesRangeName { get; set; }
        public string RoomsAndPricesChagreBasis { get; set; }
        public string RoomsAndPricesQuantity { get; set; }
        public string RoomsAndPricesMenu { get; set; }
    } 

    public class ServiceDetailsOption
    {
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
    }

    public class OPSVoucherGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public OPSVoucher OPSVoucher { get; set; } = new OPSVoucher();
    }
}
