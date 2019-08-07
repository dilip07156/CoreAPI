using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using VGER_DISTRIBUTION.Helpers;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Repositories.Master
{
    public class MasterRepository : IMasterRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public MasterRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region GetGenericMasterForType

        public IQueryable<Properties> GetGenericMasterForTypeByProperty(MasterTypeRequest Request)
        {

            var result = (from t in _MongoContext.mTypeMaster.AsQueryable()
                          where t.PropertyType.PropertyName == Request.Property
                          select t.PropertyType).ToList();
            //new Properties
            //{
            //    PropertyName = t.PropertyType.PropertyName,
            //    Property_Id = t.PropertyType.Property_Id,
            //    Attribute = t.PropertyType.Attribute
            //};
            //var result2 = _MongoContext.mTypeMaster.AsQueryable().Where(a => a.PropertyType.PropertyName == Request.Property).Select(b => b.PropertyType);
            //var result1 = result.ToList();
            result.ForEach(a =>
            {
                a.Attribute.ForEach(b =>
                {
                    b.Values = b.Values.OrderBy(c => c.SequenceNo).ToList();
                });
            });

            return result.AsQueryable();
        }

        public IQueryable<dynamic> GetGenericMasterForTypeByName(MasterTypeRequest Request)
        {
            var result = from t in _MongoContext.mTypeMaster.AsQueryable()
                         where t.PropertyType.PropertyName == Request.Property
                         select t;

            var results = from u in result.AsQueryable()
                          select u.PropertyType.Attribute.Where(y => y.AttributeName == Request.Name);

            return results;
        }

        #endregion

        #region GetCountryNames
        public List<CountryLookupProperties> GetCountryNames(CountryLookupRequest countryLookupRequest)
        {
            //var country = from c in _MongoContext.mResort.AsQueryable()
            //             where c.ResortType == "Country"
            //             select c;
            //var cities = from c in _MongoContext.mResort.AsQueryable()
            //             where c.ResortType == "City"
            //             select c;

            var country = _MongoContext.mResort.AsQueryable().Where(a => a.ResortType == "Country");
            var city = _MongoContext.mResort.AsQueryable().Where(a => a.ResortType == "City");

            if (!string.IsNullOrEmpty(countryLookupRequest.CountryName))
            {
                country = country
                    .Where(c => c.ResortName.ToLower().Contains(countryLookupRequest.CountryName.ToLower()))
                    .Select(c => c).AsQueryable();
            }

            var lstCountry = country.ToList();
            bool includeCities = false;
            //if (!string.IsNullOrEmpty(countryLookupRequest.CountryName))
            // {
            if (countryLookupRequest.IncludeCities == "Y")
                includeCities = true;
            //}
            List<CountryLookupProperties> result = new List<CountryLookupProperties>();

            if (includeCities)
            {
                var lstCity = city.ToList();

                foreach (var con in lstCountry)
                {
                    var data = new CountryLookupProperties();
                    data.Lookup = con.Lookup;
                    data.CountryName = con.ResortName;
                    data.ContinentName = con.ParentResortName;
                    data.Voyager_Resort_Id = con.Voyager_Resort_Id;
                    data.Nationality = con.Nation;
                    data.Language = con.Language;
                    List<CityLookupProperties> citiyResult = new List<CityLookupProperties>();

                    data.Cities = lstCity.Where(a => a.Voyager_Parent_Resort_Id == con.Voyager_Resort_Id).Select(ct => new CityLookupProperties
                    { CityName = ct.ResortName, Lookup = ct.Lookup, Voyager_Resort_Id = ct.Voyager_Resort_Id }
                            ).Distinct().ToList();

                    result.Add(data);
                }
            }
            else
            {
                result = lstCountry.Select(c => new CountryLookupProperties
                {
                    Lookup = c.Lookup,
                    CountryName = c.ResortName,
                    ContinentName = c.ParentResortName,
                    Voyager_Resort_Id = c.Voyager_Resort_Id,
                    Nationality = c.Nation,
                    Language = c.Language
                }).ToList();
            }

            return result;
        }
        #endregion

        #region GetCityNames
        public IQueryable<dynamic> GetCityNames(CityLookupRequestMaster cityLookupRequest)
        {
            var resort = from c in _MongoContext.mResort.AsQueryable()
                         select c;
            if (!string.IsNullOrEmpty(cityLookupRequest.VoyagerCountry_Id))
            {
                resort = resort
                    .Where(c => c.Voyager_Parent_Resort_Id.ToLower() == cityLookupRequest.VoyagerCountry_Id.ToLower())
                    .Select(c => c).AsQueryable();
            }
            if (!string.IsNullOrEmpty(cityLookupRequest.CountryName))
            {
                resort = resort
                    .Where(c => c.ParentResortName.ToLower().Contains(cityLookupRequest.CountryName.ToLower()))
                    .Select(c => c).AsQueryable();
            }
            if (!string.IsNullOrEmpty(cityLookupRequest.CityName))
            {
                resort = resort
                    .Where(c => c.Lookup.ToLower().Contains(cityLookupRequest.CityName.ToLower()))
                    .Select(c => c).AsQueryable();
            }
            return resort
                .Where(c => c.ResortType == "City")
                .Select(c => new CityLookupProperties { Lookup = c.Lookup, CityName = c.ResortName, CountryName = c.ParentResortName, Voyager_Resort_Id = c.Voyager_Resort_Id, Latitude = c.Lat, Longitude = c.Lon }).Distinct();

        }
        #endregion

        #region GetCityNamesByID
        public List<CityLookupProperties> GetCityNamesByID(List<string> id)
        {
            return _MongoContext.mResort.AsQueryable()
                    .Where(c => c.ResortType == "City" && id.Contains(c.Voyager_Resort_Id))
                    .Select(c => new CityLookupProperties { Lookup = c.Lookup, Voyager_Resort_Id = c.Voyager_Resort_Id }).Distinct().ToList();
        }
        #endregion

        #region GetCurrency

        public IQueryable<Currency> GetCurrencyList()
        {
            var result = from c in _MongoContext.mCurrency.AsQueryable()
                         orderby c.Currency
                         select new Currency { CurrencyCode = c.Currency, CurrencyName = c.Name };
            return result;
        }

        #endregion

        #region GetStatus

        public IQueryable<StatusMaster> GetStatusList()
        {
            var result = from c in _MongoContext.mStatus.AsQueryable()
                         where c.ForBooking == true || c.ForPosition == true || c.ForContact == true || c.ForDocument == true || c.ForProduct == true
                         orderby c.Status
                         select new StatusMaster
                         {
                             Status = c.Status,
                             Description = c.Description,
                             ForBooking = (c.ForBooking ? c.ForBooking : false),
                             ForContact = c.ForContact,
                             ForDocument = c.ForDocument,
                             ForPosition = c.ForPosition,
                             ForProduct = c.ForProduct

                         };
            return result;
        }

        #endregion

        #region ProductType

        public DefProductTypeRes GetProductTypes()
        {
            var response = new DefProductTypeRes();

            var res = (from c in _MongoContext.mProductType.AsQueryable()
                       orderby c.Prodtype
                       select new DefProductType
                       {
                           VoyagerProductTypeId = c.VoyagerProductType_Id,
                           ProductType = c.Prodtype,
                           ChargeBy = c.ChargeBasis,
                           ChargeByDesc = c.ChargeBasisName
                       }).ToList();
            response.DefProductType = res;


            return response;
        }

        #endregion 
    }
}
