using System;
using System.IO;
using System.IO.Compression;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Helpers
{
    public static class CommonFunction
    {
        public static string ReadFile(string Path)
        {
            string content = "";
            // string physicalWebRootPath = IServer("~/");

            //File.Exists(Path)

            content = File.ReadAllText("MyTextFile.txt");

            return content;
        }

        public static string[] SplitString(string source, char separator)
        {
            var result = source.Split(separator);

            //if (result.Length > 0)
            return result;
        }

        public static string FormatFileName(string fileName)
        {
            return fileName.Replace("/", "").Replace("\\", "").Replace(":", "").Replace("*", "")
                        .Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }

        public static void LogService(string content)
        {
            string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ErrorLog", "ErrorLog_" + DateTime.Today.ToString("ddMMyyyy") + ".txt");
            if (!Directory.Exists(Path.GetDirectoryName(outputFilePath))) Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            FileStream fs = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + " " + content + Environment.NewLine);
            sw.Flush();
            sw.Close();
        }

        public static ResponseStatus CreateZipFile(ZipDetails zipDetails)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                if (zipDetails != null && zipDetails.DocumentDetails?.Count > 0)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(zipDetails.ZipFilePath))) Directory.CreateDirectory(Path.GetDirectoryName(zipDetails.ZipFilePath));

                    string zipFilePath = Path.Combine(zipDetails.ZipFilePath, zipDetails.ZipFileName);
                    if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                    using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var fileToZip in zipDetails.DocumentDetails)
                        {
                            if (File.Exists(fileToZip.FullDocumentPath))
                            { 
                                //create the entry - this is the zipped filename
                                //change slashes - now it's VALID 
                                ZipArchiveEntry zipFileEntry = archive.CreateEntryFromFile(fileToZip.FullDocumentPath, fileToZip.DocumentName.Replace('\\', '/'), CompressionLevel.Fastest);
                            }
                        }
                    }

                    responseStatus.Status = "Success";
                }
                else
                {
                    responseStatus.Status = "Failure";
                    responseStatus.ErrorMessage = "ZipDetails/DocumentDetails cannot be null.";
                }
            }
            catch (Exception ex)
            {
                responseStatus.Status = "Failure";
                responseStatus.ErrorMessage = "CreateZipFile:- " + ex.Message;
            }
            return responseStatus;
        }
    }
}
