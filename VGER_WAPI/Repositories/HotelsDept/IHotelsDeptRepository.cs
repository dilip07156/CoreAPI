using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IHotelsDeptRepository
    {
        #region Hotels Search & View by Booking
        Task<HotelsDeptSearchRes> GetHotelsByBookingDetails(BookingSearchReq request);

        Task<HotelsByBookingGetRes> GetProductHotelDetails(ProductSRPHotelGetReq request);

        Task<HotelAlternateServicesGetRes> GetAlternateServicesByBooking(HotelAlternateServicesGetReq request);

        Task<HotelReservationEmailRes> SendHotelReservationRequestEmail(HotelReservationRequestEmail request);

        Task<AvailabilityRequestDetailsGetRes> GetHotelAvailabilityRequestDetails(AvailabilityRequestDetailsGetReq request);

        Task<HotelReservationEmailRes> UpdateHotelAvailabilityRequest(AvailabilityRequestDetailsSetReq request);

        Task<ActivateHotelDetailsGetRes> GetHotelActivationDetails(AvailabilityRequestDetailsGetReq request);

        Task<HotelReservationEmailRes> UpdateHotelActivationDetails(AvailabilityRequestDetailsGetReq request);

        Task<BudgetSupplementGetRes> GetBudgetSupplement(BudgetSupplementGetReq request);

        Task<CommonResponse> SetBudgetSupplement(BudgetSupplementSetReq request);
        #endregion
    }
}
