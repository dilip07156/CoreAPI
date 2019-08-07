using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IHandoverRepository
    {
        #region Get Set mGoAhead (AttachToMaster)
        Task<GoAheadGetRes> GetGoAhead(GoAheadGetReq request);

        Task<GetGoAheadDepatureRes> GetGoAheadDepature(GoAheadGetReq request);

        Task<GoAheadSetRes> SetGoAhead(GoAheadSetReq request);

        Task<SetMaterialisationRes> SetMaterialisation(SetMaterialisationReq request);
        #endregion

        #region Handover
        Task<ConfirmBookingSetRes> SetGoAheadConfirmMessage(ConfirmBookingSetReq request);

        Task<ConfirmBookingGetRes> GoAheadQuotes(ConfirmBookingGetReq request);

        Task<HandoverGetRes> GetGoAheadDeparturesDetails(GoAheadGetReq request);
        #endregion

        #region Add New Departures
        Task<GoAheadNewDeptGetRes> GetGoAheadExistDepartures(GoAheadGetReq request);

        Task<GoAheadNewDeptSetRes> SetGoAheadNewDepartures(GoAheadNewDeptSetReq request);
        #endregion
    }
}
