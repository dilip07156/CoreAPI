using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;

namespace VGER_WAPI.Repositories
{
    public interface IBookingRepository
    {
        #region mBookings collection
        Task<BookingSearchRes> GetBookingDetails(BookingSearchReq request);

        List<Attributes> GetBookingStatusList();

        Task<BookingSearchRes> GetBookingRoomDetails(BookingSearchReq request);

        Task<BookingSearchRes> GetBookingPositionPricingDetails(BookingSearchReq request);

        Task<BookingDocumentGetRes> GetBookingDocumentDetails(BookingDocumentGetReq request);

        Task<BookingDocumentSetRes> SetBookingDocumentDetails(BookingDocumentSetReq request);

        Task<ResponseStatus> SetBookingBackScriptDetails();
        #endregion

        #region Bookings collection
        Task<BookingSearchRes> GetSearchBookingDetails(BookingSearchReq request);

        Task<BookingInfoRes> GetBookingDetailsByParam(BookingDetailReq request);

        Task<BookingPositionsSetRes> SetBookingPositions(BookingPositionsSetReq request);

        Task<BookingPositionsSetRes> CancelBookingPositions(BookingCancelPositionSetReq request);
        
        #endregion
    }
}
