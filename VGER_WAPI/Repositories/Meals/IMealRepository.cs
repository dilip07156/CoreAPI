using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IMealRepository
    {
        Task<MealsGetRes> GetMealsDetailsByQRFID(QuoteGetReq request);

        Task<MealSetRes> SetMealDetailsByID(MealSetReq request);

        Task<MealVenueSetRes> SetMealVenueDetailsByID(MealVenueSetReq request);
        
        Task<MealVenueGetRes> GetMealVenueDetailsByID(MealVenueGetReq request);
    }
}
