using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories.Master
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

            //result.AsQueryable().FirstOrDefault().Attribute.ForEach(b => b.Values.OrderBy(c => c.Value));
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
        public List<Attributes> GetAllCountries()
        {
            return _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "country").Select(x => new Attributes { Attribute_Id = x.Voyager_Resort_Id, AttributeName = x.ResortName }).Distinct().OrderBy(x => x.AttributeName).ToList();
        }

        public List<Attributes> GetCountryNameByCityName(List<string> cityName)
        {
            try
            {
                var list = _MongoContext.mResort.AsQueryable().Where(x => cityName.Contains(x.ResortName)).Select(x => new Attributes { Attribute_Id = x.ResortName, AttributeName = x.ParentResortName }).ToList();
                list.RemoveAll(x => string.IsNullOrWhiteSpace(x.Attribute_Id));
                return list;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region GetCityNames

        public List<Attributes> GetAllCitiesByCountryId(string CountryId)
        {
            return _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city" && x.Voyager_Parent_Resort_Id == CountryId).Select(x => new Attributes { Attribute_Id = x.Voyager_Resort_Id, AttributeName = x.ResortName }).Distinct().OrderBy(x => x.AttributeName).ToList();

            // _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city").Select(x => x.Voyager_ResortType_Id);
            //return _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city").Select(x => new Attributes { Attribute_Id = x.Voyager_Resort_Id, AttributeName = x.ResortName }).Distinct().OrderBy(x=>x.AttributeName).ToList();
        }

        public IQueryable<dynamic> GetCityNames(CityLookupRequest cityLookupRequest)
        {
            if (!string.IsNullOrEmpty(cityLookupRequest.CityName))
            {
                if (!string.IsNullOrEmpty(cityLookupRequest.QRFID))
                {
                    var lstCities = _MongoContext.mQuote.AsQueryable().Where(p => p.QRFID == cityLookupRequest.QRFID).Select(p => p.RoutingInfo).FirstOrDefault().Select(p => p.ToCityName).Distinct();
                    return _MongoContext.mResort.AsQueryable()
                        .Where(c => c.ResortType == "City" && lstCities.Contains(c.Lookup) && c.Lookup.ToLower().Contains(cityLookupRequest.CityName.ToLower().Trim()))
                        .Select(c => new CityLookupProperties { Lookup = c.Lookup, Voyager_Resort_Id = c.Voyager_Resort_Id }).Distinct();
                }
                else
                {
                    return _MongoContext.mResort.AsQueryable()
                        .Where(c => c.ResortType == "City" && c.Lookup.ToLower().Contains(cityLookupRequest.CityName.ToLower().Trim()))
                        .Select(c => new CityLookupProperties { Lookup = c.Lookup, Voyager_Resort_Id = c.Voyager_Resort_Id }).Distinct();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(cityLookupRequest.QRFID))
                {
                    var lstCities = _MongoContext.mQuote.AsQueryable().Where(p => p.QRFID == cityLookupRequest.QRFID).Select(p => p.RoutingInfo).FirstOrDefault().Select(p => p.ToCityName).ToList();
                    return _MongoContext.mResort.AsQueryable()
                        .Where(c => c.ResortType == "City" && lstCities.Contains(c.Lookup))
                        .Select(c => new CityLookupProperties { Lookup = c.Lookup, Voyager_Resort_Id = c.Voyager_Resort_Id }).Distinct();
                }
                else
                {
                    return _MongoContext.mResort.AsQueryable()
                        .Where(c => c.ResortType == "City")
                        .Select(c => new CityLookupProperties { Lookup = c.Lookup, Voyager_Resort_Id = c.Voyager_Resort_Id }).Distinct().Take(100);
                }
            }
        }
        #endregion

        #region GetSatesByCountryId
        public List<Attributes> GetAllStatesByCountryId(string CountryId)
        {
            return _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "state" && x.Voyager_Parent_Resort_Id == CountryId).Select(x => new Attributes { Attribute_Id = x.Voyager_Resort_Id, AttributeName = x.ResortName }).Distinct().OrderBy(x => x.AttributeName).ToList();

            // _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city").Select(x => x.Voyager_ResortType_Id);
            //return _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city").Select(x => new Attributes { Attribute_Id = x.Voyager_Resort_Id, AttributeName = x.ResortName }).Distinct().OrderBy(x=>x.AttributeName).ToList();
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
                         select new Currency { CurrencyCode = c.Currency, CurrencyName = c.Name, CurrencyId = c.VoyagerCurrency_Id };
            return result;
        }

        #endregion

        #region CoachSize

        public CoachesGetResponse GetCoachSizes()
        {
            var response = new CoachesGetResponse();

            var res = from c in _MongoContext.mTransportMaster.AsQueryable()
                      select new { c.Brand, c.Coaches };

            var result = from c in _MongoContext.mTransportCategory.AsQueryable()
                         select c.CategoryName;

            foreach (var x in result)
            {
                response.CategoryName.Add(x);
            }

            foreach (var x in res)
            {
                foreach (var y in x.Brand)
                {
                    response.BrandName.Add(Convert.ToString(y.BrandName));
                }
                foreach (var y in x.Coaches)
                {
                    response.CoachType.Add(Convert.ToString(y.Type));
                }
            }

            return response;
        }

        #endregion

        #region DefMealPlan
        public DefMealPlanGetRes GetDefMealPlan(DefMealPlanGetReq request)
        {
            var resultMealPlan = new List<mDefMealPlan>();
            DefMealPlanGetRes response = new DefMealPlanGetRes();
            if (!string.IsNullOrEmpty(request.Status))
            {
                resultMealPlan = _MongoContext.mDefMealPlan.AsQueryable().Where(a => a.Status == request.Status).ToList();
            }
            else
            {
                resultMealPlan = _MongoContext.mDefMealPlan.AsQueryable().ToList();
            }
            resultMealPlan = resultMealPlan != null && resultMealPlan.Count > 0 ? resultMealPlan : new List<mDefMealPlan>();
            response.mDefMealPlan = resultMealPlan;
            return response;
        }
        #endregion

        #region UserSystemContactDetails
        public List<UserSystemContactDetails> GetUserSystemContactDetails()
        {
            List<UserSystemContactDetails> lstUserSystemContactDetails = new List<UserSystemContactDetails>();
            var compnyids = _MongoContext.mSystem.AsQueryable().Select(a => a.CoreCompany_Id).Distinct().ToList();
            if (compnyids != null && compnyids.Count > 0)
            {
                lstUserSystemContactDetails = _MongoContext.mContacts.AsQueryable().Where(a => compnyids.Contains(a.Company_Id)).Select(a => new UserSystemContactDetails { VoygerContactId = a.VoyagerContact_Id, FirstName = a.FIRSTNAME, LastName = a.LastNAME, IsOperationDefault = a.IsOperationDefault }).Distinct().ToList();
            }
            return lstUserSystemContactDetails;
        }

        public List<UserSystemContactDetails> GetActiveUserSystemContactDetails()
        {
            List<UserSystemContactDetails> lstUserSystemContactDetails = new List<UserSystemContactDetails>();
            var compnyids = _MongoContext.mSystem.AsQueryable().Select(a => a.CoreCompany_Id).Distinct().ToList();
            if (compnyids != null && compnyids.Count > 0)
            {
                List<string> strDeactive = new List<string> { "X", "-", "x" };
                lstUserSystemContactDetails = _MongoContext.mContacts.AsQueryable().Where(a => compnyids.Contains(a.Company_Id) && !strDeactive.Contains(a.STATUS)).Select(a => new UserSystemContactDetails { VoygerContactId = a.VoyagerContact_Id, FirstName = a.FIRSTNAME, LastName = a.LastNAME, IsOperationDefault = a.IsOperationDefault }).Distinct().ToList();
            }
            return lstUserSystemContactDetails;
        }
        #endregion

        #region Markups
        public List<Attributes> GetMarkups()
        {
            return _MongoContext.mMarkups.AsQueryable().Where(x => x.IsDeleted == false).Select(x => new Attributes { Attribute_Id = x.Markups_Id, AttributeName = x.Markup }).Distinct().ToList();
        }
        #endregion

        #region ChargeBasis
        public List<DefChargeBasis> GetChargeBasis()
        {
            var res = (from c in _MongoContext.mProductType.AsQueryable()
                       orderby c.Prodtype
                       select new DefChargeBasis
                       {
                           ChargeBy = c.ChargeBasis,
                           ChargeByDesc = c.ChargeBasisName
                       }).ToList();
            res = res.GroupBy(o => new { o.ChargeBy, o.ChargeByDesc })
                              .Select(o => o.FirstOrDefault()).OrderBy(a => a.ChargeBy).ToList();

            return res;
        }
        #endregion

        #region PersonType
        public List<mDefPersonType> GetPersonType()
        {
            return _MongoContext.mDefPersonType.AsQueryable().OrderBy(x => x.PersonType).ToList();
        }
        #endregion

        #region MealType
        public List<mMealType> GetMealType(DefMealTypeGetReq request)
        {
            return _MongoContext.mMealType.AsQueryable().Where(a => a.ForBreakfast == true).OrderBy(x => x.MealType).ToList();
        }
        #endregion

        #region ProductTemplates
        public List<mProductTemplates> GetProductTemplates(ProductTemplatesGetReq request)
        {
            if (string.IsNullOrEmpty(request?.VoyagerProductTemplate_Id))
                return _MongoContext.mProductTemplates.AsQueryable().ToList();
            else
                return _MongoContext.mProductTemplates.AsQueryable().Where(a => a.VoyagerProductTemplate_Id == request.VoyagerProductTemplate_Id).ToList();

        }
        #endregion

        #region WorkflowAction
        public async Task<WorkflowActionGetRes> GetWorkflowAction(WorkflowActionGetReq request)
        {
            var response = new WorkflowActionGetRes();
            try
            {
                response.WorkflowActions = _MongoContext.Workflow_Actions.AsQueryable().Where(x => x.ModuleParent == request.ModuleParent && x.Module == request.Module && x.Action == request.Action).ToList();
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception e)
            {
                response.ResponseStatus.ErrorMessage = e.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }
        #endregion
    }
}
