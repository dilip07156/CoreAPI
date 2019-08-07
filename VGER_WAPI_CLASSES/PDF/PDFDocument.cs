using System; 
using System.IO; 
using System.Collections.Generic; 

namespace VGER_WAPI_CLASSES
{
    public static class PaperTypes
    {
        public static String A0 = "A0";
        public static String A1 = "A1";
        public static String A2 = "A2";
        public static String A3 = "A3";
        public static String A4 = "A4";
        public static String A5 = "A5";
        public static String A6 = "A6";
        public static String A7 = "A7";
        public static String A8 = "A8";
        public static String A9 = "A9";
        public static String B0 = "B0";
        public static String B1 = "B1";
        public static String B10 = "B10";
        public static String B2 = "B2";
        public static String B3 = "B3";
        public static String B4 = "B4";
        public static String B5 = "B5";
        public static String B6 = "B6";
        public static String B7 = "B7";
        public static String B8 = "B8";
        public static String B9 = "B9";
        public static String C5E = "C5E";
        public static String Comm10E = "Comm10E";
        public static String DLE = "DLE";
        public static String Executive = "Executive";
        public static String Folio = "Folio";
        public static String Ledger = "Ledger";
        public static String Legal = "Legal";
        public static String Letter = "Letter";
        public static String Tabloid = "Tabloid";
    }

    public class PdfConvertException : Exception
    {
        public PdfConvertException(String msg) : base(msg) { }
    }

    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
    }

    public class PdfOutput
    {
        public String OutputFilePath { get; set; }
        public Stream OutputStream { get; set; }
        public Action<PdfDocument, byte[]> OutputCallback { get; set; }
    }

    public class PdfDocument
    {
        public String PaperType { get; set; }
        public String Url { get; set; }
        public String Html { get; set; }
        public String HeaderUrl { get; set; }
        public String FooterUrl { get; set; }
        public String HeaderLeft { get; set; }
        public String HeaderCenter { get; set; }
        public String HeaderRight { get; set; }
        public String FooterLeft { get; set; }
        public String FooterCenter { get; set; }
        public String FooterRight { get; set; }
        public object State { get; set; }
        public Dictionary<String, String> Cookies { get; set; }
        public Dictionary<String, String> ExtraParams { get; set; }
        public String HeaderFontSize { get; set; }
        public String FooterFontSize { get; set; }
        public String HeaderFontName { get; set; }
        public String FooterFontName { get; set; }
        public String JWTToken { get; set; }
        public String AntiForgeryToken { get; set; }
        public String CookiesToken { get; set; }
    }

    public class PdfConvertEnvironment
    {
        public String TempFolderPath { get; set; }
        public String WkHtmlToPdfPath { get; set; }
        public int Timeout { get; set; }
        public bool Debug { get; set; }
    }  
}
