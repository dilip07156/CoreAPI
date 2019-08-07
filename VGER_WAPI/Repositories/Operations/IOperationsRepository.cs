using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;

namespace VGER_WAPI.Repositories
{
	public interface IOperationsRepository
	{
		#region Hotels Search & View by Booking
		Task<OpsBookingSearchRes> GetBookingDetails(OpsBookingSearchReq request);

		Task<OpsBookingSummaryGetRes> GetOpsBookingSummary(ProductSRPHotelGetReq request);
		#endregion

		#region View Service Status->Itinerary
		Task<OpsBookingItineraryGetRes> GetBookingItineraryDetails(OpsBookingItineraryGetReq request);

		Task<OpsBookingItineraryGetRes> GetPositionTypeByPositionId(OpsBookingItineraryGetReq request);
		#endregion

		#region Opeartion Common Service
		Task<OperationHeaderInfo> GetOperationHeaderDetails(OpsHeaderGetReq request);

		Task<OpsProductTypeGetRes> GetOpsProductTypeDetails(OpsProductTypeGetReq request);

		Task<OpsProductTypeGetRes> GetProdTypePositionByParam(OpsProdTypePositionGetReq request);

        Task<OpsProdRangePersTypeGetRes> GetPersonTypeByProductRange(OpsProdRangePersTypeGetReq request);
        #endregion

        #region Position FOC
        Task<OpsFOCSetRes> SetBookingPositionFOC(OpsFOCSetReq request);
		#endregion

		#region Financials
		Task<OpsFinancialsGetRes> GetOpsFinancialDetails(OpsFinancialsGetReq request);
		#endregion

		#region RoomingList
		Task<BookingRoomingSetResponse> SetRoomingDetails(SetPassengerDetailsReq request);
		Task<BookingRoomingGetResponse> GetRoomingDetails(BookingRoomingGetRequest request);
        Task<PositionsFromBookingGetRes> GetPositionsFromBooking(PositionsFromBookingGetReq request);
        Task<BookingRoomsSetResponse> UpdateBookingRoomsDataAsperExcel(BookingRoomsSetRequest request);
        Task<BookingRoomingSetResponse> SaveRoomingListPersonDetails(SetPassengerDetailsReq request);
        Task<BookingRoomGetResponse> GetOpsBookingRoomsDetails(BookingRoomsGetRequest request);
        Task<BookingPaxDetailsGetResponse> GetOpsBookingPaxDetails(BookingPaxDetailsGetRequest request);
        Task<QrfPackagePriceGetRes> GetQRFPackagePriceForRoomsDetails(QrfPackagePriceGetReq request);
        Task<BookingRoomHotelsGetRes> GetRoomingDetailsForHotels(BookingRoomingGetRequest request);
        Task<OpsBookingSetRes> CancelRoomingListUpdate(string BookingNumber);
        #endregion

        #region Ops Booking Position
        Task<OpsBookingSetRes> SetBookingByWorkflow(OpsBookingSetReq request);
        #endregion

        #region Itinerary Builder
        Task<OpsBookingItineraryGetRes> GetItineraryBuilderDetails(OpsBookingItineraryGetReq request);

		Task<OpsBookingItineraryGetRes> GetItineraryBuilderPositionDetailById(OpsBookingItineraryGetReq request);

		Task<OpsBookingItinerarySetRes> SetRemarksForItineraryBuilderDetails(OpsBookingItinerarySetReq request);

		Task<OpsBookingItinerarySetRes> SetItineraryBuilderDetails(OpsBookingItinerarySetReq request);
        #endregion

        #region Payment Schedule
        Task<PaymentScheduleGetRes> GetPositionPaymentSchedule(PaymentScheduleGetReq request);
        #endregion
    }
}
