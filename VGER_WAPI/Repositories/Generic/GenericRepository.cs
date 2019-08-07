using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
//comment before commiting to development
//using Microsoft.Office.Interop.Outlook;
//using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace VGER_WAPI.Repositories
{
    public class GenericRepository : IGenericRepository
    {
        private readonly MongoContext _MongoContext = null;
        private readonly IConfiguration _configuration;
        private readonly ICostsheetRepository _costsheetRepository;

        public GenericRepository(IOptions<MongoSettings> settings, IConfiguration configuration, ICostsheetRepository costsheetRepository)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _costsheetRepository = costsheetRepository;
        }

        #region GetNextReferenceNumber

        public QRFCounterResponse GetNextReferenceNumber(QRFCounterRequest request)
        {
            var result = (from c in _MongoContext.mSysCounters.AsQueryable()
                          where c.CounterType == request.CounterType
                          select c.LastReferenceNumber).FirstOrDefault();

            var response = new QRFCounterResponse();
            if (result != 0)
            {
                long val = ++result;
                _MongoContext.mSysCounters.UpdateOne(x => x.CounterType == request.CounterType,
               Builders<mSysCounters>.Update.Set(x => x.LastReferenceNumber, val));
                response.LastReferenceNumber = val;
            }
            else
            {
                var item = new mSysCounters
                {
                    CounterType = request.CounterType,
                    LastReferenceNumber = 1,
                    CreateDate = DateTime.Now,
                    EditDate = DateTime.Now,
                    CreateUser = "Admin",
                    EditUser = "Admin"
                };
                _MongoContext.mSysCounters.InsertOne(item);
                response.LastReferenceNumber = item.LastReferenceNumber;
            }
            return response;
        }

        #endregion 

        //public async Task<MailGetRes> SendEmailAsync(EmailConfig email)
        //{
        //    MailGetRes response = new MailGetRes();
        //    response.ResponseStatus = new ResponseStatus();
        //    List<string> lstBCC = new List<string>();
        //    List<string> lstCC = new List<string>();
        //    try
        //    {
        //        var emailMessage = new MimeMessage();

        //        email.FromAddress = _configuration.GetValue<string>("FromAddress");
        //        var IsTestEnv = _configuration.GetValue<string>("IsTestEnv");

        //        //email.FromAddress = "internationalmarkets.ops@coxandkings.ae";
        //        if (IsTestEnv == "1")
        //        {
        //            email.ToAddress = _configuration.GetValue<string>("ToAddress"); 
        //        }
        //        else
        //        {
        //            email.ToAddress = email.ToAddress;
        //        }
        //        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ToCc")))
        //        {
        //            email.ToCc = _configuration.GetValue<string>("ToCc");
        //            emailMessage.Bcc.Add(new MailboxAddress(email.ToCc));
        //            lstBCC.Add(email.ToCc);
        //        }

        //        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ToBcc")))
        //        {
        //            email.ToBcc = _configuration.GetValue<string>("ToBcc");
        //            emailMessage.Bcc.Add(new MailboxAddress(email.ToBcc));
        //            lstBCC.Add(email.ToBcc);
        //        }

        //        //email.ToAddress = "matt.watson@ckdms.com";
        //        //email.ToAddress = "anand.desai@coxandkings.com";
        //        //email.ToAddress = email.ToAddress;
        //        //email.ToAddress = "manisha.bhosale@coxandkings.com";

        //        email.MailServerAddress = _configuration.GetValue<string>("MailServerAddress");
        //        //email.MailServerAddress = "172.21.200.122";

        //        email.MailServerPort = _configuration.GetValue<string>("MailServerPort");
        //        //email.MailServerPort = "25"; 

        //        //emailMessage.Bcc.Add(new MailboxAddress(email.ToBcc));
        //        emailMessage.From.Add(new MailboxAddress(email.FromAddress));
        //        emailMessage.To.Add(new MailboxAddress(email.ToAddress));
        //        emailMessage.Subject = email.Subject;

        //        var bodyBuilder = new BodyBuilder();
        //        bodyBuilder.HtmlBody = email.Body;
        //        //emailMessage.Body = new TextPart(TextFormat.Html) { Text = email.Body };
        //        //bodyBuilder.Attachments.Add(email.Attachment); 

        //        if (email.Attachment != null && email.Attachment.Count > 0)
        //        {
        //            ////Getting attachment stream
        //            //var fileBytes = File.ReadAllBytes(@"E:\VGERUI\wwwroot\documents\test.pdf");
        //            ////You must to inform the mime-type of the attachment and his name
        //            //bodyBuilder.Attachments.Add("ProposalDocument.pdf", fileBytes, new MimeKit.ContentType("application", "pdf"));

        //            string filepath = "";
        //            foreach (var item in email.Attachment)
        //            {
        //                if (!string.IsNullOrEmpty(item))
        //                {
        //                    if (email.PathType == "sendtoclient")
        //                    {
        //                        filepath = Path.Combine(_configuration.GetValue<string>("ProposalPDFPath"), item);
        //                        if (File.Exists(filepath))
        //                        {
        //                            var fileBytes = File.ReadAllBytes(filepath);
        //                            bodyBuilder.Attachments.Add(item, fileBytes, new MimeKit.ContentType("application", "pdf"));
        //                        }
        //                    }
        //                }
        //            }
        //            emailMessage.Body = bodyBuilder.ToMessageBody();
        //        }
        //        else
        //        {
        //            emailMessage.Body = bodyBuilder.ToMessageBody();
        //        }

        //        using (var client = new SmtpClient())
        //        {
        //            client.CheckCertificateRevocation = false;
        //            await client.ConnectAsync(email.MailServerAddress, Convert.ToInt32(email.MailServerPort), SecureSocketOptions.None).ConfigureAwait(false);
        //            await client.SendAsync(emailMessage).ConfigureAwait(false);
        //            response.MailSentOn = DateTime.Now;
        //            await client.DisconnectAsync(true).ConfigureAwait(false);
        //        }
        //        response.ResponseStatus.Status = "Success";

        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.ToString();
        //        response.ResponseStatus.ErrorMessage = "Mail not sent.";
        //        response.ResponseStatus.Status = "Error";
        //    }
        //    if (email.IsSave)
        //    {
        //        if (email.Attachment != null && email.Attachment.Count > 0)
        //        {
        //            email.Attachment = email.Attachment.Select(filename => Path.Combine(_configuration.GetValue<string>("ProposalPDFPath"), filename)).ToList();
        //        }

        //        MailSetReq request = new MailSetReq()
        //        {
        //            EmailDetails = new EmailDetails
        //            {
        //                BCC = lstBCC,
        //                CC = lstCC,
        //                EmailDetailsId = email.EmailDetailsId,
        //                EmailHtml = email.Body,
        //                From = email.FromAddress,
        //                MailSent = response.ResponseStatus.Status == "Success" ? "1" : "0",
        //                MailSentBy = email.MailSentBy,
        //                MailSentOn = DateTime.Now,
        //                MailStatus = email.MailStatus,
        //                PDFPath = email.Attachment,
        //                Remarks = email.Remarks,
        //                Subject = email.Subject,
        //                To = email.ToAddress,
        //                MailType = email.Type
        //            },
        //            QRFID = email.QRFID,
        //            QRFPriceID = email.QRFPriceID
        //        };
        //        await _costsheetRepository.SetEmailDetails(request);
        //    }
        //    return response;
        //}

        public AttributeValues getExchangeRate(string FromCurrencyId, string ToCurrencyId, string QRFID)
        {
            var result = new AttributeValues();

            if (!string.IsNullOrEmpty(FromCurrencyId) && !string.IsNullOrEmpty(ToCurrencyId))
            {
                //var ExchangeRateId = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(b => b.ExchangeRateId).FirstOrDefault();

                //var ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().ToList();

                var ExchangeRateDetailList = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).Select(b => b.ExchangeRateSnapshot.ExchangeRateDetail).FirstOrDefault();

                if (ExchangeRateDetailList == null || ExchangeRateDetailList?.Count == 0)
                {
                    var BaseCurrency = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(a => new ExchangeRateSnapshot
                    {
                        Currency_Id = a.Currency_Id,
                        REFCUR = a.RefCur,
                        ExchangeRate_id = a.ExchangeRateId,
                        DATEMAX = a.DateMax,
                        DATEMIN = a.DateMin,
                        EXRATE = a.ExRate,
                        VATRATE = a.VatRate,
                        CREA_DT = a.CreateDate
                    }).FirstOrDefault();

                    ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == BaseCurrency.ExchangeRate_id)
                        .Select(a => new ExchangeRateDetailSnapshot
                        {
                            ExchangeRateDetail_Id = a.ExchangeRateDetail_Id,
                            Currency_Id = a.Currency_Id,
                            CURRENCY = a.CURRENCY,
                            RATE = a.RATE,
                            ROUNDTO = a.ROUNDTO
                        }).ToList();
                }

                var FromCurrencyRate = ExchangeRateDetailList?.Where(a => a.Currency_Id == FromCurrencyId.ToLower()).FirstOrDefault();

                var ToCurrencyRate = ExchangeRateDetailList?.Where(a => a.Currency_Id == ToCurrencyId.ToLower()).FirstOrDefault();

                if (!(FromCurrencyRate == null || ToCurrencyRate == null))
                {
                    result.Value = Convert.ToString(ToCurrencyRate.RATE / FromCurrencyRate.RATE);

                    result.AttributeValue_Id = ToCurrencyRate.ExchangeRateDetail_Id;
                }
            }
            return result;
        }

        public bool getExchangeRateForPosition(ref List<mQRFPositionTotalCost> positionList)
        {
            if (positionList?.Count > 0)
            {
                double rate = 0;

                string QRFID = positionList?[0].QRFID;
                var BaseCurrency = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).Select(b => b.ExchangeRateSnapshot).FirstOrDefault();
                var ExchangeRateDetailList = BaseCurrency?.ExchangeRateDetail;

                if (ExchangeRateDetailList == null || ExchangeRateDetailList?.Count == 0)
                {
                    BaseCurrency = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(a => new ExchangeRateSnapshot
                    {
                        Currency_Id = a.Currency_Id,
                        REFCUR = a.RefCur,
                        ExchangeRate_id = a.ExchangeRateId,
                        DATEMAX = a.DateMax,
                        DATEMIN = a.DateMin,
                        EXRATE = a.ExRate,
                        VATRATE = a.VatRate,
                        CREA_DT = a.CreateDate
                    }).FirstOrDefault();

                    ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == BaseCurrency.ExchangeRate_id)
                        .Select(a => new ExchangeRateDetailSnapshot
                        {
                            Currency_Id = a.Currency_Id,
                            CURRENCY = a.CURRENCY,
                            RATE = a.RATE,
                            ROUNDTO = a.ROUNDTO
                        }).ToList();
                }

                if (ExchangeRateDetailList?.Count > 0)
                {
                    foreach (var pos in positionList)
                    {
                        var FromCurrencyRate = ExchangeRateDetailList.Where(a => a.Currency_Id == BaseCurrency.Currency_Id).FirstOrDefault();

                        var ToCurrencyRateBuy = ExchangeRateDetailList.Where(a => a.Currency_Id == pos.BuyCurrencyId).FirstOrDefault();

                        if (!(FromCurrencyRate == null || ToCurrencyRateBuy == null))
                        {
                            rate = Math.Round(Convert.ToDouble(ToCurrencyRateBuy.RATE / FromCurrencyRate.RATE), 4);

                            if (rate > 0)
                            {
                                pos.TotalBuyPrice = pos.TotalBuyPrice / rate;
                                pos.BuyCurrency = BaseCurrency.REFCUR;
                            }
                        }


                        var ToCurrencyRateSell = ExchangeRateDetailList.Where(a => a.Currency_Id == pos.QRFCurrency_Id).FirstOrDefault();

                        if (!(FromCurrencyRate == null || ToCurrencyRateSell == null))
                        {
                            rate = Math.Round(Convert.ToDouble(ToCurrencyRateSell.RATE / FromCurrencyRate.RATE), 4);

                            if (rate > 0)
                            {
                                pos.TotalSellPrice = pos.TotalSellPrice / rate;
                                if (pos.TotalSellPrice < 0)//for negative numbers
                                {
                                    pos.TotalSellPrice = Math.Floor(pos.TotalSellPrice);
                                }
                                else if (pos.TotalSellPrice > 0)//for positive numbers
                                {
                                    pos.TotalSellPrice = Math.Ceiling(pos.TotalSellPrice);
                                }
                                pos.ProfitAmount = pos.ProfitAmount / rate;
                                pos.QRFCurrency = BaseCurrency.REFCUR;
                            }
                        }
                    }
                }

            }
            return true;
        }

        public AttributeValues getExchangeRateFromBooking(string FromCurrencyId, string ToCurrencyId, string BookingNumber)
        {
            var result = new AttributeValues();
            try
            {

                if (!string.IsNullOrEmpty(FromCurrencyId) && !string.IsNullOrEmpty(ToCurrencyId))
                {
                    var ExchangeRateDetailList = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == BookingNumber).Select(b => b.ExchangeRateSnapshot.ExchangeRateDetail).FirstOrDefault();

                    if (ExchangeRateDetailList == null || ExchangeRateDetailList?.Count == 0)
                    {
                        var BaseCurrency = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(a => new ExchangeRateSnapshot
                        {
                            Currency_Id = a.Currency_Id,
                            REFCUR = a.RefCur,
                            ExchangeRate_id = a.ExchangeRateId,
                            DATEMAX = a.DateMax,
                            DATEMIN = a.DateMin,
                            EXRATE = a.ExRate,
                            VATRATE = a.VatRate,
                            CREA_DT = a.CreateDate
                        }).FirstOrDefault();

                        ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == BaseCurrency.ExchangeRate_id)
                            .Select(a => new ExchangeRateDetailSnapshot
                            {
                                ExchangeRateDetail_Id = a.ExchangeRateDetail_Id,
                                Currency_Id = a.Currency_Id,
                                CURRENCY = a.CURRENCY,
                                RATE = a.RATE,
                                ROUNDTO = a.ROUNDTO
                            }).ToList();
                    }

                    var FromCurrencyRate = ExchangeRateDetailList?.Where(a => a.Currency_Id == FromCurrencyId.ToLower()).FirstOrDefault();

                    var ToCurrencyRate = ExchangeRateDetailList?.Where(a => a.Currency_Id == ToCurrencyId.ToLower()).FirstOrDefault();

                    if (!(FromCurrencyRate == null || ToCurrencyRate == null))
                    {
                        result.Value = Convert.ToString(ToCurrencyRate.RATE / FromCurrencyRate.RATE);

                        result.AttributeValue_Id = ToCurrencyRate.ExchangeRateDetail_Id;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public GeocoderLocationGetRes GetLocationLatLon(string address)
        {
            var res = new GeocoderLocationGetRes();
            try
            {
                //string URL = "https://maps.googleapis.com/maps/api/geocode/xml?sensor=false&key=GoogleAPIKey&address=" + HttpUtility.UrlEncode(query);
                string URL = _configuration.GetValue<string>("URLGeoCode");
                string APIKey = _configuration.GetValue<string>("GoogleAPIKey");

                URL = URL + "?key=" + APIKey;
                URL = URL + "&sensor=false";
                URL = URL + "&address=" + HttpUtility.UrlEncode(address);

                WebRequest request = WebRequest.Create(URL);

                request.Proxy = WebRequest.DefaultWebProxy;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        XDocument document = XDocument.Load(new StreamReader(stream));

                        XElement longitudeElement = document.Descendants("lng").FirstOrDefault();
                        XElement latitudeElement = document.Descendants("lat").FirstOrDefault();

                        if (longitudeElement != null && latitudeElement != null)
                        {
                            res.GeocoderLocation = new GeocoderLocation
                            {
                                Longitude = Double.Parse(longitudeElement.Value, CultureInfo.InvariantCulture),
                                Latitude = Double.Parse(latitudeElement.Value, CultureInfo.InvariantCulture)
                            };
                            res.ResponseStatus.Status = "Success";
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                res.ResponseStatus.Status = "Failure";
                res.ResponseStatus.ErrorMessage = ex.Message;
            }
            return res;
        }

        public GeocoderLocationGetRes GetLocationLatLonByType(string address, string types)
        {
            var res = new GeocoderLocationGetRes();
            try
            {
                //string URL = "https://maps.googleapis.com/maps/api/geocode/xml?sensor=false&key=GoogleAPIKey&address=" + HttpUtility.UrlEncode(query);
                string URL = _configuration.GetValue<string>("URLGeoCodeByType");
                string APIKey = _configuration.GetValue<string>("GoogleAPIKey");

                URL = URL + "?key=" + APIKey;
                URL = URL + "&inputtype=textquery";
                URL = URL + "&input=" + HttpUtility.UrlEncode(address);
                URL = URL + "&types=" + types;
                URL = URL + "&fields=formatted_address,name,geometry,place_id";

                WebRequest request = WebRequest.Create(URL);

                request.Proxy = WebRequest.DefaultWebProxy;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        XDocument document = XDocument.Load(new StreamReader(stream));

                        XElement longitudeElement = document.Descendants("lng").FirstOrDefault();
                        XElement latitudeElement = document.Descendants("lat").FirstOrDefault();

                        if (longitudeElement != null && latitudeElement != null)
                        {
                            res.GeocoderLocation = new GeocoderLocation
                            {
                                Longitude = Double.Parse(longitudeElement.Value, CultureInfo.InvariantCulture),
                                Latitude = Double.Parse(latitudeElement.Value, CultureInfo.InvariantCulture)
                            };
                            res.ResponseStatus.Status = "Success";
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                res.ResponseStatus.Status = "Failure";
                res.ResponseStatus.ErrorMessage = ex.Message;
            }
            return res;
        }

        public async Task<DistanceMatrixGetRes> GetDistanceMatrix(DistanceMatrixGetReq req)
        {
            //string URL = "https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins=40.6655101,-73.89188969999998&destinations=41.6655101,-73.89188969999998&key=AIzaSyDhxUUgHmu48Zv0_ECSms00t9OzxZkE1h0&transit_mode=walk";
            //string URL = "https://maps.googleapis.com/maps/api/distancematrix/json?key=GoogleAPIKey&units=units&origins=origins&destinations=destinations&transit_mode=transit_mode";

            string URL = _configuration.GetValue<string>("URLDistanceMatrix");
            string APIKey = _configuration.GetValue<string>("GoogleAPIKey");

            URL = URL + "?key=" + APIKey;
            URL = URL + "&units=" + req.Units;
            URL = URL + "&origins=" + req.Origins.Latitude + "," + req.Origins.Longitude;
            URL = URL + "&destinations=" + req.Destinations.Latitude + "," + req.Destinations.Longitude;
            URL = URL + "&transit_mode=" + req.Transit_Mode;

            var json = JsonConvert.SerializeObject(req);

            HttpClient client = new HttpClient();

            var content = new StringContent("", Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage responseMessage = await client.PostAsync(URL, content);
                var responseJsonString = await responseMessage.Content.ReadAsStringAsync();
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, typeof(DistanceMatrixGetRes));
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public async Task<DistanceMatrixGetRes> GetDistanceMatrixForCity(string FromCityId, string ToCityId, string Units = "metric", string TransitMode = "bus")
        {
            DistanceMatrixGetRes response = new DistanceMatrixGetRes();
            DistanceMatrixGetReq request = new DistanceMatrixGetReq();
            try
            {
                request.Units = Units;
                request.Transit_Mode = TransitMode;

                var FromCity = GetCityDetails(FromCityId);
                var ToCity = GetCityDetails(ToCityId);
                if (FromCity == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City not found : " + FromCityId;
                    return response;
                }
                if (ToCity == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City not found : " + ToCity;
                    return response;
                }
                request.Origins.Latitude = Convert.ToDouble(FromCity.Lat);
                request.Origins.Longitude = Convert.ToDouble(FromCity.Lon);
                request.Destinations.Latitude = Convert.ToDouble(ToCity.Lat);
                request.Destinations.Longitude = Convert.ToDouble(ToCity.Lon);

                response = await GetDistanceMatrix(request);
                if (response == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Error occurred while calling Google service";
                }
                else
                {
                    response.OriginCity = FromCity.ResortName;
                    response.DestinationCity = ToCity.ResortName;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<DistanceMatrixGetRes> GetDistanceMatrixForProduct(string FromProductId, string ToProductId, string Units = "metric", string TransitMode = "bus")
        {
            DistanceMatrixGetRes response = new DistanceMatrixGetRes();
            DistanceMatrixGetReq request = new DistanceMatrixGetReq();
            try
            {
                request.Units = Units;
                request.Transit_Mode = TransitMode;

                var FromProduct = GetProductDetails(FromProductId);
                var ToProduct = GetProductDetails(ToProductId);
                if (FromProduct == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product not found : " + FromProductId;
                    return response;
                }
                if (ToProduct == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product not found : " + ToProductId;
                    return response;
                }
                if (FromProduct.Placeholder == true || ToProduct.Placeholder == true)//if any of the From or To Placeholder is true then do not calculate matrix
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product not found : " + ToProductId;
                    return response;
                }
                request.Origins.Latitude = Convert.ToDouble(FromProduct.Lat);
                request.Origins.Longitude = Convert.ToDouble(FromProduct.Long);
                request.Destinations.Latitude = Convert.ToDouble(ToProduct.Lat);
                request.Destinations.Longitude = Convert.ToDouble(ToProduct.Long);

                response = await GetDistanceMatrix(request);
                if (response == null)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Error occurred while calling Google service";
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public mResort GetCityDetails(string CityId)
        {
            var City = _MongoContext.mResort.AsQueryable().Where(a => a.Voyager_Resort_Id == CityId).FirstOrDefault();
            if (City == null)
                return null;
            if (string.IsNullOrEmpty(City.Lat) || string.IsNullOrEmpty(City.Lon) || City.Lon == "0.00")
            {
                GeocoderLocation CityLatLon = GetLocationLatLon(City.Lookup).GeocoderLocation;

                City.Lat = Convert.ToString(CityLatLon.Latitude);
                City.Lon = Convert.ToString(CityLatLon.Longitude);
                City.EditDate = DateTime.Now;
                City.EditUser = "System";

                ReplaceOneResult replaceResult = _MongoContext.mResort.ReplaceOne(Builders<mResort>.Filter.Eq("Voyager_Resort_Id", City.Voyager_Resort_Id), City);
            }
            return City;
        }

        #region 3rd party get Country and City info based on Id as mandator and by name(not mandatory).

        public async Task<PartnerCountryCityRes> GetPartnerCountryDetails(Attributes CountryInfo)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            var Country = string.IsNullOrEmpty(CountryInfo.AttributeName) ? _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode == CountryInfo.Attribute_Id && a.ResortType == "Country").FirstOrDefault()
                                    : _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode == CountryInfo.Attribute_Id && a.ResortType == "Country" && a.ResortName.Trim().ToLower() == CountryInfo.AttributeName.Trim().ToLower()).FirstOrDefault();
            if (Country == null)
                return response;
            if (string.IsNullOrEmpty(Country.Lat) || string.IsNullOrEmpty(Country.Lon) || Country.Lon == "0.00")
            {
                //GeocoderLocation CountryLatLon = GetLocationLatLon(Country.Lookup).GeocoderLocation;

                //Country.Lat = Convert.ToString(CountryLatLon.Latitude);
                //Country.Lon = Convert.ToString(CountryLatLon.Longitude);
                //Country.EditDate = DateTime.Now;
                //Country.EditUser = "System";

                //ReplaceOneResult replaceResult = _MongoContext.mResort.ReplaceOne(Builders<mResort>.Filter.Eq("Voyager_Resort_Id", Country.Voyager_Resort_Id), Country);
            }
            response.ResortInfo = Country;
            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCityDetails(Attributes CityInfo)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            var City = string.IsNullOrEmpty(CityInfo.AttributeName) ? _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode == CityInfo.Attribute_Id && a.ResortType == "City").FirstOrDefault()
                            : _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode == CityInfo.Attribute_Id && a.ResortType == "City" && a.ResortName.Trim().ToLower() == CityInfo.AttributeName.Trim().ToLower()).FirstOrDefault();
            if (City == null)
                return response;
            if (string.IsNullOrEmpty(City.Lat) || string.IsNullOrEmpty(City.Lon) || City.Lon == "0.00")
            {
                //GeocoderLocation CityLatLon = GetLocationLatLon(City.Lookup).GeocoderLocation;

                //City.Lat = Convert.ToString(CityLatLon.Latitude);
                //City.Lon = Convert.ToString(CityLatLon.Longitude);
                //City.EditDate = DateTime.Now;
                //City.EditUser = "System";

                //ReplaceOneResult replaceResult = _MongoContext.mResort.ReplaceOne(Builders<mResort>.Filter.Eq("Voyager_Resort_Id", City.Voyager_Resort_Id), City);
            }
            response.ResortInfo = City;
            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCountryDetailsBasedOnCode(string Countrycode)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            var Country = _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode.ToLower() == Countrycode.ToLower() && a.ResortType == "Country").FirstOrDefault();
            if (Country == null)
                return response;

            response.ResortInfo = Country;

            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnCode(string CityCode)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            var City = _MongoContext.mResort.AsQueryable().Where(a => a.ResortCode.ToLower() == CityCode.ToLower() && a.ResortType == "City").FirstOrDefault();
            if (City == null)
                return response;

            response.ResortInfo = City;

            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnName(TravelogiCountryCityRes CityInfo)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            var City = _MongoContext.mResort.AsQueryable().Where(a => a.ResortName.ToLower() == CityInfo.TargetSupplierCityName.ToLower() && a.ParentResortCode.ToLower() == CityInfo.TargetSupplierCountryCode.ToLower() && a.ResortType == "City").FirstOrDefault();
            if (City == null)
                return response;

            response.ResortInfo = City;

            return response;
        }

        #endregion

        public mProducts GetProductDetails(string ProductId)
        {
            var Product = _MongoContext.mProducts.AsQueryable().Where(a => a.VoyagerProduct_Id == ProductId).FirstOrDefault();
            if (Product == null)
                return null;
            if (string.IsNullOrEmpty(Product.Lat) || string.IsNullOrEmpty(Product.Long))
            {
                string address = Product.ProdName + "," + Product.CityName + "," + Product.CountryName;
                string type = new Func<string>(() => { switch (Product.ProductType) { case "Hotel": return "lodging"; case "Meal": return "restaurant"; default: return null; } })();

                GeocoderLocation ProductLatLon = GetLocationLatLonByType(address, type).GeocoderLocation;

                Product.Lat = Convert.ToString(ProductLatLon.Latitude);
                Product.Long = Convert.ToString(ProductLatLon.Longitude);
                Product.EditDate = DateTime.Now;
                Product.EditUser = "System";

                ReplaceOneResult replaceResult = _MongoContext.mProducts.ReplaceOne(Builders<mProducts>.Filter.Eq("VoyagerProduct_Id", Product.VoyagerProduct_Id), Product);
            }
            return Product;
        }

        public bool IsSalesOfficeUser(string UserName)
        {
            try
            {
                var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => !string.IsNullOrEmpty(x.UserName) && x.UserName.ToLower() == UserName.ToLower().Trim()).Select(y => y.Company_Id).FirstOrDefault();
                var ParentAgent_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == UserCompany_Id).Select(y => y.HeadOffice_Id).FirstOrDefault();

                if (ParentAgent_Id == CoreCompany_Id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeletePositionPriceFOC(List<string> PositionIds, string UserName, bool IsClone = false, bool IsHardDelete = false)
        {
            try
            {
                if (PositionIds?.Count > 0 && !string.IsNullOrEmpty(UserName))
                {
                    if (IsClone)
                    {
                        if (IsHardDelete)
                        {
                            await _MongoContext.mPositionPriceQRF.DeleteManyAsync<mPositionPriceQRF>(a => PositionIds.Contains(a.PositionId));
                            await _MongoContext.mQRFPositionFOC.DeleteManyAsync<mQRFPositionFOC>(a => PositionIds.Contains(a.PositionId));
                        }
                        else
                        {
                            UpdateResult updateResultPrice = await _MongoContext.mPositionPriceQRF.UpdateManyAsync<mPositionPriceQRF>(a => PositionIds.Contains(a.PositionId),
                        Builders<mPositionPriceQRF>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).Set("EditUser", UserName));

                            UpdateResult updateResultFoc = await _MongoContext.mQRFPositionFOC.UpdateManyAsync<mQRFPositionFOC>(a => PositionIds.Contains(a.PositionId),
                                  Builders<mQRFPositionFOC>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).Set("EditUser", UserName));
                        }
                    }
                    else
                    {
                        if (IsHardDelete)
                        {
                            await _MongoContext.mPositionPrice.DeleteManyAsync<mPositionPrice>(a => PositionIds.Contains(a.PositionId));
                            await _MongoContext.mPositionFOC.DeleteManyAsync<mPositionFOC>(a => PositionIds.Contains(a.PositionId));
                        }
                        else
                        {
                            UpdateResult updateResultPrice = await _MongoContext.mPositionPrice.UpdateManyAsync<mPositionPrice>(a => PositionIds.Contains(a.PositionId),
                         Builders<mPositionPrice>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).Set("EditUser", UserName));

                            UpdateResult updateResultFoc = await _MongoContext.mPositionFOC.UpdateManyAsync<mPositionFOC>(a => PositionIds.Contains(a.PositionId),
                                  Builders<mPositionFOC>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).Set("EditUser", UserName));
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        
        public DateTime? ConvertStringToDateTime(string Date)
        {
            DateTime? newDate = null;
            if (!string.IsNullOrEmpty(Date))
            {
                var strFromDT = Date.Split("/");
                if (strFromDT?.Count() >= 3)
                {
                    DateTime fromDT = new DateTime(Convert.ToInt32(strFromDT[2]), Convert.ToInt32(strFromDT[1]), Convert.ToInt32(strFromDT[0]));
                    newDate = fromDT;
                }
                else
                {
                    newDate = null;
                }
            }
            return newDate;
        }
    }
}
