using Kata_Invoicing.Model;

namespace Kata_Invoicing.Code
{
    public interface IExportableInvoice
    {
        ExportResult Export(BaseInvoiceModel model);
    }
}
