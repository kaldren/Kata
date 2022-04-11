using Kata_Invoicing.Code;

namespace Kata_Invoicing.InvoiceManager.InvoiceGenerators
{
    public sealed class SettlementInvoiceGenerator : BaseInvoiceGenerator
    {
        public SettlementInvoiceGenerator() : base (3)
        {

        }

        public override ExportResult ExportInvoice(IExportableInvoice exportInvoice)
        {
            return exportInvoice.Export(base.InvoiceModel);
        }

        protected override void ComputeInvoiceFirstPage()
        {
            base.GetInvoiceDataFromDb();
        }

        protected override void GetInvoiceDataFromDb()
        {
            base.GetInvoiceDataFromDb();
        }
    }
}
