using Kata_Invoicing.Infrastructure.Common.Utils;
using Kata_Invoicing.Model;
using NPOI.SS.UserModel;
using System.IO;
using Kata_Invoicing.Model.Settings;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using System;
using Kata_Invoicing.Model.Invoices;
using log4net;
using NPOI.SS.Util;

namespace Kata_Invoicing.Code
{
    public sealed class ExcelExportInvoice : IExportableInvoice
    {
        private HSSFWorkbook workbook;
        private InvoiceModel model;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(ExcelExportInvoice));

        public ExcelExportInvoice()
        {
            this.workbook = new HSSFWorkbook();
        }

        public ExportResult Export(BaseInvoiceModel model)
        {
            this.model = model as InvoiceModel;

            var exportResult = new ExportResult();
            try
            {
                var settings = SettingsService.GetSettings();

                if (string.IsNullOrEmpty(settings.ExportInvoicesPath))
                {
                    exportResult.ErrorMessage = "Invoice Export Directory is not configured!";
                    return exportResult;
                }

                InitializeWorkBook();

                GenerateInvoiceSheet();

                //make final adjustments to the workbook according to the InvoiceType
                workbook.SetSheetName(0, this.model.InvoiceTypeName);

                //save invoice to the specific path
                exportResult.FilePath = SaveWorkBook(settings);
                if (!string.IsNullOrEmpty(exportResult.FilePath))
                {
                    //update invoice Status (only if not Final)
                    var invoice = InvoiceService.GetInvoiceByID(this.model.InvoiceDetails.InvoiceID);

                    invoice.InvoiceFilePathXLS = exportResult.FilePath;
                    var ok = InvoiceService.UpdateInvoice(invoice);
                    if (ok)
                    {
                        exportResult.IsOk = true;
                        this.model.InvoiceDetails.InvoiceFilePathXLS = exportResult.FilePath;
                    }
                    else
                    {
                        exportResult.ErrorMessage = "Problem on updating invoice in database!";
                    }
                }
            }
            catch (IOException e)
            {
                Log.Error($"InvoiceID : {this.model.InvoiceDetails.InvoiceID} - {e}");
                exportResult.ErrorMessage =
                    "Invoice not Exported! Please make sure that another user is not processing this invoice, and then try again. \n" +
                    e.Message;
            }
            catch (Exception e)
            {
                Log.Error($"InvoiceID : {this.model.InvoiceDetails.InvoiceID} - {e}");
                exportResult.ErrorMessage = e.Message;
            }

            return exportResult;
        }

        private void InitializeWorkBook()
        {
            //from template
            var file = new FileStream(@"Templates/Invoice.xls", FileMode.Open, FileAccess.Read);
            workbook = new HSSFWorkbook(file);

            var dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "Smart2Pay";
            workbook.DocumentSummaryInformation = dsi;

            var si = PropertySetFactory.CreateSummaryInformation();
            si.Title = "InvoiceItem";
            si.CreateDateTime = DateTime.Now;
            workbook.SummaryInformation = si;
        }

        private string SaveWorkBook(Settings settings)
        {
            var exportPath = Path.Combine(settings.ExportInvoicesPath);
            if (!Directory.Exists(exportPath)) Directory.CreateDirectory(exportPath);

            var invoiceName = this.model.InvoiceDetails.InvoiceNumber + Constants.XlsExtension;
            var invoiceFullName = Path.Combine(exportPath, invoiceName);

            var file = new FileStream(invoiceFullName, FileMode.Create);
            workbook.Write(file);
            file.Close();

            var filePath = invoiceFullName;
            return filePath;
        }

        private void GenerateInvoiceSheet()
        {
            var invoiceSheet = workbook.GetSheet("Invoice");

            var clientName = !string.IsNullOrEmpty(this.model.InvoiceDetails.ClientName)
                ? this.model.InvoiceDetails.ClientName
                : "";

            #region InvoiceHeader

            var indexStart = new IndexCell
            {
                Col = 1,
                Row = 5
            };
            var indexCellTemp = new IndexCell();

            IRow row;

            //Invoice Type Name
            if (!string.IsNullOrEmpty(this.model.InvoiceTypeName))
            {
                row = CellUtil.GetRow(4, invoiceSheet);
                CellUtil.GetCell(row, 0).SetCellValue(this.model.InvoiceTypeName);
            }

            //Client Dates
            row = CellUtil.GetRow(indexStart.Row++, invoiceSheet);
            var cell = CellUtil.GetCell(row, indexStart.Col);

            cell.SetCellValue(clientName);
            cell.CellStyle.WrapText = true;

            indexCellTemp.Row = indexStart.Row + 7;

            indexStart.Row = 5;
            indexStart.Col = 4;


            row = CellUtil.GetRow(indexStart.Row++, invoiceSheet);
            cell = CellUtil.GetCell(row, indexStart.Col - 1);
            cell.SetCellValue(this.model.InvoiceTypeName.ToLower() + " number");
            CellUtil.GetCell(row, indexStart.Col).SetCellValue(this.model.InvoiceNumber);

            row = CellUtil.GetRow(indexStart.Row++, invoiceSheet);
            cell = CellUtil.GetCell(row, indexStart.Col);
            cell.SetCellValue(this.model.InvoiceDate);

            row = CellUtil.GetRow(indexStart.Row++, invoiceSheet);
            cell = CellUtil.GetCell(row, indexStart.Col);
            cell.SetCellValue(this.model.InvoiceStartDate);
            cell = CellUtil.GetCell(row, indexStart.Col + 1);
            cell.SetCellValue(this.model.InvoiceEndDate);

            indexStart.Row = indexCellTemp.Row;

            #endregion

            if (this.model.InvoiceDetails.InvoiceTypeID != (int)InvoiceType.Invoice)
            {
                #region Payments

                indexStart.Col = 0;
                indexCellTemp.Col = indexStart.Col;
                indexStart.Row = indexCellTemp.Row;

                //Payments
                row = CellUtil.GetRow(indexCellTemp.Row, invoiceSheet);
                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue("Payments");
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.Bolded, CellStyleUtil.Border.TopLeft, BorderStyle.Thin, null);

                row = CellUtil.GetRow(indexCellTemp.Row++, invoiceSheet);
                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue($"{this.model.InvoiceStartDate} - {this.model.InvoiceEndDate}");
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);

                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue(string.Empty);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);

                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue(string.Empty);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);

                cell = CellUtil.GetCell(row, indexCellTemp.Col);
                cell.SetCellValue((double)this.model.PaymentsAmount);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.All, BorderStyle.Medium,
                    this.model.InvoiceSettlementCurrency);

                indexCellTemp.Col = (short)(indexStart.Col + 1);



                //Total Payments
                row = CellUtil.GetRow(indexCellTemp.Row, invoiceSheet);
                cell = CellUtil.GetCell(row, indexStart.Col + 5);
                cell.SetCellValue((double)this.model.TotalPaymentsAmount);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.All, BorderStyle.Medium,
                    this.model.InvoiceSettlementCurrency);

                //set borders
                for (var i = indexStart.Row + 1; i <= indexCellTemp.Row; i++)
                {
                    row = CellUtil.GetRow(i, invoiceSheet);
                    cell = CellUtil.GetCell(row, indexStart.Col);
                    cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                        CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Left, BorderStyle.Thin, null);
                }

                //set borders on total row
                row = CellUtil.GetRow(indexCellTemp.Row + 1, invoiceSheet);
                for (var i = 0; i < indexStart.Col + 5; i++)
                {
                    cell = CellUtil.GetCell(row, indexStart.Col + i);
                    cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                        CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Top, BorderStyle.Thin, null);
                }

                indexStart.Row = indexCellTemp.Row + 5;

                #endregion
            }

            if (this.model.InvoiceDetails.InvoiceTypeID != (int)InvoiceType.Settlement)
            {
                #region Transaction Fees

                indexStart.Col = 0;
                indexCellTemp.Col = indexStart.Col;
                indexCellTemp.Row = indexStart.Row;


                //Transaction Fees
                row = CellUtil.GetRow(indexCellTemp.Row, invoiceSheet);
                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue("Transaction Fees");
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.Bolded, CellStyleUtil.Border.TopLeft, BorderStyle.Thin, null);

                row = CellUtil.GetRow(indexCellTemp.Row++, invoiceSheet);
                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue($"{this.model.InvoiceStartDate} - {this.model.InvoiceEndDate}");
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);

                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue(string.Empty);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);

                cell = CellUtil.GetCell(row, indexCellTemp.Col++);
                cell.SetCellValue(string.Empty);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.TopBottom, BorderStyle.Medium, null);


                cell = CellUtil.GetCell(row, indexCellTemp.Col);
                cell.SetCellValue((double)this.model.TransactionFeesAmount);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.All, BorderStyle.Medium,
                    this.model.InvoiceSettlementCurrency);

                indexCellTemp.Col = (short)(indexStart.Col + 1);

                //Total Transaction Fees
                row = CellUtil.GetRow(indexCellTemp.Row, invoiceSheet);
                cell = CellUtil.GetCell(row, indexStart.Col + 5);
                cell.SetCellValue((double)this.model.TotalTransactionFeesAmount);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.All, BorderStyle.Medium,
                    this.model.InvoiceSettlementCurrency);

                //set borders
                for (var i = indexStart.Row + 1; i <= indexCellTemp.Row; i++)
                {
                    row = CellUtil.GetRow(i, invoiceSheet);
                    cell = CellUtil.GetCell(row, indexStart.Col);
                    cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                        CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Left, BorderStyle.Thin, null);
                }

                //set borders on total row
                row = CellUtil.GetRow(indexCellTemp.Row + 1, invoiceSheet);
                for (var i = 0; i < indexStart.Col + 5; i++)
                {
                    cell = CellUtil.GetCell(row, indexStart.Col + i);
                    cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                        CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Top, BorderStyle.Thin, null);
                }

                indexStart.Row = indexCellTemp.Row + 5;
                indexStart.Col = 0;

                #endregion
            }

            #region TOTAL

            //TOTAL PAY OUT
            row = CellUtil.GetRow(indexStart.Row + 2, invoiceSheet);
            for (var i = 1; i < 5; i++)
            {
                cell = CellUtil.GetCell(row, indexStart.Col + i);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Bottom, BorderStyle.Medium, null);
            }

            row = CellUtil.GetRow(indexStart.Row + 3, invoiceSheet);
            for (var i = 1; i < 5; i++)
            {
                cell = CellUtil.GetCell(row, indexStart.Col + i);
                cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                    CellStyleUtil.Bold.NotBolded, CellStyleUtil.Border.Bottom, BorderStyle.Thin, null);
            }

            cell = CellUtil.GetCell(row, indexStart.Col + 1);
            cell.SetCellValue(this.model.TotalDescription);
            cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Text,
                CellStyleUtil.Bold.Bolded, CellStyleUtil.Border.Bottom, BorderStyle.Thin, null);

            row = CellUtil.GetRow(indexStart.Row + 3, invoiceSheet);
            cell = CellUtil.GetCell(row, indexStart.Col + 5);
            cell.SetCellValue((double)this.model.TotalPayOutAmount); //totalPayoutAmount
            cell.CellStyle = CellStyleUtil.GetCellStyleKey(workbook, CellStyleUtil.CellType.Number,
                CellStyleUtil.Bold.Bolded, CellStyleUtil.Border.All, BorderStyle.Medium, this.model.InvoiceSettlementCurrency);

            #endregion


        }
    }
}
