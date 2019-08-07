using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace VGER_WAPI.Repositories
{
    public class CommercialsRepository : ICommercialsRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IQRFSummaryRepository _qRFSummaryRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailRepository;
        private readonly IQuoteRepository _quoteRepository;

        #endregion

        public CommercialsRepository(IConfiguration configuration, IOptions<MongoSettings> settings, IGenericRepository genericRepository, IQRFSummaryRepository qRFSummaryRepository, IHostingEnvironment env, IEmailRepository emailRepository,IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _qRFSummaryRepository = qRFSummaryRepository;
            _env = env;
            _configuration = configuration;
            _emailRepository = emailRepository;
            _quoteRepository = quoteRepository;
        }

        public CommercialsGetRes GetCommercials(CommercialsGetReq request)
        {
            CommercialsGetRes response = new CommercialsGetRes();

            var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion).OrderByDescending(b => b.VersionId).FirstOrDefault();
            response.QRFID = QRFPrice.QRFID;
            response.QRFPriceId = QRFPrice.QRFPrice_Id;
            response.PercentSoldOptional = QRFPrice.PercentSoldOptional;

            var PositionsList = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id && a.Departure_Id == request.DepartureId && a.PaxSlab_Id == request.PaxSlabId).ToList();
            _genericRepository.getExchangeRateForPosition(ref PositionsList);

            #region Bare Bone data
            double vCostPrice = 0;
            double vSellPrice = 0;
            double vProfit = 0;
            double vProfitPercent = 0;
            string vCostPriceCurrency = "";
            string vSellPriceCurrency = "";

            var PositionForHotel = PositionsList.Where(a => a.PositionType.ToLower() == "hotel" || a.PositionType.ToLower() == "overnight ferry").ToList();
            vCostPrice = PositionForHotel.Sum(a => a.TotalBuyPrice);
            vSellPrice = PositionForHotel.Sum(a => a.TotalSellPrice);
            vProfit = PositionForHotel.Sum(a => a.ProfitAmount);
            vProfitPercent = PositionForHotel.Sum(a => a.ProfitPercentage) / PositionForHotel.Count;
            vCostPriceCurrency = PositionForHotel.Count > 0 ? PositionForHotel[0].BuyCurrency : null;
            vSellPriceCurrency = PositionForHotel.Count > 0 ? PositionForHotel[0].QRFCurrency : null;

            response.BareBoneList.Add(new CommercialsData
            {
                ProductType = "Hotel",
                CostPrice = vCostPrice,
                SellPrice = vSellPrice,
                ProfitLoss = vProfit,
                ProfitPercent = vProfitPercent,
                BuyCurrency = vCostPriceCurrency,
                SellCurrency = vSellPriceCurrency
            });

            var PositionForTransportation = PositionsList.Where(a => a.PositionType.ToLower() == "ldc" || a.PositionType.ToLower() == "coach" || a.PositionType.ToLower() == "private transfer"
            || a.PositionType.ToLower() == "scheduled transfer" || a.PositionType.ToLower() == "ferry passenger" || a.PositionType.ToLower() == "ferry transfer" || a.PositionType.ToLower() == "train").ToList();
            vCostPrice = PositionForTransportation.Sum(a => a.TotalBuyPrice);
            vSellPrice = PositionForTransportation.Sum(a => a.TotalSellPrice);
            vProfit = PositionForTransportation.Sum(a => a.ProfitAmount);
            vProfitPercent = PositionForTransportation.Sum(a => a.ProfitPercentage) / PositionForTransportation.Count;
            vCostPriceCurrency = PositionForTransportation.Count > 0 ? PositionForTransportation[0].BuyCurrency : null;
            vSellPriceCurrency = PositionForTransportation.Count > 0 ? PositionForTransportation[0].QRFCurrency : null;

            response.BareBoneList.Add(new CommercialsData
            {
                ProductType = "Transportation",
                CostPrice = vCostPrice,
                SellPrice = vSellPrice,
                ProfitLoss = vProfit,
                ProfitPercent = vProfitPercent,
                BuyCurrency = vCostPriceCurrency,
                SellCurrency = vSellPriceCurrency
            });

            var PositionForActivities = PositionsList.Where(a => a.PositionType.ToLower() == "attractions" || a.PositionType.ToLower() == "sightseeing - citytour").ToList();
            vCostPrice = PositionForActivities.Sum(a => a.TotalBuyPrice);
            vSellPrice = PositionForActivities.Sum(a => a.TotalSellPrice);
            vProfit = PositionForActivities.Sum(a => a.ProfitAmount);
            vProfitPercent = PositionForActivities.Sum(a => a.ProfitPercentage) / PositionForActivities.Count;
            vCostPriceCurrency = PositionForActivities.Count > 0 ? PositionForActivities[0].BuyCurrency : null;
            vSellPriceCurrency = PositionForActivities.Count > 0 ? PositionForActivities[0].QRFCurrency : null;

            response.BareBoneList.Add(new CommercialsData
            {
                ProductType = "Activities",
                CostPrice = vCostPrice,
                SellPrice = vSellPrice,
                ProfitLoss = vProfit,
                ProfitPercent = vProfitPercent,
                BuyCurrency = vCostPriceCurrency,
                SellCurrency = vSellPriceCurrency
            });

            var PositionForMeal = PositionsList.Where(a => a.PositionType.ToLower() == "meal").ToList();
            vCostPrice = PositionForMeal.Sum(a => a.TotalBuyPrice);
            vSellPrice = PositionForMeal.Sum(a => a.TotalSellPrice);
            vProfit = PositionForMeal.Sum(a => a.ProfitAmount);
            vProfitPercent = PositionForMeal.Sum(a => a.ProfitPercentage) / PositionForMeal.Count;
            vCostPriceCurrency = PositionForMeal.Count > 0 ? PositionForMeal[0].BuyCurrency : null;
            vSellPriceCurrency = PositionForMeal.Count > 0 ? PositionForMeal[0].QRFCurrency : null;

            response.BareBoneList.Add(new CommercialsData
            {
                ProductType = "Meals",
                CostPrice = vCostPrice,
                SellPrice = vSellPrice,
                ProfitLoss = vProfit,
                ProfitPercent = vProfitPercent,
                BuyCurrency = vCostPriceCurrency,
                SellCurrency = vSellPriceCurrency
            });

            var PositionForOthers = PositionsList.Where(a => !(a.PositionType.ToLower() == "ldc" || a.PositionType.ToLower() == "coach" || a.PositionType.ToLower() == "private transfer"
            || a.PositionType.ToLower() == "scheduled transfer" || a.PositionType.ToLower() == "ferry passenger" || a.PositionType.ToLower() == "ferry transfer" || a.PositionType.ToLower() == "train"
            || a.PositionType.ToLower() == "hotel" || a.PositionType.ToLower() == "overnight ferry" || a.PositionType.ToLower() == "attractions" || a.PositionType.ToLower() == "sightseeing - citytour" || a.PositionType.ToLower() == "meal")).ToList();
            vCostPrice = PositionForOthers.Sum(a => a.TotalBuyPrice);
            vSellPrice = PositionForOthers.Sum(a => a.TotalSellPrice);
            vProfit = PositionForOthers.Sum(a => a.ProfitAmount);
            vProfitPercent = PositionForOthers.Sum(a => a.ProfitPercentage) / PositionForOthers.Count;
            vCostPriceCurrency = PositionForOthers.Count > 0 ? PositionForOthers[0].BuyCurrency : null;
            vSellPriceCurrency = PositionForOthers.Count > 0 ? PositionForOthers[0].QRFCurrency : null;

            response.BareBoneList.Add(new CommercialsData
            {
                ProductType = "Others",
                CostPrice = vCostPrice,
                SellPrice = vSellPrice,
                ProfitLoss = vProfit,
                ProfitPercent = vProfitPercent,
                BuyCurrency = vCostPriceCurrency,
                SellCurrency = vSellPriceCurrency
            });
            #endregion

            response.PositionIncluded = PositionsList.Where(a => a.PositionKeepAs.ToUpper() == "INCLUDED").OrderBy(b => b.PositionType).ThenBy(c => c.ProductName).ToList();
            response.PositionSupplement = PositionsList.Where(a => a.PositionKeepAs.ToUpper() == "SUPPLEMENT").OrderBy(b => b.ProductName).ToList();
            response.PositionOptional = PositionsList.Where(a => a.PositionKeepAs.ToUpper() == "OPTIONAL").OrderBy(b => b.ProductName).ToList();

            response.QRFExhangeRates = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id).Select(b => b.QRFExchangeRates).FirstOrDefault();

            return response;
        }

        public async Task<CommonResponse> ChangePositionKeepAs(ChangePositionKeepReq request)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                mGuesstimate guesstimate;
                guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();

                guesstimate.EditUser = request.EditUser;
                guesstimate.EditDate = DateTime.Now;

                foreach (var pos in guesstimate.GuesstimatePosition)
                {
                    if (request.PositionIds.Contains(pos.PositionId))
                    {
                        pos.KeepAs = request.ChangeType;
                    }
                }

                ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimate.GuesstimateId), guesstimate);

                response.Id = await _qRFSummaryRepository.SaveQRFPrice("Commercial", "Position Keep As changed", request.QRFID, request.EditUser);

                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<CommonResponse> SaveCommercials(CommercialsSetReq request)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFPrice_Id == request.QRFPriceId).FirstOrDefault();

                QRFPrice.PercentSoldOptional = request.PercentSoldOptional;
                QRFPrice.EditUser = request.EditUser;
                QRFPrice.EditDate = DateTime.Now;

                ReplaceOneResult replaceResultNew = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", QRFPrice.QRFPrice_Id), QRFPrice);
                response.ResponseStatus.Status = replaceResultNew.MatchedCount > 0 ? "Success" : "Failure";
                response.ResponseStatus.ErrorMessage = replaceResultNew.MatchedCount > 0 ? "Commercial data Successfully." : "Commercial data not updated.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<string> SetQuoteDetails(QuoteSetReq request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    if (!string.IsNullOrEmpty(request.EnquiryPipeline))
                    {
                        bool flag = false;
                        if (request.EnquiryPipeline.ToLower() == "costing pipeline")
                        {
                            if (request.IsApproveQuote)
                            {
                                await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                   Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                   Builders<mQuote>.Update.
                                                   Set("CurrentPipeline", "Costing Approval Pipeline").
                                                   Set("CurrentPipelineStep", "Costing").
                                                   Set("Remarks", request.Remarks).
                                                   Set("CurrentPipelineSubStep", "Itinerary").
                                                   Set("QuoteResult", "Success").
                                                   Set("Status", "NewCostingApprovalPipeline").
                                                   Set("EditUser", request.PlacerEmail).
                                                   Set("EditDate", DateTime.Now)
                                                   );

                                await _MongoContext.mQRFPrice.UpdateManyAsync(
                                              Builders<mQRFPrice>.Filter.Eq("QRFID", request.QRFID),
                                              Builders<mQRFPrice>.Update.
                                              Set("ProductAccountant", request.CostingOfficer).
                                              Set("EditUser", request.PlacerEmail).
                                              Set("EditDate", DateTime.Now)
                                              );

                                #region Add Followup 
                                request.PlacerEmail = request.PlacerEmail.ToLower().Trim();
                                request.CostingOfficer = request.CostingOfficer.Trim().ToLower();
                               var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.PlacerEmail)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.CostingOfficer)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == request.CostingOfficer).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Costing Approval Requested";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.PlacerEmail;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = request.CostingOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Pending for Approval";
                                task.Notes = "Costing Approval Requested";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion
                            }
                            else
                            {
                                #region Add Followup 
                                request.PlacerEmail = request.PlacerEmail.ToLower().Trim();
                                var SalesOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.SalesOfficer?.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.PlacerEmail)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == SalesOfficer)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == SalesOfficer).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "QRF Change Suggested";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.PlacerEmail;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = SalesOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Draft";
                                task.Notes = "QRF Change Suggested";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion

                                await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                   Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                   Builders<mQuote>.Update.
                                                   Set("CurrentPipeline", "Quote Pipeline").
                                                   Set("CurrentPipelineStep", "Quote").
                                                   Set("Remarks", request.Remarks).
                                                   Set("CurrentPipelineSubStep", "AgentInformation").
                                                   Set("QuoteResult", "Success").
                                                   Set("Status", "NewQuote").
                                                   Set("EditUser", request.PlacerEmail).
                                                   Set("EditDate", DateTime.Now)
                                                   );
                              
                                flag = await SetDataFromCostingToSales(request.QRFID, request.PlacerEmail, request.PlacerUser);
                            }
                        }
                        else if (request.EnquiryPipeline.ToLower() == "amendment pipeline")
                        {
                            await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                     Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                     Builders<mQuote>.Update.
                                                     Set("CurrentPipeline", "Costing Approval Pipeline").
                                                     Set("CurrentPipelineStep", "Costing").
                                                     Set("Remarks", request.Remarks).
                                                     Set("CurrentPipelineSubStep", "Itinerary").
                                                     Set("QuoteResult", "Success").
                                                     Set("Status", "NewCostingApprovalPipeline").
                                                     Set("EditUser", request.PlacerEmail).
                                                     Set("EditDate", DateTime.Now)
                                                     );

                            await _MongoContext.mQRFPrice.UpdateManyAsync(
                                              Builders<mQRFPrice>.Filter.Eq("QRFID", request.QRFID),
                                              Builders<mQRFPrice>.Update.
                                              Set("ProductAccountant", request.CostingOfficer).
                                              Set("EditUser", request.PlacerEmail).
                                              Set("EditDate", DateTime.Now)
                                              );
                        }
                        else if (request.EnquiryPipeline.ToLower() == "costing approval pipeline")
                        {
                            if (request.IsApproveQuote)
                            {
                                await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                       Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                       Builders<mQuote>.Update.
                                                       Set("CurrentPipeline", "Agent Approval Pipeline").
                                                       Set("CurrentPipelineStep", "").
                                                       Set("Remarks", request.Remarks).
                                                       Set("CurrentPipelineSubStep", "").
                                                       Set("QuoteResult", "Success").
                                                       Set("Status", "NewAgentApprovalPipeline").
                                                       Set("EditUser", request.PlacerEmail).
                                                       Set("EditDate", DateTime.Now)
                                                       );

                                #region Add Followup 
                                request.PlacerEmail = request.PlacerEmail.ToLower().Trim();
                                var SalesOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.SalesOfficer?.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.PlacerEmail)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == SalesOfficer)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == SalesOfficer).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Costing Approved";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.PlacerEmail;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = SalesOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Replied";
                                task.Notes = "Costing Approved";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion
                            }
                            else
                            {
                                await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                       Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                       Builders<mQuote>.Update.
                                                       Set("CurrentPipeline", "Costing Pipeline").
                                                       Set("CurrentPipelineStep", "Itinerary").
                                                       Set("Remarks", request.Remarks).
                                                       Set("CurrentPipelineSubStep", "").
                                                       Set("QuoteResult", "Reject").
                                                       Set("Status", "NewCostingPipeline").
                                                       Set("EditUser", request.PlacerEmail).
                                                       Set("EditDate", DateTime.Now)
                                                       );

                                #region Add Followup 
                                var CostingOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.CostingOfficer?.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.PlacerEmail)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower()== request.PlacerEmail.ToLower()).FirstOrDefault();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == CostingOfficer)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == CostingOfficer).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Costing Changes Suggested";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.PlacerEmail;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = CostingOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Request";
                                task.Notes = "Costing Changes Suggested";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion
                            }
                        }

                        #region Send Email
                        string doctype = "";
                        if (request.EnquiryPipeline.ToLower() == "costing pipeline")
                        {
                            if (request.IsApproveQuote)
                                doctype = DocType.COAPPROVAL;
                            else
                                doctype = DocType.COREJECT;
                        }
                        else if (request.EnquiryPipeline.ToLower() == "amendment pipeline")
                        {
                            doctype = DocType.COAPPROVAL;
                        }
                        else if (request.EnquiryPipeline.ToLower() == "costing approval pipeline")
                        {
                            if (request.IsApproveQuote)
                                doctype = DocType.CAPAPPROVAL;
                            else
                                doctype = DocType.CAPREJECT;
                        }

                        var objEmailGetReq = new EmailGetReq()
                        {
                            UserName = request.PlacerUser,
                            UserEmail = request.PlacerEmail,
                            PlacerUserId = request.PlacerUserId,
                            QrfId = request.QRFID,
                            Remarks = request.Remarks,
                            DocumentType = doctype,
                            EnquiryPipeline = request.EnquiryPipeline,
                            IsApproveQuote = request.IsApproveQuote
                        };
                        var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                        if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                        {
                            responseStatusMail.ResponseStatus = new ResponseStatus();
                            responseStatusMail.ResponseStatus.Status = "Error";
                            responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                        }
                        #endregion

                        return request.QRFID;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (MongoWriteException)
            {
                return null;
            }
        }

        public async Task<bool> SetDataFromCostingToSales(string QRFID, string COEmail, string COUserName)
        {
            try
            {
                if (!string.IsNullOrEmpty(QRFID))
                {
                    //Getting Quote Data of Positions and Prices
                    var quote = _MongoContext.mQuote.AsQueryable().Where(x => x.QRFID == QRFID).FirstOrDefault();
                    var positionprices = _MongoContext.mPositionPrice.AsQueryable().Where(x => x.QRFID == QRFID).ToList();
                    var positions = _MongoContext.mPosition.AsQueryable().Where(x => x.QRFID == QRFID).ToList();

                    //Getting costing data of Positions and Prices
                    var qrfprice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == QRFID && x.IsCurrentVersion == true).FirstOrDefault();
                    var guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(x => x.QRFID == QRFID && x.IsCurrentVersion == true).FirstOrDefault();

					//Updating KeepAs of Positions
					foreach (var g in guesstimate.GuesstimatePosition)
					{
						foreach (var pos in positions)
						{
							if (pos.PositionId == g.PositionId)
							{
								pos.KeepAs = g.KeepAs;
							
								await _MongoContext.mPosition.FindOneAndUpdateAsync(Builders<mPosition>.Filter.Eq("PositionId", pos.PositionId),
								Builders<mPosition>.Update.Set("KeepAs", pos.KeepAs).Set("EditUser", COEmail).Set("EditDate", DateTime.Now));
							}
						}
					}

					//Update Budgetprice of PositionPrice
					foreach (var g in guesstimate.GuesstimatePosition)
					{
						var prices = g.GuesstimatePrice.Where(x => x.SupplierId == g.ActiveSupplierId).ToList();

						await _MongoContext.mPosition.FindOneAndUpdateAsync(Builders<mPosition>.Filter.Eq("PositionId", g.PositionId),
								Builders<mPosition>.Update.Set("SupplierId", g.ActiveSupplierId)
								.Set("SupplierName", g.ActiveSupplier).Set("EditUser", COEmail).Set("EditDate", DateTime.Now));


						foreach (var price in prices)
						{
							foreach (var posprice in positionprices)
							{
								if (price.PositionPriceId == posprice.PositionPriceId)
								{
									posprice.BudgetPrice = price.BudgetPrice;
									posprice.SupplierId = g.ActiveSupplierId;
									posprice.Supplier = g.ActiveSupplier;

                                    await _MongoContext.mPositionPrice.FindOneAndUpdateAsync(Builders<mPositionPrice>.Filter.Eq("PositionPriceId", posprice.PositionPriceId),
                                    Builders<mPositionPrice>.Update.Set("BudgetPrice", posprice.BudgetPrice).Set("SupplierId", posprice.SupplierId)
                                    .Set("Supplier", posprice.Supplier).Set("EditUser", COEmail).Set("EditDate", DateTime.Now));
                                }
                            }
                        }
                    }

                    //Update Margins of Quote
                    //Package
                    //1. Package Properties
                    List<PackageProperties> lstPkg = new List<PackageProperties>();
                    foreach (var pprop in qrfprice.QRFMargin.Package.PackageProperties)
                    {
                        PackageProperties qprop = new PackageProperties();
                        qprop.ComponentName = pprop.ComponentName;
                        qprop.SellingPrice = pprop.SellingPrice;
                        qprop.MarginUnit = pprop.MarginUnit;
                        lstPkg.Add(qprop);
                    }
                    quote.Margins.Package = new Package();
                    quote.Margins.Package.PackageProperties = lstPkg;

                    //2. Margin Computed
                    quote.Margins.Package.MarginComputed.TotalCost = qrfprice.QRFMargin.Package.MarginComputed.TotalCost;
                    quote.Margins.Package.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Package.MarginComputed.TotalLeadersCost;
                    quote.Margins.Package.MarginComputed.Upgrade = qrfprice.QRFMargin.Package.MarginComputed.Upgrade;
                    quote.Margins.Package.MarginComputed.MarkupType = qrfprice.QRFMargin.Package.MarginComputed.MarkupType;

					//Product
					//1. Product Properties
					List<ProductProperties> lstProd = new List<ProductProperties>();
					foreach (var pprop in qrfprice.QRFMargin.Product.ProductProperties)
					{
						ProductProperties qprop = new ProductProperties();
						qprop.VoyagerProductType_Id = pprop.VoyagerProductType_Id;
						qprop.Prodtype = pprop.Prodtype;
						qprop.SellingPrice = pprop.SellingPrice;
						qprop.MarginUnit = pprop.MarginUnit;
						qprop.HowMany = pprop.HowMany;
						lstProd.Add(qprop);
					}
					quote.Margins.Product = new Product();
					quote.Margins.Product.ProductProperties = lstProd;

                    //2. Margin Computed
                    quote.Margins.Product.MarginComputed.TotalCost = qrfprice.QRFMargin.Product.MarginComputed.TotalCost;
                    quote.Margins.Product.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Product.MarginComputed.TotalLeadersCost;
                    quote.Margins.Product.MarginComputed.Upgrade = qrfprice.QRFMargin.Product.MarginComputed.Upgrade;
                    quote.Margins.Product.MarginComputed.MarkupType = qrfprice.QRFMargin.Product.MarginComputed.MarkupType;

					//Item
					//1. Item Properties
					List<ItemProperties> lstItem = new List<ItemProperties>();
					foreach (var pprop in qrfprice.QRFMargin.Item.ItemProperties)
					{
						ItemProperties qprop = new ItemProperties();
						qprop.PositionID = pprop.PositionID;
						qprop.ProductName = pprop.ProductName;
						qprop.VoyagerProductType_Id = pprop.VoyagerProductType_Id;
						qprop.Prodtype = pprop.Prodtype;
						qprop.SellingPrice = pprop.SellingPrice;
						qprop.MarginUnit = pprop.MarginUnit;
						qprop.HowMany = pprop.HowMany;
						lstItem.Add(qprop);
					}
					quote.Margins.Itemwise = new Itemwise();
					quote.Margins.Itemwise.ItemProperties = lstItem;

                    //2. Margin Computed
                    quote.Margins.Itemwise.MarginComputed.TotalCost = qrfprice.QRFMargin.Item.MarginComputed.TotalCost;
                    quote.Margins.Itemwise.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Item.MarginComputed.TotalLeadersCost;
                    quote.Margins.Itemwise.MarginComputed.Upgrade = qrfprice.QRFMargin.Item.MarginComputed.Upgrade;
                    quote.Margins.Itemwise.MarginComputed.MarkupType = qrfprice.QRFMargin.Item.MarginComputed.MarkupType;

                    await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", quote.QRFID),
                                    Builders<mQuote>.Update.Set("Margins", quote.Margins).Set("FollowUp", qrfprice.FollowUp).Set("EditUser", COEmail).Set("EditDate", DateTime.Now));

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
