using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace PdfToText
{
    public static class PdfTextRead
    {
        public static string PdfText(string path)
        {
            var reader = new PdfReader(path);
            var text = string.Empty;
            for (var page = 1; page <= reader.NumberOfPages; page++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader, page);
            }

            reader.Close();
            return text;
        }
    }
}