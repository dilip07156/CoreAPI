using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    #region Get Request & response

    public class AvailabilityRequestDetailsGetReq
    {
        public string QRFID { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public string UserEmailId { get; set; }
        public string PlacerUserId { get; set; }
        public string MailStatus { get; set; }
        public string MailType { get; set; }
        public string PageType { get; set; }
        public string OptionDate { get; set; }
        public string CancellationDeadline { get; set; }

    }

    public class AvailabilityRequestDetailsGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public CostingGetProperties CostingGetProperties { get; set; } = new CostingGetProperties();
        public ReservationRequestDetails ReservationRequestDetails { get; set; } = new ReservationRequestDetails();
        public List<ProductSRPDetails> ProductSRPDetails { get; set; } = new List<ProductSRPDetails>();
        public List<Currency> CurrencyList { get; set; } = new List<Currency>();
        public UpdateAvailabilityReqDetails UpdateReqDetails { get; set; } = new UpdateAvailabilityReqDetails();
        public List<AltSvcRoomsAndPrices> AltSvcRoomsAndPrices { get; set; } = new List<AltSvcRoomsAndPrices>();
        public List<BudgetSupplements> BudgetSupplements { get; set; } = new List<BudgetSupplements>();
    }

    public class ReservationRequestDetails
    {
        public string Check_In { get; set; }
        public string Check_Out_Date { get; set; }
        public string Nights { get; set; }
        public string Nationality { get; set; }
        public string Board_Basis { get; set; }
        public int? Stars { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
    }

    public class ActivateHotelDetailsGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public PositionProductDetails PositionProductDetails { get; set; } = new PositionProductDetails();
        public ReservationRequestDetails ReservationRequestDetails { get; set; } = new ReservationRequestDetails();
        public UpdateAvailabilityReqDetails UpdateReqDetails { get; set; } = new UpdateAvailabilityReqDetails();
        public List<AltSvcRoomsAndPrices> AltSvcRoomsAndPrices { get; set; } = new List<AltSvcRoomsAndPrices>();
        public List<ProductSRPDetails> PosProductSRPDetails { get; set; } = new List<ProductSRPDetails>();
        public List<ProductSRPDetails> AltProductSRPDetails { get; set; } = new List<ProductSRPDetails>();
    }

    public class PositionProductDetails
    {
        public string ProductName { get; set; }
        public string ProductCity { get; set; }
        public string ProductCountry { get; set; }
        public string PositionRooms { get; set; }
        public string PositionTwinRate { get; set; }
        public string PositionStatus { get; set; }
        //public string PositionDate { get; set; }
        //public string PositionCity { get; set; }
        //public string PositionCountry { get; set; }
    }

    public class BudgetSupplementGetReq
    {
        public string QRFID { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public string UserEmailId { get; set; }
        public List<AltSvcRoomsAndPrices> AltSvcRoomsAndPrices { get; set; } = new List<AltSvcRoomsAndPrices>();
    }

    public class BudgetSupplementGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public string QRFID { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public List<BudgetSupplements> BudgetSupplements { get; set; } = new List<BudgetSupplements>();
    }

    public class BudgetSupplementSetReq
    {
        public string QRFID { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public string UserEmailId { get; set; }
        public List<BudgetSupplements> BudgetSupplements { get; set; } = new List<BudgetSupplements>();
    }
    #endregion

    #region Set Request & response
    public class AvailabilityRequestDetailsSetReq
    {
        public string QRFID { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public string UserEmailId { get; set; }
        public string PlacerUserId { get; set; }
        public string Caller { get; set; }

        /// <summary>
        /// This is optional. If not provided then will be fetched from alternate service supplier info
        /// </summary>
        public string SupplierId { get; set; }

        /// <summary>
        /// the status of the request e.g. Available/Waitlist/Unavailable
        /// </summary>
        public string Status { get; set; }

        public UpdateAvailabilityReqDetails UpdateReqDetails { get; set; } = new UpdateAvailabilityReqDetails();
        public List<AltSvcRoomsAndPrices> AltSvcRoomsAndPrices { get; set; } = new List<AltSvcRoomsAndPrices>();
        public List<BudgetSupplements> BudgetSupplements { get; set; } = new List<BudgetSupplements>();
    }

    #endregion

    #region common declaration for get & set

    public class AltSvcRoomsAndPrices
    {
        public string BookingRoomsId { get; set; }
        public string RoomName { get; set; }
        public string PersonType { get; set; }
        public string Quantity { get; set; }

        [Required]
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        [Required]
        public string RoomRate { get; set; }
    }

    public class UpdateAvailabilityReqDetails
    {
        public DateTime? OptionDate { get; set; }
        public int? CancellationDeadline { get; set; }
        public string Availability { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ReservationsEmail { get; set; }

        //[Required]
        public string Telephone { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

		public string SupplierId { get; set; }

	}
    #endregion
}