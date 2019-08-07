using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Repositories.Master
{
    public interface IMasterRepository
    {  
        IQueryable<Properties> GetGenericMasterForTypeByProperty(MasterTypeRequest request);
        IQueryable<dynamic> GetGenericMasterForTypeByName(MasterTypeRequest Request);
        List<CountryLookupProperties> GetCountryNames(CountryLookupRequest countryLookupRequest);
        IQueryable<dynamic> GetCityNames(CityLookupRequestMaster cityLookupRequest);
        List<CityLookupProperties> GetCityNamesByID(List<string> id);
        IQueryable<Currency> GetCurrencyList();
        IQueryable<StatusMaster> GetStatusList();
        DefProductTypeRes GetProductTypes();
    }
}
