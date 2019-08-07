using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IItineraryRepository
    {
        #region Itinerary
        Task<mItinerary> GetItinerary(ItineraryGetReq request);

        Task<mItinerary> GetItineraryDetails(ItineraryGetReq request);

        Task<ItinerarySetRes> SetItinerary(ItinerarySetReq request);

        Task<ItinerarySetRes> EnableDisablePosition(ItinerarySetReq request);

        Task<ItinerarySetRes> SaveRemarks(ItinerarySetReq request);
        #endregion

        #region QRFPosition
        Task<ItineraryGetRes> GetQRFPosition(ItineraryGetReq request);
        #endregion
    }
}
