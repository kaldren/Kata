using Kata_Invoicing.Code;

namespace Kata_Invoicing.InvoiceManager.InvoiceGenerators
{
    public sealed class SettlementReportGenerator : BaseInvoiceGenerator
    {
        public SettlementReportGenerator() : base(1)
        {
        }

        public override ExportResult ExportInvoice(IExportableInvoice exportInvoice)
        {
            return exportInvoice.Export(base.InvoiceModel);
        }

        protected override void ComputeInvoiceFirstPage()
        {
            base.ComputeInvoiceFirstPage();
        }

        protected override void GetInvoiceDataFromDb()
        {
            base.GetInvoiceDataFromDb();
        }
    }
}
