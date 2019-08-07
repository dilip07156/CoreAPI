using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IProductSRPRepository
    {
        #region Product SRP
        Task<List<mProducts_Lite>> GetProductDetailsBySearchCriteria(ProductSRPSearchReq request);

        #endregion
    }
}
