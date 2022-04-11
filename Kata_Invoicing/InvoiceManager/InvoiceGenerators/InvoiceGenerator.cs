using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Kata_Invoicing.Code;
using Kata_Invoicing.Infrastructure.Common.Utils;
using Kata_Invoicing.Model.Invoices;
using Kata_Invoicing.Model.Settings;
using log4net;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace Kata_Invoicing.InvoiceManager.InvoiceGenerators
{
    public sealed class InvoiceGenerator : BaseInvoiceGenerator
    {
        #region Constructor

        public InvoiceGenerator() : base(1)
        {
        }

        #endregion

        #region Export Invoice Methods

        public override ExportResult ExportInvoice(IExportableInvoice exportInvoice)
        {
            return exportInvoice.Export(base.InvoiceModel);
        }

        #region Private Methods

        //get data from db
        protected override void GetInvoiceDataFromDb()
        {
            base.GetInvoiceDataFromDb();
        }

        protected override void ComputeInvoiceFirstPage()
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

        #endregion
    }

    #endregion
}