using Kata_Invoicing.Code;
using Kata_Invoicing.InvoiceManager.InvoiceGenerators;
using System;

namespace Startup
{
    class Program
    {
        static void Main(string[] args)
        {
            //Invoices:
            //SettlementReport: InvoiceID: 3
            //Invoice: InvoiceID: 1

            BaseInvoiceGenerator generator;

            generator = new SettlementReportGenerator();
            var exportResult1 = generator.ExportInvoice(new ExcelExportInvoice());
            Console.WriteLine($"File {exportResult1.FilePath} exported");

            generator = new InvoiceGenerator();
            var exportResult2 = generator.ExportInvoice(new PdfExportInvoice());

            generator = new SettlementInvoiceGenerator();
            var exportResult3 = generator.ExportInvoice(new NotepadExportInvoice());

            Console.WriteLine("finished...");
            Console.ReadKey();
        }
    }
}
