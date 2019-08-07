using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface ICostingRepository
    {
        Task<CostingGetRes> GetCostingDetailsByQRFID(CostingGetReq request);

        #region Departures

        QRFDepartureDateGetRes GetDepartureDatesForCostingByQRF_Id(QRFDepartureDateGetReq req);

        #endregion

        #region PaxSlabDetails

        QRFPaxGetResponse GetPaxSlabDetailsForCostingByQRF_Id(QRFPaxSlabGetReq req);

        #endregion
    }
}
