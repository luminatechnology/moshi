using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.Reports;
using PX.Reports;
using PX.Reports.Data;
using PX.DbServices;

namespace PX.Objects.AR
{
    public class ARInvoiceEntry_Extension : PXGraphExtension<ARInvoiceEntry>
    {
        #region Action
        public PXAction<ARInvoice> exportInvoices2PDF;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Invoices(Not combined)", Visible = false)]
        public virtual IEnumerable ExportInvoices2PDF(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate ()
            {
                PrepareReportFiles(adapter.Get<ARInvoice>().ToList());
            });

            return adapter.Get();
        }
        #endregion

        #region Method
        private void PrepareReportFiles(List<ARInvoice> invoices)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ZipArchive zip = new ZipArchive(ms);

                foreach (ARInvoice invoice in invoices)
                {
                    // Report Paramenters
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    parameters["ARInvoice.DocType"] = invoice.DocType;
                    parameters["ARInvoice.RefNbr"]  = invoice.RefNbr;

                    // Report Processing
                    PX.Reports.Controls.Report _report = PXReportTools.LoadReport(PX.Objects.SO.SOShipmentEntry_Extension.CommInvRpt, null);

                    PXReportTools.InitReportParameters(_report, parameters, SettingsProvider.Instance.Default);

                    ReportNode reportNode = ReportProcessor.ProcessReport(_report);

                    // Generation PDF
                    byte[] data = PX.Reports.Mail.Message.GenerateReport(reportNode, ReportProcessor.FilterPdf).First();

                    using (System.IO.Stream zipStream = zip.OpenWrite(string.Format("{0}{1}", reportNode.ExportFileName, PX.Objects.CN.Common.Descriptor.Constants.PdfFileExtension)))
                    {
                        zipStream.Write(data, 0, data.Length);
                    }
                }

                PX.SM.FileInfo file = new PX.SM.FileInfo(Base.Accessinfo.CompanyName + "_Commercial Invoices.zip", null, ms.ToArray());

                // Downloading of the report
                throw new PXRedirectToFileException(file, true);
            }
        }
        #endregion
    }
}