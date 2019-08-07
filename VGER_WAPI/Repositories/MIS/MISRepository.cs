using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES.MIS;
//comment before commiting to development
//using Microsoft.Office.Interop.Outlook;
//using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace VGER_WAPI.Repositories
{
    public class MISRepository : IMISRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailRepository;
        #endregion

        public MISRepository(IOptions<MongoSettings> settings, IConfiguration configuration, IGenericRepository genericRepository, IBookingRepository bookingRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
        }

        #region Sales Dashboard
        public async Task<MISMappingRes> CheckMisMappingsRoles(AgentCompanyReq request)
        {
            MISMappingRes response = new MISMappingRes();
            try
            {
                #region MIS Mapping List
                if (request?.Roles?.Count() <= 0 && !string.IsNullOrEmpty(request?.UserId))
                {
                    request.Roles = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.UserId == request.UserId).Select(a => a.RoleName).ToArray();
                }

                if (request?.Roles?.Count() > 0)
                {
                    response.MISMappingList = _MongoContext.mMISMapping.AsQueryable().Where(a => a.UserGroups.Any(b => request.Roles.Contains(b)))
                        .Select(a => new MISMappingDetails { ItemName = a.Item, ItemUrl = a.ItemUrl, ItemSeq = a.ItemSeq }).ToList();
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }
        public async Task<MisSearchGetResList> SearchMisData(SearchMisReqGet request)
        {
            MisSearchGetResList response = new MisSearchGetResList();
            try
            {
                response.MisResults = _MongoContext.mMISMapping.AsQueryable().Select(x => new MisSearchGetRes
                {
                    Type = x.ItemType,
                    Item = x.Item,
                    ItemSeq = x.ItemSeq,
                    _Id = x._Id,
                    Groups = x.UserGroups

                }).ToList();

                if (!string.IsNullOrEmpty(request.TypeName))
                {
                    response.MisResults = response.MisResults.Where(x => x.Type == request.TypeName).ToList();

                }
                if (!string.IsNullOrEmpty(request.Item))
                {
                    response.MisResults = response.MisResults.Where(x => x.Item.ToLower() == request.Item.ToLower().Trim()).ToList();

                }
                if (!string.IsNullOrEmpty(request.RoleName))
                {
                    response.MisResults = response.MisResults.Where(x => x.Groups.Contains(request.RoleName)).ToList();

                }

                response.ResponseStatus.Status = "Success";


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
            }
            return response;

        }
        public async Task<MisSaveResponse> SaveMisData(SearchMisReqGet request)
        {
            mMISMapping mMis = new mMISMapping();
            mMISMapping mMisNew = new mMISMapping();
            MisSaveResponse response = new MisSaveResponse();
            try
            {
                mMis = _MongoContext.mMISMapping.AsQueryable().Where(x => x.ItemType == request.TypeName && x.Item.ToLower() == request.Item.ToLower().Trim()).FirstOrDefault();
                if (mMis != null)
                {
                    mMis.UserGroups.Add(request.RoleName);
                    await _MongoContext.mMISMapping.UpdateOneAsync(Builders<mMISMapping>.Filter.Eq("_Id", mMis._Id),
                           Builders<mMISMapping>.Update.Set("UserGroups", mMis.UserGroups));
                    response.Id = mMis._Id;
                    response.TypeName = mMis.ItemType;
                    response.Item = mMis.Item;
                    response.ItemUrl = mMis.ItemUrl;
                    response.Users = mMis.UserGroups;
                    response.ItemSeq = mMis.ItemSeq;
                    response.Response.Status = "Success";
                    response.Response.StatusMessage = "Successfully Updated Data";
                }
                if (mMis == null)
                {
                    var Sequence = _MongoContext.mMISMapping.AsQueryable().Max(x => x.ItemSeq);
                    mMisNew.ItemType = request.TypeName;
                    mMisNew.Item = request.Item.Trim();
                    mMisNew.ItemSeq = (Sequence + 1);
                    mMisNew.UserGroups.Add(request.RoleName);
                    await _MongoContext.mMISMapping.InsertOneAsync(mMisNew);
                    response.Id = mMisNew._Id;
                    response.TypeName = mMisNew.ItemType;
                    response.Item = mMisNew.Item;
                    response.ItemUrl = mMisNew.ItemUrl;
                    response.Users = mMisNew.UserGroups;
                    response.ItemSeq = mMisNew.ItemSeq;
                    response.Response.Status = "Success";
                    response.Response.StatusMessage = "Successfully Inserted Data";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Response.Status = "Failure";
            }
            return response;
        }
        public async Task<MisSaveResponse> DeleteMisArtifactData(SearchMisReqGet request)
        {
            mMISMapping mMis = new mMISMapping();
            List<string> Roles = new List<string>();
            MisSaveResponse response = new MisSaveResponse();
            try
            {
                mMis = _MongoContext.mMISMapping.AsQueryable().Where(x => x.ItemType == request.TypeName && x.Item == request.Item && x.UserGroups.Contains(request.RoleName)).FirstOrDefault();

                if (mMis != null)
                {
                    Roles.AddRange(mMis.UserGroups);
                    mMis.UserGroups.Clear();
                    Roles.ForEach(x => { if (String.Compare(x, request.RoleName, true) != 0) { mMis.UserGroups.Add(x); } });
                }
                await _MongoContext.mMISMapping.UpdateOneAsync(Builders<mMISMapping>.Filter.Eq("_Id", mMis._Id),
                              Builders<mMISMapping>.Update.Set("UserGroups", mMis.UserGroups));
                response.Response.Status = "Success";
                response.Response.StatusMessage = "Deleted Successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Response.Status = "Failure";
            }
            return response;
        }

        public async Task<SalesDashboardFiltersRes> GetSalesDashboardFiltersList(AgentCompanyReq request)
        {
            SalesDashboardFiltersRes response = new SalesDashboardFiltersRes();
            try
            {
                #region Filters List
                if (!string.IsNullOrEmpty(request.CompanyId))
                {
                    if (request.SpecificFilterName?.ToUpper() == "ALL")
                    {
                        response.SalesOfficeList = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId)
                            .Select(a => a.Branches).FirstOrDefault().OrderBy(a => a.Company_Name).ToList();

                        response.AgentList = _MongoContext.mQuote.AsQueryable().Where(x => x.SystemCompany_Id == request.CompanyId).Select(a => new AgentProperties
                        {
                            Name = a.AgentInfo.AgentName,
                            VoyagerCompany_Id = a.AgentInfo.AgentID
                        }).Distinct().OrderBy(a => a.Name).ToList();
                    }

                    response.SalesPersonList = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId)
                                            .Select(a => a.ContactDetails).FirstOrDefault()
                                            .Where(a => string.IsNullOrWhiteSpace(a.STATUS) && a.Roles != null && a.Roles.Any(b => b.RoleName == "Sales Officer" || b.RoleName == "Sales"))
                                            .Select(a => new AttributeValues { AttributeValue_Id = a.MAIL, Value = a.FIRSTNAME + " " + a.LastNAME })
                                            .OrderBy(a => a.Value).ToList();
                }
                else
                {
                    response.SalesOfficeList = new List<ChildrenCompanies>();

                    response.AgentList = _MongoContext.mQuote.AsQueryable().Select(a => new AgentProperties
                    {
                        Name = a.AgentInfo.AgentName,
                        VoyagerCompany_Id = a.AgentInfo.AgentID
                    }).ToList();
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public async Task<SalesDashboardRes> GetSalesDashboardSummary(SalesDashboardReq request)
        {
            SalesDashboardRes response = new SalesDashboardRes();
            try
            {
                FilterDefinition<mQuote> filter = Builders<mQuote>.Filter.Empty;
                if (!string.IsNullOrWhiteSpace(request.SalesOfficeID) && !string.IsNullOrWhiteSpace(request.SalesOffice))
                {
                    var company = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.SalesOfficeID).FirstOrDefault();
                    var SalesUserList = company?.ContactDetails?.Select(a => a.MAIL).ToList();
                    filter = filter & Builders<mQuote>.Filter.Where(x => SalesUserList.Contains(x.SalesPerson));
                }
                if (!string.IsNullOrWhiteSpace(request.DestinationID) && !string.IsNullOrWhiteSpace(request.Destination))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.Destination, new BsonRegularExpression(new Regex(request.Destination)));
                }
                if (!string.IsNullOrWhiteSpace(request.AgentID) && !string.IsNullOrWhiteSpace(request.Agent))
                {
                    filter = filter & Builders<mQuote>.Filter.Eq(x => x.AgentInfo.AgentName, request.Agent.Trim());
                }
                if (!string.IsNullOrWhiteSpace(request.SalesPersonID) && !string.IsNullOrWhiteSpace(request.SalesPerson))
                {
                    filter = filter & Builders<mQuote>.Filter.Eq(x => x.SalesPerson, request.SalesPersonID.Trim());
                }

                var resSales = _MongoContext.mQuote.Find(filter).ToList().Select(a => new SalesDashboardData
                {
                    QRFID = a.QRFID,
                    Destination = a.AgentProductInfo.Destination,
                    Customer = a.AgentInfo.AgentName,
                    //adultpax = no of departures X no of Adults
                    AdultPax = (a.Departures.Where(b => b.IsDeleted == false).Count()) * (a.AgentPassengerInfo.Where(b => b.Type == "ADULT").Select(b => b.count).FirstOrDefault()),
                    QRFStatus = a.CurrentPipeline,
                    Budget = a.AgentProductInfo.BudgetAmount,
                    SalesOfficer = a.SalesPersonUserName,
                    SalesOffice = a.AgentProductInfo.Division,
                    SalesOfficeID = a.AgentProductInfo.DivisionID,
                    BusinessType = a.AgentProductInfo.Type,
                    BaseCurrency = a.ExchangeRateSnapshot.REFCUR,
                    //SalesValue = (Budget per person X currency conversion to EUR) X no of pax in "Twin" and "Double" rooms X no of departures
                    SalesValue = CalculateSalesValue(a),
                    CostValue = 0,
                    NoOfDepartures = a.Departures.Where(b => b.IsDeleted == false).Count(),
                    InvoiceValue = 0,
                    CreateDate = a.Departures.Where(b => b.IsDeleted == false).OrderBy(b => b.Date).Select(b => b.Date).FirstOrDefault(),
                    StatusDate = a.EditDate,
                    Age = (DateTime.Now.Subtract(Convert.ToDateTime(a.EditDate)).Days) // / 7
                }).ToList();

                var SAP = resSales.Where(a => "Quote Pipeline,Amendment Pipeline".Contains(a.QRFStatus)).ToList();
                var CAP = resSales.Where(a => "Costing Pipeline,Costing Approval Pipeline".Contains(a.QRFStatus)).ToList();
                var AAP = resSales.Where(a => a.QRFStatus == "Agent Approval Pipeline").ToList();
                var GAP = resSales.Where(a => a.QRFStatus == "Handover Pipeline").ToList();
                var monthlist = CurrentFinancialYear(DateTime.Now, "yyyy-MM");
                int year = Convert.ToInt16(monthlist?[0].Substring(0, 4));
                DateTime FinStartDate = new DateTime(year, 4, 1);
                DateTime FinEndDate = new DateTime(year + 1, 3, 31);

                response.SalesDashboardSummary = new SalesDashboardSummary
                {
                    #region Basic Fields
                    SAPQuotes = SAP.Count,
                    SAPPax = Convert.ToInt32(SAP.Sum(b => b.AdultPax)),
                    SAPValue = Convert.ToDouble(SAP.Sum(b => b.SalesValue)),
                    SAPAge1Week = SAP.Count <= 0 ? 0 : (decimal)((SAP.Where(b => b.Age >= 0 && b.Age <= 7).Count()) * 100) / SAP.Count,     //% of Quotes Aging 1 Week
                    SAPAge2Week = SAP.Count <= 0 ? 0 : (decimal)((SAP.Where(b => b.Age >= 8 && b.Age <= 14).Count()) * 100) / SAP.Count,    //% of Quotes Aging 2 Week
                    SAPAge2PlusWeek = SAP.Count <= 0 ? 0 : (decimal)((SAP.Where(b => b.Age > 14).Count()) * 100) / SAP.Count,               //% of Quotes Aging 2+ Week

                    CAPQuotes = CAP.Count,
                    CAPPax = Convert.ToInt32(CAP.Sum(b => b.AdultPax)),
                    CAPValue = Convert.ToDouble(CAP.Sum(b => b.SalesValue)),
                    CAPAge1Week = CAP.Count <= 0 ? 0 : (decimal)((CAP.Where(b => b.Age >= 0 && b.Age <= 7).Count()) * 100) / CAP.Count,     //% of Quotes Aging 1 Week
                    CAPAge2Week = CAP.Count <= 0 ? 0 : (decimal)((CAP.Where(b => b.Age >= 8 && b.Age <= 14).Count()) * 100) / CAP.Count,    //% of Quotes Aging 2 Week
                    CAPAge2PlusWeek = CAP.Count <= 0 ? 0 : (decimal)((CAP.Where(b => b.Age > 14).Count()) * 100) / CAP.Count,               //% of Quotes Aging 2+ Week

                    AAPQuotes = AAP.Count,
                    AAPPax = Convert.ToInt32(AAP.Sum(b => b.AdultPax)),
                    AAPValue = Convert.ToDouble(AAP.Sum(b => b.SalesValue)),
                    AAPAge1Week = AAP.Count <= 0 ? 0 : (decimal)((AAP.Where(b => b.Age >= 0 && b.Age <= 7).Count()) * 100) / AAP.Count,     //% of Quotes Aging 1 Week
                    AAPAge2Week = AAP.Count <= 0 ? 0 : (decimal)((AAP.Where(b => b.Age >= 8 && b.Age <= 14).Count()) * 100) / AAP.Count,    //% of Quotes Aging 2 Week
                    AAPAge2PlusWeek = AAP.Count <= 0 ? 0 : (decimal)((AAP.Where(b => b.Age > 14).Count()) * 100) / AAP.Count,               //% of Quotes Aging 2+ Week

                    GAPQuotes = GAP.Count,
                    GAPPax = Convert.ToInt32(GAP.Sum(b => b.AdultPax)),
                    GAPValue = Convert.ToDouble(GAP.Sum(b => b.SalesValue)),
                    GAPAge1Week = GAP.Count <= 0 ? 0 : (decimal)((GAP.Where(b => b.Age >= 0 && b.Age <= 7).Count()) * 100) / GAP.Count,     //% of Quotes Aging 1 Week
                    GAPAge2Week = GAP.Count <= 0 ? 0 : (decimal)((GAP.Where(b => b.Age >= 8 && b.Age <= 14).Count()) * 100) / GAP.Count,    //% of Quotes Aging 2 Week
                    GAPAge2PlusWeek = GAP.Count <= 0 ? 0 : (decimal)((GAP.Where(b => b.Age > 14).Count()) * 100) / GAP.Count,               //% of Quotes Aging 2+ Week
                    BaseCurrency = resSales.Where(a => !string.IsNullOrEmpty(a.BaseCurrency)).Select(a => a.BaseCurrency).FirstOrDefault(),
                    FinancialYearMonths = CurrentFinancialYear(DateTime.Now),
                    #endregion

                    PassengerForecastGraph = resSales.Where(a => !string.IsNullOrEmpty(a.SalesOffice) && (a.CreateDate >= FinStartDate && a.CreateDate <= FinEndDate))
                    .GroupBy(a => new { a.CreateDate.Value.Year, a.CreateDate.Value.Month })
                    .Select(a => new PassengerForecastGraph
                    {
                        MonthYear = Convert.ToDateTime(a.FirstOrDefault().CreateDate).ToString("yyyy-MM"),
                        PaxDetails = a.GroupBy(b => b.SalesOffice).Select(b => new SalesOfficeWiseDetailsGraph
                        {
                            SalesOffice = b.FirstOrDefault().SalesOffice,
                            Quotes = b.Count(),
                            TotalPax = Convert.ToInt32(b.Sum(c => c.AdultPax)),
                            SalesValue = Convert.ToDouble(b.Sum(c => c.SalesValue))
                        }).OrderBy(b => b.SalesOffice).ToList()
                    }).ToList(),

                    PassengerForecastGrid = resSales.Where(a => !string.IsNullOrEmpty(a.SalesOffice) && (a.CreateDate >= FinStartDate && a.CreateDate <= FinEndDate))
                    .GroupBy(a => a.SalesOffice)
                    .Select(a => new PassengerForecastGrid
                    {
                        SalesOffice = a.FirstOrDefault().SalesOffice,
                        PaxDetails = a.GroupBy(b => new { b.CreateDate.Value.Month, b.CreateDate.Value.Year }).Select(b => new SalesOfficeWiseDetailsGrid
                        {
                            MonthYear = Convert.ToDateTime(b.FirstOrDefault().CreateDate).ToString("MMM yyyy"),
                            Quotes = b.Count(),
                            TotalPax = Convert.ToInt32(b.Sum(c => c.AdultPax)),
                            SalesValue = Convert.ToDouble(b.Sum(c => c.SalesValue))
                        }).ToList()
                    }).OrderBy(a => a.SalesOffice).ToList(),
                };

                //response.SalesDashboardSummary.SalesOfficeList = response.SalesDashboardSummary.PassengerForecastGrid.Select(a => a.SalesOffice.Replace(" ", "_")).ToList();
                response.SalesDashboardSummary.SalesOfficeList = response.SalesDashboardSummary.PassengerForecastGrid.Select(a => a.SalesOffice).ToList();

                monthlist.ForEach(b =>
                {
                    if (response.SalesDashboardSummary.PassengerForecastGraph.Where(a => a.MonthYear == b).Count() < 1)
                    {
                        response.SalesDashboardSummary.PassengerForecastGraph.Add(new PassengerForecastGraph { MonthYear = b, PaxDetails = new List<SalesOfficeWiseDetailsGraph>() });
                    }
                });

                //foreach (var month in monthlist)
                //{
                //    if (response.SalesDashboardSummary.PassengerForecast1.Where(a => a.MonthYear == month).Count() < 1)
                //    {
                //        response.SalesDashboardSummary.PassengerForecast1.Add(new PassengerForecast1 { MonthYear = month, PaxDetails = new List<SalesOfficeWiseDetails1>() });
                //    }
                //}

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }
        #endregion

        #region Bookings Dashboard
        public BookingsDashboardRes GetBookingsDashboardSummary(SalesDashboardReq request)
        {
            BookingsDashboardRes response = new BookingsDashboardRes();
            try
            {
                List<string> statusList = new List<string> { "C", "!", "J" };

                #region filters
                //FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;

                //if (!string.IsNullOrWhiteSpace(request.SalesOfficeID) && !string.IsNullOrWhiteSpace(request.SalesOffice))
                //{
                //    var company = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.SalesOfficeID).FirstOrDefault();
                //    var SalesUserList = company?.ContactDetails?.Select(a => a.MAIL).ToList();
                //    filter = filter & Builders<mQuote>.Filter.Where(x => SalesUserList.Contains(x.SalesPerson));
                //}
                //if (!string.IsNullOrWhiteSpace(request.DestinationID) && !string.IsNullOrWhiteSpace(request.Destination))
                //{
                //    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.Destination, new BsonRegularExpression(new Regex(request.Destination)));
                //}
                //if (!string.IsNullOrWhiteSpace(request.AgentID) && !string.IsNullOrWhiteSpace(request.Agent))
                //{
                //    filter = filter & Builders<mQuote>.Filter.Eq(x => x.AgentInfo.AgentName, request.Agent.Trim());
                //}
                //if (!string.IsNullOrWhiteSpace(request.SalesPersonID) && !string.IsNullOrWhiteSpace(request.SalesPerson))
                //{
                //    filter = filter & Builders<mQuote>.Filter.Eq(x => x.SalesPerson, request.SalesPersonID.Trim());
                //}
                #endregion

                var resSales = _MongoContext.Bookings.AsQueryable().Where(a => !statusList.Contains(a.STATUS)).ToList().Select(a => new SalesDashboardData
                {
                    QRFID = a.BookingNumber,
                    AdultPax = a.BookingPax.Where(b => b.PERSTYPE == "ADULT").FirstOrDefault()?.PERSONS,
                    //Budget = 1,//a.AgentProductInfo.BudgetAmount,
                    SalesOfficer = a.StaffDetails?.Staff_SalesUser_Name,
                    SalesOffice = a.AgentInfo?.Division_Name,     //a.AgentProductInfo.Division,          //check
                    SalesOfficeID = a.AgentInfo?.Division_ID,      //a.AgentProductInfo.DivisionID,        //check
                    //SalesValue = (Budget per person X currency conversion to EUR) X no of pax in "Twin" and "Double" rooms X no of departures
                    SalesValue = 1,                                         //CalculateSalesValue(a),               //check
                    NoOfDepartures = 1,
                    CreateDate = a.AuditTrail.CREA_DT,
                    StatusDate = a.AuditTrail.MODI_DT,
                    Age = (DateTime.Now.Subtract(Convert.ToDateTime(a.AuditTrail.MODI_DT)).Days) // / 7
                }).ToList();

                var monthlist = CurrentFinancialYear(DateTime.Now, "yyyy-MM");
                int year = Convert.ToInt16(monthlist?[0].Substring(0, 4));
                DateTime FinStartDate = new DateTime(year, 4, 1);
                DateTime FinEndDate = new DateTime(year + 1, 3, 31);

                response.BookingsDashboardSummary = new BookingsDashboardSummary
                {

                    FinancialYearMonths = CurrentFinancialYear(DateTime.Now),

                    BookingVolumeGraph = resSales.Where(a => !string.IsNullOrEmpty(a.SalesOffice) && (a.CreateDate >= FinStartDate && a.CreateDate <= FinEndDate))
                    .GroupBy(a => new { a.CreateDate.Value.Year, a.CreateDate.Value.Month })
                    .Select(a => new PassengerForecastGraph
                    {
                        MonthYear = Convert.ToDateTime(a.FirstOrDefault().CreateDate).ToString("yyyy-MM"),
                        PaxDetails = a.GroupBy(b => b.SalesOffice).Select(b => new SalesOfficeWiseDetailsGraph
                        {
                            SalesOffice = b.FirstOrDefault().SalesOffice,
                            Quotes = b.Count(),
                            TotalPax = Convert.ToInt32(b.Sum(c => c.AdultPax)),
                            SalesValue = Convert.ToDouble(b.Sum(c => c.SalesValue))
                        }).OrderBy(b => b.SalesOffice).ToList()
                    }).ToList(),

                    BookingVolumeGrid = resSales.Where(a => !string.IsNullOrEmpty(a.SalesOffice) && (a.CreateDate >= FinStartDate && a.CreateDate <= FinEndDate))
                    .GroupBy(a => a.SalesOffice)
                    .Select(a => new PassengerForecastGrid
                    {
                        SalesOffice = a.FirstOrDefault().SalesOffice,
                        PaxDetails = a.GroupBy(b => new { b.CreateDate.Value.Month, b.CreateDate.Value.Year }).Select(b => new SalesOfficeWiseDetailsGrid
                        {
                            MonthYear = Convert.ToDateTime(b.FirstOrDefault().CreateDate).ToString("MMM yyyy"),
                            Quotes = b.Count(),
                            TotalPax = Convert.ToInt32(b.Sum(c => c.AdultPax)),
                            SalesValue = Convert.ToDouble(b.Sum(c => c.SalesValue))
                        }).ToList()
                    }).OrderBy(a => a.SalesOffice).ToList(),
                };

                response.BookingsDashboardSummary.SalesOfficeList = response.BookingsDashboardSummary.BookingVolumeGrid.Select(a => a.SalesOffice).ToList();

                monthlist.ForEach(b =>
                {
                    if (response.BookingsDashboardSummary.BookingVolumeGraph.Where(a => a.MonthYear == b).Count() < 1)
                    {
                        response.BookingsDashboardSummary.BookingVolumeGraph.Add(new PassengerForecastGraph { MonthYear = b, PaxDetails = new List<SalesOfficeWiseDetailsGraph>() });
                    }
                });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        #endregion

        #region Helper Methods
        private static double CalculateSalesValue(mQuote a)
        {
            double value, FromCurrencyRate, BaseCurrencyRate, TwinRooms = 0;
            FromCurrencyRate = Convert.ToDouble(a.ExchangeRateSnapshot?.ExchangeRateDetail?.Where(b => b.CURRENCY == a.AgentProductInfo?.BudgetCurrencyCode).Select(b => b.RATE).FirstOrDefault());
            BaseCurrencyRate = Convert.ToDouble(a.ExchangeRateSnapshot?.ExchangeRateDetail?.Where(b => b.CURRENCY == a.ExchangeRateSnapshot?.REFCUR).Select(b => b.RATE).FirstOrDefault());
            TwinRooms = Convert.ToDouble(a.AgentRoom?.Where(b => "Twin,Double".Contains(b.RoomTypeName)).Select(b => b.RoomCount).FirstOrDefault());

            value = Math.Round(
                        (Convert.ToDouble(a.AgentProductInfo?.BudgetAmount) / ((FromCurrencyRate == 0 ? 1 : FromCurrencyRate) / (BaseCurrencyRate == 0 ? 1 : BaseCurrencyRate))) *
                        (2 * TwinRooms) * a.Departures.Where(b => b.IsDeleted == false).Count());
            return value;
        }

        public List<string> CurrentFinancialYear(DateTime date, string monthYearFormat = "MMM yyyy")
        {
            List<string> FinancialYearMonths = new List<string>();
            try
            {
                int year = (date.Month >= 4 ? date.Year + 1 : date.Year);

                if (monthYearFormat == "MMM yyyy")
                {
                    FinancialYearMonths.AddRange(new List<string>
                    {
                        "APR " + (year-1), "MAY " + (year-1), "JUN " + (year-1), "JUL " + (year-1), "AUG " + (year-1), "SEP " + (year-1),
                        "OCT " + (year-1), "NOV " + (year-1), "DEC " + (year-1), "JAN " + year, "FEB " + year, "MAR " + year
                    });
                }
                else if (monthYearFormat == "yyyy-MM")
                {
                    FinancialYearMonths.AddRange(new List<string>
                    {
                        (year-1) + "-04", (year-1) + "-05", (year-1) + "-06", (year-1) + "-07" , (year-1) + "-08" , (year-1) + "-09" ,
                        (year-1) + "-10", (year-1) + "-11", (year-1) + "-12", year + "-01", year + "-02", year + "-03"
                    });
                }
            }
            catch (Exception ex)
            { }
            return FinancialYearMonths;
        }
        #endregion
    }
}