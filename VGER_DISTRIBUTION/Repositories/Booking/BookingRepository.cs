using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Helpers;
using VGER_DISTRIBUTION.Models;
using VGER_DISTRIBUTION.Repositories;
using VGER_WAPI_CLASSES;
using VGER_Communicator;

namespace VGER_DISTRIBUTION.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly CommonFunction common = null;
        #endregion

        public BookingRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
            common = new CommonFunction(_MongoContext);
        }

        public async Task<BookingDetails> GetBookingDetail(BookingDetailReq request, UserCookieDetail userdetails)
        {
            common.GetUserCompanyType(ref userdetails);
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)

            BookingDetails booking = new BookingDetails();
            FilterDefinition<mBookings> filter;
            FilterDefinition<mBookingPax> filterPax;
            FilterDefinition<mBookingRooms> filterRooms;
            FilterDefinition<mBookingPositions> filterPositions;
            FilterDefinition<mBookingPositionPricing> filterPositionPricing;
            FilterDefinition<mFOCDilution> filterFOC;
            FilterDefinition<mBookingItineraryDetail> filterItin;
            filter = Builders<mBookings>.Filter.Empty;
            filterPax = Builders<mBookingPax>.Filter.Empty;
            filterRooms = Builders<mBookingRooms>.Filter.Empty;
            filterPositions = Builders<mBookingPositions>.Filter.Empty;
            filterItin = Builders<mBookingItineraryDetail>.Filter.Empty;
            bool OnAndAfter = false;
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime dateValue;

            if(!string.IsNullOrWhiteSpace(request.BookingReference))
            {
                filter = filter & Builders<mBookings>.Filter.Where(f => request.BookingReference == f.BookingNumber);
                //filter = filter & Builders<mBookings>.Filter.Where(f => f.ISEHSupp == true);

                var result = await _MongoContext.mBookings.Find(filter).Project(p => new BookingDetails
                {
                    AgentCode = p.AgentCode,
                    AgentName = p.AgentName,
                    AgentReference = p.CUSTREF,
                    EndDate = (p.ARRIVALDATE == null) ? null : Convert.ToDateTime(p.ARRIVALDATE.ToString()).ToString("yyyy-MM-dd") ,
                    BookedDate = (p.CreaDT == null) ? null : Convert.ToDateTime(p.CreaDT.ToString()).ToString("yyyy-MM-dd"),
                    StartDate = (p.DEPARTUREDATE == null) ? null : Convert.ToDateTime(p.DEPARTUREDATE.ToString()).ToString("yyyy-MM-dd"),
                    UpdateDate = (p.MODIDT == null) ? null : Convert.ToDateTime(p.MODIDT.ToString()).ToString("yyyy-MM-dd"),
                    OPTIONDATE = (p.OPTIONDATE == null) ? null : Convert.ToDateTime(p.OPTIONDATE.ToString()).ToString("yyyy-MM-dd"),
                    BookingReference = p.BookingNumber,
                    Duration = Convert.ToString(p.Duration),
                    Status = p.StatusDesc,
                    Operator = p.Operator,
                    OperatorEmail = p.OperatorEmail,
                    TourLeader = p.TourLeader ?? "",
                    TourLeaderContact = p.TourLeader_Contact ?? "",
                    InvoiceCurrency = p.Currency,
                    GoAheadDate = Convert.ToDateTime(p.BookingGoAheadDT.ToString()).ToString("yyyy-MM-dd")
                }).FirstOrDefaultAsync();



                if (result != null)
                {
                    string Booking_Id = (await _MongoContext.mBookings.Find(filter).Project(p => p.BookingId).FirstOrDefaultAsync());
                    string AgentId = (await _MongoContext.mBookings.Find(filter).Project(p => p.AgentId).FirstOrDefaultAsync());
                    string Suppliers = (await _MongoContext.mBookings.Find(filter).Project(p => p.Suppliers).FirstOrDefaultAsync());

                    if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
                    {
                        if ((userdetails.IsAgent ?? false))
                        {
                            if(userdetails.Company_Id.Trim().ToLower() != AgentId.Trim().ToLower())
                            {
                                booking = new BookingDetails();
                                return booking;
                            }
                        }
                        else if ((userdetails.IsSupplier ?? false))
                        {
                            if(!Suppliers.Contains("," + userdetails.Company_Id + ","))
                            {
                                booking = new BookingDetails();
                                return booking;
                            }
                        }
                            
                    }

                    Booking_Id = Booking_Id.ToLower();
                    if (!string.IsNullOrWhiteSpace(Booking_Id))
                    //if (Booking_Id != null && Booking_Id != Guid.Empty)
                    {


                        filterItin = filterItin & Builders<mBookingItineraryDetail>.Filter.Where(f => request.BookingReference == f.BookingNumber);
                        var bItin = await _MongoContext.mBookingItineraryDetail.Find(filterItin).Project(p => new BookingItineraryDetail
                        {
                            City = p.CityName,
                            City_Id = p.City_Id,
                            Country = p.CountryName,
                            Country_Id = p.Country_Id,
                            Date = (p.STARTDATE == null) ? null : Convert.ToDateTime(p.STARTDATE.ToString()).ToString("yyyy-MM-dd"),
                            Name = p.Description,
                            Position_Id = p.Position_Id ?? "",
                            Remarks = p.Details,
                            Time = p.STARTTIME,
                            Type = p.ProductType
                        }).ToListAsync();

                        filterPax = filterPax & Builders<mBookingPax>.Filter.Where(f => (f.Booking_Id) == Booking_Id && (f.Status == null || (f.Status != "X" && f.Status != "-"))  );
                        var bpax = await _MongoContext.mBookingPax.Find(filterPax).Project(p => new BookingPax
                        {
                            Pax_Id = p.BookingPax_Id,
                            Type = p.PersonType,
                            Count = p.PERSONS ?? "0",
                            Age = p.AGE ?? "N/A",
                            ClientName = p.ClientName,
                            StartDate = (p.STARTDATE == null) ? null : Convert.ToDateTime(p.STARTDATE.ToString()).ToString("yyyy-MM-dd"),
                            EndDate = (p.ENDDATE == null) ? null : Convert.ToDateTime(p.ENDDATE.ToString()).ToString("yyyy-MM-dd")
                        }).ToListAsync();

                        filterRooms = filterRooms & Builders<mBookingRooms>.Filter.Where(f => (f.Booking_Id) == Booking_Id && (f.Position_Id == null || f.Position_Id == "")  && (f.Status == null || (f.Status != "X" && f.Status != "-")));
                        var broom = await _MongoContext.mBookingRooms.Find(filterRooms).Project(p => new BookingRooms
                        {
                            Room_Id = p.BookingRooms_ID,
                            Type = (p.ProductTemplate == null) ? "" : p.ProductTemplate.Trim(),
                            For = (p.PersonType == null) ? "Passenger" : p.PersonType.Trim() ,
                            Count = p.ROOMNO ?? 0
                        }).ToListAsync();

                        filterPositions = filterPositions & Builders<mBookingPositions>.Filter.Where(f => (f.Booking) == request.BookingReference && (f.STATUS == null || (f.STATUS != "X" && f.STATUS != "-" && f.STATUS != "J" && f.STATUS != "C")));
                        var bpos = await _MongoContext.mBookingPositions.Find(filterPositions).Project(p => new BookingPositions
                        {
                            City = p.City.Trim(),
                            Country = p.Country.Trim(),
                            
                            DriverContact = p.DriverContactNumber,
                            DriverName = p.DriverName ?? "",
                            DropOffPoint = p.ENDLOC,
                            Duration = p.DURATION,
                            EmptyLeg = (p.EmptyLegs == null) ? "" : p.EmptyLegs.Trim(),
                            EndDate = (p.ENDDATE == null) ? "" : Convert.ToDateTime(p.ENDDATE.ToString()).ToString("yyyy-MM-dd"),
                            EndTime = p.ENDTIME,
                            No = p.ORDERNR?? 0,
                            PickupPoint = p.STARTLOC,
                            ProductCode = p.ProductCode,
                            ProductName = p.Product,
                            StartDate = (p.STARTDATE == null) ? "" : Convert.ToDateTime(p.STARTDATE.ToString()).ToString("yyyy-MM-dd"),
                            StartTime = p.STARTTIME,
                            Status = p.SatusDesc,
                            Supplier = p.Supplier,
                            SupplierEmail = p.SENDADDR,
                            SupplierNote = p.PROPMEMO,
                            SupplierPhone = p.SupplierTel,
                            MealPlan = p.MealPlan,
                            SupplierConfirmationNumber = p.SUPPCONFNR,
                            ProductType = p.ProductType,                            
                            Advice = p.HotelAdvice,
                            CancellationDeadline = (p.CancelDeadline == null) ? "" : Convert.ToDateTime(p.CancelDeadline.ToString()).ToString("yyyy-MM-dd"),
                            CancellationPolicy = p.CancellationPolicy,
                            EndSupplier = p.EndSupplier ?? "",
                            LicencePlate = p.LicencePlate ?? "",
                            OptionDate = (p.OPTIONDATE == null) ? "" : Convert.ToDateTime(p.OPTIONDATE.ToString()).ToString("yyyy-MM-dd"),
                            Priority = p.Priority ?? null,
                            StandardPax =  (p.AUTOSPOS == null) ? null : p.AUTOSPOS.ToString(),
                            Type = p.PositionType,
                            Position_Id = Convert.ToString(p.Position_Id),
                            Country_Id = Convert.ToString(p.ParentResort_Id),
                            City_Id = Convert.ToString(p.Resort_Id),
                        }).ToListAsync();

                        foreach (BookingPositions pos in bpos)
                        {
                            filterPositionPricing = Builders<mBookingPositionPricing>.Filter.Empty;
                            filterPositionPricing = filterPositionPricing & Builders<mBookingPositionPricing>.Filter.Where(f => f.BookingNumber == request.BookingReference);

                            filterPositionPricing = filterPositionPricing & Builders<mBookingPositionPricing>.Filter.Where(f => f.PositionNumber == pos.No);

                            var ppos = await _MongoContext.mBookingPositionPricing.Find(filterPositionPricing).Project(p => new BookingPositionPricing
                            {
                                Age = (p.AGE == null) ? null : (p.AGE).ToString(),
                                BudgetPrice = (p.BudgetPrice == null) ? null : (p.BudgetPrice).ToString(),
                                Category = p.Category,
                                ChargeBasis = p.PersonType,
                                Currency = p.BPRICECUR,
                                EndDate = (p.EndDate == null) ? null : Convert.ToDateTime(p.EndDate.ToString()).ToString("yyyy-MM-dd"),
                                OneOff = "false",
                                PBR_Id = p.BookingRooms_Id,
                                PP_Id = p.PositionPricing_Id,
                                PriceConfirmed = (p.ConfirmedReqPrice == null) ? null : (p.ConfirmedReqPrice).ToString(),
                                PurchasePrice = (p.BPRICE == null) ? null : (p.BPRICE).ToString(),
                                Quantity = (p.Quantity == null) ? null : (p.Quantity).ToString(),
                                Service = (p.ProductTemplate == null) ? null : Convert.ToString(p.ProductTemplate),
                                StartDate = (p.StartDate == null) ? null : Convert.ToDateTime(p.StartDate.ToString()).ToString("yyyy-MM-dd"),
                            }).ToListAsync();

                            if (ppos != null)
                            {
                                if (ppos.Count > 0)
                                {
                                    foreach (BookingPositionPricing pp in ppos)
                                    {
                                        filterFOC = Builders<mFOCDilution>.Filter.Empty;
                                        filterFOC = filterFOC & Builders<mFOCDilution>.Filter.Where(f => (f.PositionNumber) == pos.No && f.BookingNumber == request.BookingReference && f.ProductTemplate == pp.Service);

                                        var foc = await _MongoContext.mFOCDilution.Find(filterFOC).Project(p => new SupplierFOC
                                        {
                                            FoCBuy = (p.TotalUnits == null) ? null : (p.TotalUnits).ToString(),
                                            FoCGet = (p.FreeUnits == null) ? null : (p.FreeUnits).ToString()
                                        }).ToListAsync();

                                        if (foc != null)
                                        {
                                            if (foc.Count > 0)
                                            {
                                                pp.FOC = foc;
                                            }
                                        }
                                    }

                                   

                                        pos.Purchasing = ppos;
                                    pos.Purchasing = pos.Purchasing.OrderBy(c => c.StartDate).ThenBy(c => c.ChargeBasis).ToList();

                                }
                            }
                        }


                        if (bpax != null)
                        {
                            if (bpax.Count > 0)
                            {
                                result.BookingPassengers = bpax;
                            }
                        }
                        if (broom != null)
                        {
                            if (broom.Count > 0)
                            {
                                result.BookingRooms = broom;
                            }
                        }
                        if (bpos != null)
                        {
                            if (bpos.Count > 0)
                            {
                                result.Services = bpos;
                                result.Services = result.Services.OrderBy(c => c.StartDate).ThenBy(c => c.StartTime).ToList();
                            }
                        }
                        if (bItin != null)
                        {
                            if (bItin.Count > 0)
                            {
                                result.Itinerary = bItin;
                                result.Itinerary = result.Itinerary.OrderBy(c => c.Date).ThenBy(c => c.Time).ToList();
                            }
                        }
                    }
                    booking = result;
                }

            }

            /*var result = await _MongoContext.mBookings.Find(filter).Project(p => new BookingDetails
            {
                AgentCode = p.AgentCode,
                AgentName = p.AgentName,
                AgentReference = p.CUSTREF,
                EndDate = (p.EndDate == null) ? "" : p.EndDate.ToString("yyyy-MM-dd"),
                BookedDate = Convert.ToDateTime(p.CreaDT.ToString()).ToString("yyyy-MM-dd"),
                StartDate = (p.StartDate == null) ? "" : (p.StartDate.ToString("yyyy-MM-dd")),
                UpdateDate = Convert.ToDateTime(p.MODIDT.ToString()).ToString("yyyy-MM-dd"),
                BookingReference = p.BookingNumber,
                Duration = Convert.ToString(p.Duration),
                Status = p.StatusDesc,

                //BookingPassengers = (_MongoContext.mBookingPax.Find(filterPax & Builders<mBookingPax>.Filter.Where(f => p.BookingId == f.Booking_Id)))
                //.Project(p => new BookingPax
                //{
                //    Count = 1
                //}).ToListAsync()),

            }).ToListAsync();*/

            return booking;
        }

        public async Task<BookingSearchRes> GetBookings(BookingSearchReq request, UserCookieDetail userdetails)
        {
            common.GetUserCompanyType(ref userdetails);
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)

            FilterDefinition<mBookings> filter;
            filter = Builders<mBookings>.Filter.Empty;
            bool OnAndAfter = false;
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime dateValue;

            if (request.OnAndAfter)
                OnAndAfter = true;

            //filter = filter & Builders<mBookings>.Filter.Where(f => f.ISEHSupp == true);
            if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
            {
                if ((userdetails.IsAgent ?? false))
                {
                    filter = filter & Builders<mBookings>.Filter.Regex(x => x.AgentId, new BsonRegularExpression(new Regex(userdetails.Company_Id.Trim(), RegexOptions.IgnoreCase)));
                }
                else if ((userdetails.IsSupplier ?? false))
                    filter = filter & Builders<mBookings>.Filter.Where(f => f.Suppliers.Contains("," + userdetails.Company_Id + ","));
            }
            if (request.AgentCode != null)
            {
                //filter = filter & Builders<mBookings>.Filter.Where(f => request.AgentCode == f.AgentCode);
                filter = filter & Builders<mBookings>.Filter.Regex(x => x.AgentCode, new BsonRegularExpression(new Regex(request.AgentCode.Trim(), RegexOptions.IgnoreCase)));

            }
            if (request.AgentReference != null)
            {
                filter = filter & Builders<mBookings>.Filter.Where(f => f.CUSTREF.Contains(request.AgentReference));
            }
            if (request.Status != null)
            {
                filter = filter & Builders<mBookings>.Filter.Where(f => f.StatusDesc.Contains(request.Status));
                //filter = filter & Builders<mBookings>.Filter.Regex(x => x.StatusDesc.Contains(request.Status), new BsonRegularExpression(new Regex(request.Status.Trim(), RegexOptions.IgnoreCase)));
            }
            if (request.StartDate != null)
            {
                if (DateTime.TryParseExact(request.StartDate, "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out dateValue))
                {
                    if (OnAndAfter || request.EndDate != null)
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.DEPARTUREDATE >= Convert.ToDateTime(request.StartDate));
                    else
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.DEPARTUREDATE == Convert.ToDateTime(request.StartDate));
                }
                else
                    throw new System.ArgumentException(string.Format("StartDate must be in yyyy-MM-dd date format ", "StartDate"));
            }
            if (request.EndDate != null)
            {
                if (DateTime.TryParseExact(request.EndDate, "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out dateValue))
                {
                    
                    if (request.StartDate != null)
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.DEPARTUREDATE <= Convert.ToDateTime(request.EndDate));
                    else if (OnAndAfter)
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.ARRIVALDATE >= Convert.ToDateTime(request.EndDate));
                    else
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.ARRIVALDATE == Convert.ToDateTime(request.EndDate));
                }
                else
                    throw new System.ArgumentException(string.Format("EndDate must be in yyyy-MM-dd date format ", "EndDate"));
            }
            if (request.UpdateDate != null)
            {
                if (DateTime.TryParseExact(request.UpdateDate, "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out dateValue))
                {
                    if (OnAndAfter)
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.MODIDT >= Convert.ToDateTime(request.UpdateDate));
                    else
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.MODIDT == Convert.ToDateTime(request.UpdateDate));
                }
                else
                    throw new System.ArgumentException(string.Format("UpdateDate must be in yyyy-MM-dd date format ", "UpdateDate"));
            }

            if (request.GoAheadDate != null)
            {
                DateTime? GoAhdDate = null;
                if (DateTime.TryParseExact(request.GoAheadDate, "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out dateValue))
                {
                    GoAhdDate = dateValue;
                    if (OnAndAfter)
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.BookingGoAheadDT >= GoAhdDate);
                    else
                        filter = filter & Builders<mBookings>.Filter.Where(f => f.BookingGoAheadDT == GoAhdDate);
                }
                else
                    throw new System.ArgumentException(string.Format("Go-Ahead must be in yyyy-MM-dd date format ", "Go-Ahead"));
            }

            var result = await _MongoContext.mBookings.Find(filter).Project(p => new BookingList
            {
                AgentCode = p.AgentCode,
                Agentontact = p.AgentContactEmail,
                AgentName = p.AgentName,
                AgentReference = p.CUSTREF,
                EndDate = Convert.ToDateTime(p.ARRIVALDATE.ToString()).ToString("yyyy-MM-dd"),
                BookedDate = Convert.ToDateTime(p.CreaDT.ToString()).ToString("yyyy-MM-dd"),
                StartDate = Convert.ToDateTime(p.DEPARTUREDATE.ToString()).ToString("yyyy-MM-dd"),
                UpdateDate = Convert.ToDateTime(p.MODIDT.ToString()).ToString("yyyy-MM-dd"),
                BookingReference = p.BookingNumber,
                Duration = Convert.ToString(p.Duration),
                Status = p.Status,
                StatusDesc = p.StatusDesc,
                GoAheadDate = (p.BookingGoAheadDT == null) ? "" : Convert.ToDateTime(p.BookingGoAheadDT.ToString()).ToString("yyyy-MM-dd"),
                FileHandler = p.Operator,
                FileHandlerContact = p.OperatorEmail,
                Priority = p.PRIORITYDesc
            }).ToListAsync();

            result = result.OrderByDescending(p => p.BookingReference).ToList();

            var res = new BookingSearchRes();

            if (result.Count() > 0)
            {
                res.Bookings = result;
            }
            return res;
        }

        public async Task<UpdateOperationDetails_RS> UpdateOperationDetails(UpdateOperationDetails_RQ request, IConfiguration _configuration, UserCookieDetail userdetails)
        {
            UpdateOperationDetails_RS result = new UpdateOperationDetails_RS();
            common.GetUserCompanyType(ref userdetails);
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)

            FilterDefinition<mBookings> filter;
            FilterDefinition<mBookingPositions> filterPos;
            filter = Builders<mBookings>.Filter.Empty;
            filterPos = Builders<mBookingPositions>.Filter.Empty;

            foreach(UpdateOperationDetails upd in request.UpdateOperationDetails)
            {
                filter = Builders<mBookings>.Filter.Empty;
                filterPos = Builders<mBookingPositions>.Filter.Empty;
                if (upd.Position_Id != null)
                {
                    string curPos_Id = upd.Position_Id.ToString().ToLower();
                    filterPos = Builders<mBookingPositions>.Filter.Where(f => curPos_Id == f.Position_Id);
                    string Booking_Id = (await _MongoContext.mBookingPositions.Find(filterPos).Project(p => p.Booking_Id).FirstOrDefaultAsync());
                    Booking_Id = Booking_Id.ToLower();
                    filter = Builders<mBookings>.Filter.Where(f => f.BookingId == Booking_Id);
                    string AgentId = (await _MongoContext.mBookings.Find(filter).Project(p => p.AgentId).FirstOrDefaultAsync());
                    string Suppliers = (await _MongoContext.mBookings.Find(filter).Project(p => p.Suppliers).FirstOrDefaultAsync());

                    if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
                    {
                        if ((userdetails.IsAgent ?? false))
                        {
                            if (userdetails.Company_Id.Trim().ToLower() != AgentId.Trim().ToLower())
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }
                        else if ((userdetails.IsSupplier ?? false))
                        {
                            if (!Suppliers.Contains("," + userdetails.Company_Id + ","))
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }

                    }
                }
            }

            VGER_Communicator.Providers.BookingProviders provider = new VGER_Communicator.Providers.BookingProviders(_configuration);
            result = await provider.UpdateOperationDetails(request);
            if(result != null && result.UpdateOperationDetails.Count > 0)
                result.ResponseStatus.Status = "Success";
            else
                result.ResponseStatus.Status = "Failure";
            result.Request = request;
            return result;
        }

        public async Task<UpdatePurchaseDetails_RS> UpdatePurchaseDetails(UpdatePurchaseDetails_RQ request, IConfiguration _configuration, UserCookieDetail userdetails)
        {
            UpdatePurchaseDetails_RS result = new UpdatePurchaseDetails_RS();
            common.GetUserCompanyType(ref userdetails);
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)

            FilterDefinition<mBookings> filter;
            FilterDefinition<mBookingPositions> filterPos;
            filter = Builders<mBookings>.Filter.Empty;
            filterPos = Builders<mBookingPositions>.Filter.Empty;

            foreach (UpdatePurchaseDetails upd in request.UpdatePurchaseDetails)
            {
                filter = Builders<mBookings>.Filter.Empty;
                filterPos = Builders<mBookingPositions>.Filter.Empty;
                if (upd.Position_Id != null)
                {
                    string curPos_Id = upd.Position_Id.ToString().ToLower();
                    filterPos = Builders<mBookingPositions>.Filter.Where(f => curPos_Id == f.Position_Id);
                    string Booking_Id = (await _MongoContext.mBookingPositions.Find(filterPos).Project(p => p.Booking_Id).FirstOrDefaultAsync());
                    Booking_Id = Booking_Id.ToLower();
                    filter = Builders<mBookings>.Filter.Where(f => f.BookingId == Booking_Id);
                    string AgentId = (await _MongoContext.mBookings.Find(filter).Project(p => p.AgentId).FirstOrDefaultAsync());
                    string Suppliers = (await _MongoContext.mBookings.Find(filter).Project(p => p.Suppliers).FirstOrDefaultAsync());

                    if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
                    {
                        if ((userdetails.IsAgent ?? false))
                        {
                            if (userdetails.Company_Id.Trim().ToLower() != AgentId.Trim().ToLower())
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }
                        else if ((userdetails.IsSupplier ?? false))
                        {
                            if (!Suppliers.Contains("," + userdetails.Company_Id + ","))
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }

                    }
                }
            }
            VGER_Communicator.Providers.BookingProviders provider = new VGER_Communicator.Providers.BookingProviders(_configuration);
            result = await provider.UpdatePurchaseDetails(request);
            if (result != null && result.UpdatePurchaseDetails.Count > 0)
                result.ResponseStatus.Status = "Success";
            else
                result.ResponseStatus.Status = "Failure";
            result.Request = request;
            return result;
        }

        public async Task<UpdatePositionProduct_RS> UpdatePositionProduct(UpdatePositionProduct_RQ request, IConfiguration _configuration, UserCookieDetail userdetails)
        {
            UpdatePositionProduct_RS result = new UpdatePositionProduct_RS();
            common.GetUserCompanyType(ref userdetails);
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)

            FilterDefinition<mBookings> filter;
            FilterDefinition<mBookingPositions> filterPos;
            filter = Builders<mBookings>.Filter.Empty;
            filterPos = Builders<mBookingPositions>.Filter.Empty;

            foreach (UpdatePositionProduct upd in request.UpdatePositionProduct)
            {
                filter = Builders<mBookings>.Filter.Empty;
                filterPos = Builders<mBookingPositions>.Filter.Empty;
                if (upd.Position_Id != null)
                {
                    string curPos_Id = upd.Position_Id.ToString().ToLower();
                    filterPos = Builders<mBookingPositions>.Filter.Where(f => curPos_Id == f.Position_Id);
                    string Booking_Id = (await _MongoContext.mBookingPositions.Find(filterPos).Project(p => p.Booking_Id).FirstOrDefaultAsync());
                    Booking_Id = Booking_Id.ToLower();
                    filter = Builders<mBookings>.Filter.Where(f => f.BookingId == Booking_Id);
                    string AgentId = (await _MongoContext.mBookings.Find(filter).Project(p => p.AgentId).FirstOrDefaultAsync());
                    string Suppliers = (await _MongoContext.mBookings.Find(filter).Project(p => p.Suppliers).FirstOrDefaultAsync());

                    if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
                    {
                        if ((userdetails.IsAgent ?? false))
                        {
                            if (userdetails.Company_Id.Trim().ToLower() != AgentId.Trim().ToLower())
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }
                        else if ((userdetails.IsSupplier ?? false))
                        {
                            if (!Suppliers.Contains("," + userdetails.Company_Id + ","))
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "User is not authorised to update this position " + curPos_Id;
                                return result;
                            }
                        }

                    }
                }
            }
            VGER_Communicator.Providers.BookingProviders provider = new VGER_Communicator.Providers.BookingProviders(_configuration);
            result = await provider.UpdatePositionProduct(request);
            if (result != null && result.UpdatePositionProduct.Count > 0)
                result.ResponseStatus.Status = "Success";
            else
                result.ResponseStatus.Status = "Failure";
            result.Request = request;
            return result;
        }
    }
}
