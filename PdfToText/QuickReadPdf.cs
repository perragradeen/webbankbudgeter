using System;
using System.Collections.Generic;
using System.Text;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace PdfToText
{
    public class QuickReadPdf
    {
        public static string ReadPdf(string fullPath)
        {
            return PdfParser.PdfTextRead.pdfText(fullPath);
        }
    }
}

namespace PdfParser
{
    public static class PdfTextRead
    {
        public static string pdfText(string path)
        {
            PdfReader reader = new PdfReader(path);
            string text = string.Empty;
            for(int page = 1; page <= reader.NumberOfPages; page++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader,page);
            }
            reader.Close();
            return text;
        }   
    }
}
