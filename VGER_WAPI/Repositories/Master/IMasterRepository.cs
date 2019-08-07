using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IMasterRepository
    {  
        IQueryable<Properties> GetGenericMasterForTypeByProperty(MasterTypeRequest request);
        IQueryable<dynamic> GetGenericMasterForTypeByName(MasterTypeRequest Request);
        /// <summary>
        /// To get List of all countries from mResort collection
        /// </summary>
        /// <returns></returns>
        List<Attributes> GetAllCountries();
		/// <summary>
		/// To get country name by city name
		/// </summary>
		/// <param name="cityName"></param>
		/// <returns></returns>
		List<Attributes> GetCountryNameByCityName(List<string> cityName);
		/// <summary>
		/// To get List of all cities from mResort collection
		/// </summary>
		/// <returns></returns>
		List<Attributes> GetAllCitiesByCountryId(string CountryId);
        List<Attributes> GetAllStatesByCountryId(string CountryId);
        IQueryable<dynamic> GetCityNames(CityLookupRequest cityLookupRequest);
        List<CityLookupProperties> GetCityNamesByID(List<string> id);
        IQueryable<Currency> GetCurrencyList();
        CoachesGetResponse GetCoachSizes();
        DefMealPlanGetRes GetDefMealPlan(DefMealPlanGetReq request);
        List<UserSystemContactDetails> GetUserSystemContactDetails();
        List<UserSystemContactDetails> GetActiveUserSystemContactDetails();        
        List<Attributes> GetMarkups();
        List<DefChargeBasis> GetChargeBasis();
        List<mDefPersonType> GetPersonType();
        List<mMealType> GetMealType(DefMealTypeGetReq request);
        List<mProductTemplates> GetProductTemplates(ProductTemplatesGetReq request);
        Task<WorkflowActionGetRes> GetWorkflowAction(WorkflowActionGetReq request);
    }
}
