using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface ICostsheetRepository
    {
        CostsheetGetRes GetCostsheetData(CostsheetGetReq request);

        List<CostsheetVersion> GetCostsheetVersions(CostsheetGetReq request);

        Task<CostsheetVerSetRes> UpdateCostsheetVersion(CostsheetVerSetReq request);

        //below function will check if Active Costheet version has zero price then it will return error msg
        Task<ResponseStatus> CheckActiveCostsheetPrice(CostsheetGetReq request);

        //Task<MailSetRes> SetEmailDetails(MailSetReq request);

        //Task<List<mQRFPackagePrice>> GetlstQrfPackagePrice(CostsheetGetReq request);

        //Task<List<mQRFNonPackagedPrice>> GetlstQrfNonPackagePrice(CostsheetGetReq request);

        //Task<List<mQRFPositionTotalCost>> GetlstQrfPositionTotalCost(CostsheetGetReq request);
    }
}
