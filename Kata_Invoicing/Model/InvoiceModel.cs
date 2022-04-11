using Kata_Invoicing.Model;
using Kata_Invoicing.Model.Invoices;
using System.Collections.Generic;

namespace Kata_Invoicing.Code
{
    public class InvoiceModel : BaseInvoiceModel
    {
        public InvoiceDetails InvoiceDetails;

        public List<InvoicePayment> InvoicePayments;

        public List<InvoiceFee> InvoiceFees;
    }
}
