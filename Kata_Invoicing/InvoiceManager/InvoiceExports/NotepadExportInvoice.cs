using Kata_Invoicing.Model;
using System;

namespace Kata_Invoicing.Code
{
    public sealed class NotepadExportInvoice : IExportableInvoice
    {
        public ExportResult Export(BaseInvoiceModel model)
        {
            // Logic for exporting to Notepad here
            Console.WriteLine("Export to notepad");
            return new ExportResult();
        }
    }
}
