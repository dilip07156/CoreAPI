using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class CostingRepository : ICostingRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        #endregion

        public CostingRepository(IOptions<MongoSettings> settings, IGenericRepository genericRepository, IQuoteRepository quoteRepository, IConfiguration configuration, IUserRepository userRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _quoteRepository = quoteRepository;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<CostingGetRes> GetCostingDetailsByQRFID(CostingGetReq request)
        {
            //var builder = Builders<mQRFPrice>.Filter;
            //var filter = builder.Where(q => q.QRFID == request.QRFID && q.IsCurrentVersion == true);
            //return await _MongoContext.mQRFPrice.Find(filter).Project(q => new CostingGetProperties
            //{
            //    QRFID = q.QRFID,
            //    VersionId = q.VersionId,
            //    VersionName = q.VersionName,
            //    VersionDescription = q.VersionDescription,
            //    IsCurrentVersion = q.IsCurrentVersion,
            //    SalesOfficer = q.SalesOfficer,
            //    CostingOfficer = q.CostingOfficer,
            //    ProductAccountant = q.ProductAccountant,
            //    AgentInfo = q.AgentInfo,
            //    AgentProductInfo = q.AgentProductInfo,
            //    AgentPassengerInfo = q.AgentPassengerInfo,
            //    AgentRoom = q.QRFAgentRoom,
            //    DepartureDates = q.Departures,
            //}).FirstOrDefaultAsync();

            CostingGetRes response = new CostingGetRes();
            var qrfprice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();
           
            response.CostingGetProperties.QRFID = qrfprice.QRFID;
            response.CostingGetProperties.QRFPriceID = qrfprice.QRFPrice_Id;
            response.CostingGetProperties.VersionId = qrfprice.VersionId;
            response.CostingGetProperties.VersionName = qrfprice.VersionName;
            response.CostingGetProperties.VersionDescription = qrfprice.VersionDescription;
            response.CostingGetProperties.IsCurrentVersion = qrfprice.IsCurrentVersion;
            response.CostingGetProperties.SalesOfficer = qrfprice.SalesOfficer;
            response.CostingGetProperties.CostingOfficer = qrfprice.CostingOfficer;
            response.CostingGetProperties.ProductAccountant = qrfprice.ProductAccountant;
            response.CostingGetProperties.ValidForAcceptance = qrfprice.ValidForAcceptance;
            response.CostingGetProperties.ValidForTravel = qrfprice.ValidForTravel;
            response.CostingGetProperties.AgentInfo = qrfprice.AgentInfo;
            response.CostingGetProperties.AgentProductInfo = qrfprice.AgentProductInfo;
            response.CostingGetProperties.AgentPassengerInfo = qrfprice.AgentPassengerInfo;
            response.CostingGetProperties.AgentRoom = qrfprice.QRFAgentRoom;
            response.CostingGetProperties.DepartureDates = qrfprice.Departures;
            response.CostingGetProperties.FollowUpCostingOfficer = qrfprice.FollowUpCostingOfficer;
            response.CostingGetProperties.FollowUpWithClient = qrfprice.FollowUpWithClient;

            ContactDetailsResponse objContactDetailsRes = _userRepository.GetContactsByEmailId(new ContactDetailsRequest { Email = qrfprice.SalesOfficer });
            if (objContactDetailsRes!=null && objContactDetailsRes.Contacts!=null)
            {
                response.CostingGetProperties.SalesOfficerMobile = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.MOBILE) ? objContactDetailsRes.Contacts.MOBILE : objContactDetailsRes.Contacts.TEL;
            }

            response.EnquiryPipeline = _MongoContext.mQuote.AsQueryable().Where(x => x.QRFID == request.QRFID).Select(y => y.CurrentPipeline).FirstOrDefault();

            bool IsLinkedQRFsExist = _quoteRepository.ChcekLinkedQRFsExist(request.QRFID).Result;
            response.IsLinkedQRFsExist = IsLinkedQRFsExist;
            return response;
        }

        #region Departures
        public QRFDepartureDateGetRes GetDepartureDatesForCostingByQRF_Id(QRFDepartureDateGetReq req)
        {
            var response = new QRFDepartureDateGetRes();
            try
            {
                var filters = Builders<mQRFPrice>.Filter.Where(x => x.QRFID == req.QRFID);
                if (_MongoContext.mQRFPrice.Find(filters).Count() > 0)
                {
                    if (req.date == null)
                    {
                        var res = from m in _MongoContext.mQRFPrice.AsQueryable()
                                  where m.QRFID == req.QRFID
                                  select new QRFDepartureDateGetRes { DepartureDates = m.Departures };

                        response.DepartureDates = res.First().DepartureDates.Where(x => x.IsDeleted == false).ToList();
                        if (response.DepartureDates.Count() > 0)
                        {
                            response.Status = "Success";
                        }
                        else
                        {
                            response.Status = "No Departures Found";
                        }
                    }
                    else
                    {
                        var WarnMessage = GetWarning(req.date);
                        var departure = new QRFDepartureDates { Date = req.date, Warning = Convert.ToString(WarnMessage) };
                        response.DepartureDates.Add(departure);
                        response.Status = "Success";
                    }
                }
                else
                {
                    if (req.date != null)
                    {
                        var WarnMessage = GetWarning(req.date);
                        var departure = new QRFDepartureDates { Date = req.date, Warning = Convert.ToString(WarnMessage) };
                        response.DepartureDates.Add(departure);
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Invalid Qrf";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Exception Occured";
            }
            return response;
        }

        public string GetWarning(DateTime? date)
        {
            StringBuilder warn = new StringBuilder();
            if (date != null)
            {
                var res = (from m in _MongoContext.mHotDate.AsQueryable()
                           where (date >= m.StartDate && date <= m.EndDate)
                           select m.Name).Distinct().ToList();

                res.ForEach(x => { warn = warn.Append(x).Append(","); });
                if (warn.Length > 0)
                {
                    warn.Length--;
                }
            }
            return Convert.ToString(warn);
        }
        #endregion

        #region PaxSlabDetails
        public QRFPaxGetResponse GetPaxSlabDetailsForCostingByQRF_Id(QRFPaxSlabGetReq req)
        {
            var response = new QRFPaxGetResponse();
            try
            {
                var filters = Builders<mQRFPrice>.Filter.Where(x => x.QRFID == req.QRFID);
                if (_MongoContext.mQRFPrice.Find(filters).Count() > 0)
                {
                    var res = (from m in _MongoContext.mQRFPrice.AsQueryable()
                               where m.QRFID == req.QRFID
                               select new QRFPaxGetResponse { PaxSlabDetails = m.PaxSlabDetails }).FirstOrDefault().PaxSlabDetails;
                    if (res != null && res.QRFPaxSlabs.Count() > 0)
                    {
                        response.PaxSlabDetails.QRFPaxSlabs = res.QRFPaxSlabs.Where(x => x.IsDeleted == false).ToList();
                        response.PaxSlabDetails.HotelCategories = res.HotelCategories;
                        response.PaxSlabDetails.HotelChain = res.HotelChain;
                        response.PaxSlabDetails.HotelFlag = res.HotelFlag;
                        response.QRFID = req.QRFID;
                        response.Status = "Success";
                    }
                }
                else
                {

                    response.Status = "Invalid Qrf";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Exception Occured";
            }
            return response;
        }

        private QRFPaxSlabDetails GetDefaultPaxSlab()
        {
            QRFPaxSlabDetails response = new QRFPaxSlabDetails();
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFPaxSlab"].ToString() };

            string defCatId = "786fab83-6a95-49c8-ab98-7aa795b3902d";
            string defCat = "Standard";
            string defCoach = "49-Seater with WC and intercom";

            response.CreateDate = DateTime.Now;
            response.EditDate = null;
            response.EditUser = "";
            response.HotelFlag = "no";
            response.QRFPaxSlabs.AddRange(new List<QRFPaxSlabs>
            {
                new QRFPaxSlabs
                {
                    PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                    From = 10,
                    To = 19,
                    DivideByCost = 10,
                    Category = defCat,
                    Category_Id = defCatId,
                    CoachType = defCoach,
                    CoachType_Id = defCoach,
                    Brand = "",
                    Brand_Id = "",
                    HowMany = 1,
                    BudgetAmount = 0,
                    IsDeleted = false,
                    DeleteUser = "",
                    DeleteDate = null,
                    CreateDate = DateTime.Now,
                    EditDate = null
                },
                new QRFPaxSlabs
                {
                    PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                    From = 20,
                    To = 29,
                    DivideByCost = 20,
                    Category = defCat,
                    Category_Id = defCatId,
                    CoachType = defCoach,
                    CoachType_Id = defCoach,
                    Brand = "",
                    Brand_Id = "",
                    HowMany = 1,
                    BudgetAmount = 0,
                    IsDeleted = false,
                    DeleteUser = "",
                    DeleteDate = null,
                    CreateDate = DateTime.Now,
                    EditDate = null
                },
                new QRFPaxSlabs
                {
                    PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                    From = 30,
                    To = 39,
                    DivideByCost = 30,
                    Category = defCat,
                    Category_Id = defCatId,
                    CoachType = defCoach,
                    CoachType_Id = defCoach,
                    Brand = "",
                    Brand_Id = "",
                    HowMany = 1,
                    BudgetAmount = 0,
                    IsDeleted = false,
                    DeleteUser = "",
                    DeleteDate = null,
                    CreateDate = DateTime.Now,
                    EditDate = null
                },
                new QRFPaxSlabs
                {
                    PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                    From = 40,
                    To = 49,
                    DivideByCost = 40,
                    Category = defCat,
                    Category_Id = defCatId,
                    CoachType = defCoach,
                    CoachType_Id = defCoach,
                    Brand = "",
                    Brand_Id = "",
                    HowMany = 1,
                    BudgetAmount = 0,
                    IsDeleted = false,
                    DeleteUser = "",
                    DeleteDate = null,
                    CreateDate = DateTime.Now,
                    EditDate = null
                },
            });

            return response;
        }
        #endregion
    }
}
