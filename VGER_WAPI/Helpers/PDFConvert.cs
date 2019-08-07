using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO; 
using System.Text;
using System.Threading;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Helpers
{
    public class PdfConvert
    {
        public static IConfiguration _configuration;
        static PdfConvertEnvironment _e;

        public static PdfConvertEnvironment Environment
        {
            get
            {
                if (_e == null)
                    _e = new PdfConvertEnvironment
                    {
                        TempFolderPath = Path.GetTempPath(),
                        WkHtmlToPdfPath = GetWkhtmlToPdfExeLocation(),
                        Timeout = 120000
                    };
                return _e;
            }
        }

        private static string GetWkhtmlToPdfExeLocation()
        {
            string filePath, customPath = _configuration.GetValue<string>("SystemSettings:wkHtmlToPdfLocation");
            //"C:\\Program Files\\wkhtmltopdf\\bin"; //ConfigurationManager.AppSettings["wkhtmltopdf:path"];

            if (customPath != null)
            {
                filePath = Path.Combine(customPath, @"wkhtmltopdf.exe");

                if (File.Exists(filePath))
                    return filePath;
            }

            string programFilesPath = System.Environment.GetEnvironmentVariable("ProgramFiles");
            filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            string programFilesx86Path = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            filePath = Path.Combine(programFilesx86Path, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
            if (File.Exists(filePath))
                return filePath;

            return Path.Combine(programFilesx86Path, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdf(document, null, output);
        }

        public static void ConvertHtmlToPdfCustom(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdfCustom(document, null, output);
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = Environment;

            if (document.Html != null)
                document.Url = "-";

            String outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, String.Format("{0}.pdf", Guid.NewGuid()));
                delete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException(String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.WkHtmlToPdfPath));

            StringBuilder paramsBuilder = new StringBuilder();

            if (String.IsNullOrEmpty(document.PaperType))
                document.PaperType = PaperTypes.A4;
            paramsBuilder.AppendFormat("--page-size {0} ", document.PaperType);

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                paramsBuilder.Append("--margin-top 25 ");
                paramsBuilder.Append("--header-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
                paramsBuilder.Append("--margin-bottom 25 ");
                paramsBuilder.Append("--footer-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.HeaderLeft))
                paramsBuilder.AppendFormat("--header-left \"{0}\" ", document.HeaderLeft);

            if (!string.IsNullOrEmpty(document.HeaderCenter))
                paramsBuilder.AppendFormat("--header-center \"{0}\" ", document.HeaderCenter);

            if (!string.IsNullOrEmpty(document.HeaderRight))
                paramsBuilder.AppendFormat("--header-right \"{0}\" ", document.HeaderRight);

            if (!string.IsNullOrEmpty(document.FooterLeft))
                paramsBuilder.AppendFormat("--footer-left \"{0}\" ", document.FooterLeft);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--footer-center \"{0}\" ", document.FooterCenter);

            if (!string.IsNullOrEmpty(document.FooterRight))
                paramsBuilder.AppendFormat("--footer-right \"{0}\" ", document.FooterRight);

            if (!string.IsNullOrEmpty(document.HeaderFontSize))
                paramsBuilder.AppendFormat("--header-font-size \"{0}\" ", document.HeaderFontSize);

            if (!string.IsNullOrEmpty(document.FooterFontSize))
                paramsBuilder.AppendFormat("--footer-font-size \"{0}\" ", document.FooterFontSize);

            if (!string.IsNullOrEmpty(document.HeaderFontName))
                paramsBuilder.AppendFormat("--header-font-name \"{0}\" ", document.HeaderFontName);

            if (!string.IsNullOrEmpty(document.FooterFontName))
                paramsBuilder.AppendFormat("--footer-font-name \"{0}\" ", document.FooterFontName);


            if (document.ExtraParams != null)
                foreach (var extraParam in document.ExtraParams)
                    paramsBuilder.AppendFormat("--{0} {1} ", extraParam.Key, extraParam.Value);

            if (document.Cookies != null)
                foreach (var cookie in document.Cookies)
                    paramsBuilder.AppendFormat("--cookie {0} {1} ", cookie.Key, cookie.Value);

            paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);

            try
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = paramsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        DataReceivedEventHandler outputHandler = (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };

                        DataReceivedEventHandler errorHandler = (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        process.OutputDataReceived += outputHandler;
                        process.ErrorDataReceived += errorHandler;

                        try
                        {
                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (document.Html != null)
                            {
                                using (var stream = process.StandardInput)
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(document.Html);
                                    stream.BaseStream.Write(buffer, 0, buffer.Length);
                                    stream.WriteLine();
                                }
                            }

                            if (process.WaitForExit(environment.Timeout) && outputWaitHandle.WaitOne(environment.Timeout) && errorWaitHandle.WaitOne(environment.Timeout))
                            {
                                if (process.ExitCode != 0 && !File.Exists(outputPdfFilePath))
                                {
                                    throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Wkhtmltopdf output: \r\n{1}", document.Url, error));
                                }
                            }
                            else
                            {
                                if (!process.HasExited)
                                    process.Kill();

                                throw new PdfConvertTimeoutException();
                            }
                        }
                        finally
                        {
                            process.OutputDataReceived -= outputHandler;
                            process.ErrorDataReceived -= errorHandler;
                        }
                    }
                }


                if (woutput.OutputStream != null)
                {
                    using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[32 * 1024];
                        int read;

                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                            woutput.OutputStream.Write(buffer, 0, read);
                    }
                }

                if (woutput.OutputCallback != null)
                {
                    byte[] pdfFileBytes = File.ReadAllBytes(outputPdfFilePath);
                    woutput.OutputCallback(document, pdfFileBytes);
                }

            }
            finally
            {
                if (delete && File.Exists(outputPdfFilePath))
                    File.Delete(outputPdfFilePath);
            }
        }

        public static void ConvertHtmlToPdfCustom(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = Environment;

            if (document.Html != null)
                document.Url = "-";

            String outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, String.Format("{0}.pdf", Guid.NewGuid()));
                delete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException(String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.WkHtmlToPdfPath));

            StringBuilder paramsBuilder = new StringBuilder();

            //if (String.IsNullOrEmpty(document.PaperType))
            //    document.PaperType = PaperTypes.A4;
            //paramsBuilder.AppendFormat("--page-size {0} ", document.PaperType);

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
            }
            if (!string.IsNullOrEmpty(document.JWTToken))
            {
                string cookieData = ".AspNetCore.Antiforgery.5uYu5VgZzcE=" + document.AntiForgeryToken
                    + ";JWTToken=" + document.JWTToken
                    + ";.AspNetCore.Cookies=" + document.CookiesToken;
                paramsBuilder.AppendFormat("--custom-header Cookie \"{0}\" ", cookieData);
            }

            paramsBuilder.AppendFormat("--debug-javascript ");
            paramsBuilder.AppendFormat("--no-custom-header-propagation ");

            /*-B, --margin-bottom <unitreal>      Set the page bottom margin
              -L, --margin-left <unitreal>        Set the page left margin (default 10mm)
              -R, --margin-right <unitreal>       Set the page right margin (default 10mm)
              -T, --margin-top <unitreal> */

            if (document.ExtraParams != null)
                foreach (var extraParam in document.ExtraParams)
                    paramsBuilder.AppendFormat("--{0} {1} ", extraParam.Key, extraParam.Value);

            if (document.Cookies != null)
                foreach (var cookie in document.Cookies)
                    paramsBuilder.AppendFormat("--cookie {0} {1} ", cookie.Key, cookie.Value);

            paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);

            try
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = paramsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        DataReceivedEventHandler outputHandler = (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };

                        DataReceivedEventHandler errorHandler = (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        process.OutputDataReceived += outputHandler;
                        process.ErrorDataReceived += errorHandler;

                        try
                        {
                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (document.Html != null)
                            {
                                using (var stream = process.StandardInput)
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(document.Html);
                                    stream.BaseStream.Write(buffer, 0, buffer.Length);
                                    stream.WriteLine();
                                }
                            }

                            if (process.WaitForExit(environment.Timeout) && outputWaitHandle.WaitOne(environment.Timeout) && errorWaitHandle.WaitOne(environment.Timeout))
                            {
                                if (process.ExitCode != 0 && !File.Exists(outputPdfFilePath))
                                {
                                    throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Wkhtmltopdf output: \r\n{1}", document.Url, error));
                                }
                            }
                            else
                            {
                                if (!process.HasExited)
                                    process.Kill();

                                throw new PdfConvertTimeoutException();
                            }
                        }
                        finally
                        {
                            process.OutputDataReceived -= outputHandler;
                            process.ErrorDataReceived -= errorHandler;
                        }
                    }
                }


                if (woutput.OutputStream != null)
                {
                    using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[32 * 1024];
                        int read;

                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                            woutput.OutputStream.Write(buffer, 0, read);
                    }
                }

                if (woutput.OutputCallback != null)
                {
                    byte[] pdfFileBytes = File.ReadAllBytes(outputPdfFilePath);
                    woutput.OutputCallback(document, pdfFileBytes);
                }

            }
            finally
            {
                if (delete && File.Exists(outputPdfFilePath))
                    File.Delete(outputPdfFilePath);
            }
        }

        internal static void ConvertHtmlToPdf(string url, string outputFilePath)
        {
            ConvertHtmlToPdf(new PdfDocument { Url = url }, new PdfOutput { OutputFilePath = outputFilePath });
        }

        public static bool GenerateDocument(string PageURL, string HeaderURL, string FooterUrl, string PDFPath, string token, IRequestCookieCollection cookies)
        {
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Environment.Debug = false;
                ConvertHtmlToPdfCustom(new PdfDocument
                {
                    Url = PageURL,
                    HeaderUrl = HeaderURL,
                    FooterUrl = FooterUrl,
                    JWTToken = token,
                    AntiForgeryToken = cookies[".AspNetCore.Antiforgery.5uYu5VgZzcE"],
                    CookiesToken = cookies[".AspNetCore.Cookies"]
                }, new PdfOutput
                {
                    OutputFilePath = PDFPath
                });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static ResponseStatusMessage GenerateDocument(PdfDocument pdfDocument, PdfOutput pdfOutput)
        {
            ResponseStatusMessage responseStatusMessage = new ResponseStatusMessage();
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Environment.Debug = false;
                ConvertHtmlToPdfCustom(pdfDocument, pdfOutput);
                responseStatusMessage.Status = "Success";
            }
            catch (Exception ex)
            {
                responseStatusMessage.Status = "Failure";
                responseStatusMessage.ErrorMessage.Add(ex.Message);
            }
            return responseStatusMessage;
        }

        public static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".zip", "application/zip"}
            };
        }

        //class OSUtil
        //{
        //    public static string GetProgramFilesx86Path()
        //    {
        //        if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
        //        {
        //            return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        //        }
        //        return Environment.GetEnvironmentVariable("ProgramFiles");
        //    }
        //}

        //public static class HttpResponseExtensions
        //{
        //    public static void SendFileForDownload(this HttpResponse response, String filename, byte[] content)
        //    {
        //        SetFileDownloadHeaders(response, filename);
        //        response.OutputStream.Write(content, 0, content.Length);
        //        response.Flush();
        //    }

        //    public static void SendFileForDownload(this HttpResponse response, String filename)
        //    {
        //        SetFileDownloadHeaders(response, filename);
        //        response.TransmitFile(filename);
        //        response.Flush();
        //    }

        //    public static void SetFileDownloadHeaders(this HttpResponse response, String filename)
        //    {
        //        FileInfo fi = new FileInfo(filename);
        //        response.ContentType = "application/force-download";
        //        response.AddHeader("Content-Disposition", "attachment; filename=\"" + fi.Name + "\"");
        //    }
        //}
    }
}
