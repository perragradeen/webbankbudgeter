namespace PdfToText
{
    public class QuickReadPdf
    {
        public static string ReadPdf(string fullPath)
        {
            return PdfTextRead.PdfText(fullPath);
        }
    }
}