using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IPositionRepository
    {
        #region Get Set Position
        Task<PositionGetRes> GetPosition(PositionGetReq request);

        Task<PositionSetRes> SetPosition(PositionSetReq request);
        #endregion

        #region Prices
        Task<PositionPriceGetRes> GetPositionPrice(PositionPriceGetReq request);

        Task<PositionPriceSetRes> SetPositionPrice(PositionPriceSetReq request);

        Task<bool> SetAllPriceFOCByQRFID(string QRFID, string UserName);
        #endregion

        #region FOC
        Task<PositionFOCGetRes> GetPositionFOC(PositionFOCGetReq request);
        Task<PositionFOCSetRes> SetPositionFOC(PositionFOCSetReq request);
        #endregion

        #region Get Dynamic Tour Entity
        Task<TourEntitiesGetRes> GetDynamicTourEntities(TourEntitiesGetReq request);
        #endregion

        #region Get QuickPick Activities
        Task<PosQuicePickGetRes> GetQuickPickActivities(PositionGetReq request);
        #endregion

        #region Set DefaultMealPlan Accomodation
        Task<PositionDefMealSetRes> SetDefaultMealPlan(PositionDefMealSetReq request);
        #endregion

        #region PositionRoomDetails
        Task<PositionRoomsGetRes> GetPositionRoomDetails(PositionRoomsGetReq request);

        Task<PositionRoomsSetRes> SetPositionRoomDetails(PositionRoomsSetReq request);
        #endregion 
    }
}
