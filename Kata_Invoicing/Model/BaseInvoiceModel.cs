namespace Kata_Invoicing.Model
{
    public class BaseInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceStartDate { get; set; }
        public string InvoiceEndDate { get; set; }
        public string InvoiceSettlementCurrency { get; set; }
        public string InvoiceTypeName { get; set; }

        public decimal PaymentsAmount { get; set; }
        public decimal TotalPaymentsAmount { get; set; }

        public decimal TransactionFeesAmount { get; set; }
        public decimal TotalTransactionFeesAmount { get; set; }

        public string TotalDescription { get; set; }
        public decimal TotalPayOutAmount { get; set; }
    }
}
