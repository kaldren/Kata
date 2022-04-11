using Kata_Invoicing.Model;
using System;

namespace Kata_Invoicing.Code
{
    public sealed class PdfExportInvoice : IExportableInvoice
    {
        public ExportResult Export(BaseInvoiceModel model)
        {
            // Logic for exporting to PDF here
            Console.WriteLine("Export to PDF");
            return new ExportResult();
        }
    }
}
