using Kata_Invoicing.Code;
using Kata_Invoicing.Model.Invoices;
using log4net;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Kata_Invoicing.InvoiceManager.InvoiceGenerators
{
    public abstract class BaseInvoiceGenerator
    {
        protected readonly int InvoiceId;
        protected string ErrorMessage;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseInvoiceGenerator));

        protected InvoiceModel InvoiceModel = new InvoiceModel();

        public BaseInvoiceGenerator(int invoiceId)
        {
            this.InvoiceId = invoiceId;

            try
            {
                GetInvoiceDataFromDb();
                ComputeInvoiceFirstPage();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                ErrorMessage = ex.Message;
            }
        }

        public abstract ExportResult ExportInvoice(IExportableInvoice exportInvoice);

        protected virtual void ComputeInvoiceFirstPage()
        {
            try
            {
                #region InvoiceData

                //set Invoice Data that is computed on the generation time
                InvoiceModel.InvoiceNumber = !string.IsNullOrEmpty(InvoiceModel.InvoiceDetails.InvoiceNumber)
                    ? InvoiceModel.InvoiceDetails.InvoiceNumber
                    : "000000000000";
                InvoiceModel.InvoiceDate = InvoiceModel.InvoiceDetails.InvoiceDate.ToString(Constants.LONG_DATE_FORMAT);
                InvoiceModel.InvoiceStartDate = InvoiceModel.InvoiceDetails.InvoiceStartDate.ToString(Constants.LONG_DATE_FORMAT);
                InvoiceModel.InvoiceEndDate = InvoiceModel.InvoiceDetails.InvoiceEndDate.ToString(Constants.LONG_DATE_FORMAT);
                InvoiceModel.InvoiceSettlementCurrency = InvoiceModel.InvoiceDetails.InvoiceSettlementCurrency;

                InvoiceModel.InvoiceTypeName = InvoiceModel.InvoiceDetails.InvoiceTypeName;

                #endregion

                #region Payments

                //set the payments
                InvoiceModel.PaymentsAmount = InvoiceModel.InvoicePayments.Where(x => x.PaymentTypeID == (int)PaymentType.Payment)
                    .Sum(x => x.CalculatedAmount);

                InvoiceModel.TotalPaymentsAmount = InvoiceModel.PaymentsAmount;

                #endregion

                #region TransactionFees

                InvoiceModel.TransactionFeesAmount = InvoiceModel.InvoiceFees.Where(x => x.FeeID == (int)FeeType.TransactionFee).Sum(f => f.CalculatedAmount);

                InvoiceModel.TotalTransactionFeesAmount = InvoiceModel.TransactionFeesAmount;

                #endregion

                #region Total

                InvoiceModel.TotalDescription = InvoiceModel.InvoiceDetails.InvoiceTypeID == (int)InvoiceType.Invoice ? "TOTAL FEES" : "TOTAL PAY OUT";

                InvoiceModel.TotalPayOutAmount = InvoiceModel.InvoiceDetails.InvoiceTypeID == (int)InvoiceType.Invoice ? InvoiceModel.TotalTransactionFeesAmount : InvoiceModel.PaymentsAmount;

                #endregion
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        protected virtual void GetInvoiceDataFromDb()
        {
            try
            {
                //get invoice data from database
                InvoiceModel.InvoiceDetails = InvoiceService.GetInvoiceDetails(InvoiceId);
                InvoiceModel.InvoicePayments = InvoiceService.GetInvoicePayments(InvoiceId);
                InvoiceModel.InvoiceFees = InvoiceService.GetInvoiceFees(InvoiceId);
            }
            catch (SqlException e)
            {
                Log.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }
    }
}
