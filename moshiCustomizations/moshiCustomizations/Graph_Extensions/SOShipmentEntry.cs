using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Reports;
using PX.Export.Excel.Core;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Reports;
using PX.Reports.Data;
using PX.SM;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
    public class SOShipmentEntry_Extension : PXGraphExtension<SOShipmentEntry>
    {
        #region Override Data View Definition        
        [PXViewName(Messages.SOPackageDetail)]
        [PXImport(typeof(SOPackageDetailEx))]
        public PXSelect<SOPackageDetailEx, Where<SOPackageDetailEx.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> Packages;
        #endregion

        #region Properties & Constant Classes
        public int count;
        public string reportID;
        public string rptFormat;
        public string fileExt;

        public const string CommInvRpt     = "SO643042";
        public const string CommInvRpt2    = "SO643041";
        public const string PackListRpt    = "SO642010";
        public const string PackListRpt2   = "SO642011";
        public const string stockout       = "Print Stockout Form";
        public const string PckQtyExceeded = "Packing Qty Cannot Exceed Remaining Qty.";
        public const string NonNotifSrc    = "The Customer [{0}] Mailing Settings Not Yet Set.";
        public const string NoCommInv      = "No Commercial Invoice Data.";
        public const string NoCustContAddr = "The Customer [{0}] Doesn't Have Contact Email Address.";

        public class EAN13Attr : PX.Data.BQL.BqlString.Constant<EAN13Attr>
        {
            public EAN13Attr() : base("EAN13") { }
        }

        public class CartonQtyAttr : PX.Data.BQL.BqlString.Constant<CartonQtyAttr>
        {
            public CartonQtyAttr() : base("CTQTY") { }
        }
        #endregion

        #region Actions
        public PXAction<SOShipment> printStockoutForm;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Stockout Form", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable PrintStockoutForm(PXAdapter adapter)
        {
            List<SOShipment> shipments = new List<SOShipment>();

            foreach (SOShipment soShipment in adapter.Get<SOShipment>())
            {
                shipments.Add(soShipment);
            }

            this.ExportToExcel(shipments);

            return adapter.Get();
        }

        public PXAction<SOShipLine> autoPacking;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Auto Packing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable AutoPacking(PXAdapter adapter)
        {
            SOShipLine shipLine = Base.Transactions.Current;

            Base.Save.Press();

            PXLongOperation.StartOperation(Base, delegate ()
            {
                PackageCreation(SOPackageType.Auto, shipLine);
            });

            return adapter.Get();
        }

        public PXAction<SOShipLine> manualPacking;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Manual Packing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable ManualPacking(PXAdapter adapter)
        {
            this.Base.Save.Press();


            PXLongOperation.StartOperation(Base, delegate ()
            {
                PackageCreation(SOPackageType.Manual, Base.Transactions.Current);
            });

            return adapter.Get();
        }

        public PXAction<SOShipment> emailToCustomer;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Send Email To Customer", Visible = false)]
        public virtual IEnumerable EmailToCustomer(PXAdapter adapter) => Notification(adapter, "COMMERCIAL INV2", SendDestination.Customer);

        public PXAction<SOShipment> emailToUPS;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Send Email To UPS", Visible = false)]
        public virtual IEnumerable EmailToUPS(PXAdapter adapter) => Notification(adapter, "COMMERCIAL INV", SendDestination.UPS);

        public PXAction<SOShipment> notification;
        [PXUIField(DisplayName = "", Visible = false)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
        public virtual IEnumerable Notification(PXAdapter adapter,
        [PXString]
        string notificationCD, SendDestination destination)
        {
            if (destination == SendDestination.Customer)
            {
                reportID  = PackListRpt2;
                rptFormat = ReportProcessor.FilterPdf;
                fileExt   = PX.Objects.CN.Common.Descriptor.Constants.PdfFileExtension;
            }
            else
            {
                reportID  = PackListRpt;
                rptFormat = ReportProcessor.FilterExcel;
                fileExt   = PX.Objects.CN.Common.Descriptor.Constants.ExcelFileExtension;
            }

            foreach (SOShipment shipment in adapter.Get<SOShipment>().RowCast<SOShipment>().Where(ship => ship.Selected == true))
            {
                try
                {
                    Base.Document.Current = shipment;

                    VerifyCustomerMailInfo(destination == SendDestination.Customer ? CommInvRpt2 : CommInvRpt);

                    // Report Paramenters
                    Dictionary<string, string> packingParms = new Dictionary<string, string>();
                    Dictionary<string, string> invoiceParms = new Dictionary<string, string>();

                    List<Guid?> attachments = new List<Guid?>();

                    foreach (SOOrderShipment orderShip in SelectFrom<SOOrderShipment>.Where<SOOrderShipment.shipmentType.IsEqual<@P.AsString>
                                                                                            .And<SOOrderShipment.shipmentNbr.IsEqual<@P.AsString>>>
                                                                                     .AggregateTo<GroupBy<SOOrderShipment.invoiceType,
                                                                                                          GroupBy<SOOrderShipment.invoiceNbr>>>.View.Select(Base, shipment.ShipmentType, shipment.ShipmentNbr))
                    {
                        if (string.IsNullOrEmpty(orderShip.InvoiceNbr) )
                        {
                            throw new PXException(NoCommInv);
                        }

                        invoiceParms["DocType"] = orderShip.InvoiceType;
                        invoiceParms["RefNbr"] = orderShip.InvoiceNbr;

                        foreach (SOPackageDetailEx detailEx in SelectFrom<SOPackageDetailEx>.Where<SOPackageDetailEx.shipmentNbr.IsEqual<@P.AsString>>
                                                                                            .AggregateTo<GroupBy<SOPackageDetailEx.shipmentNbr>>.View.Select(Base, shipment.ShipmentNbr) )
                        {
                            packingParms["ShipmentNbr"] = shipment.ShipmentNbr;

                            // Report Processing
                            PX.Reports.Controls.Report _report = PXReportTools.LoadReport(reportID, null);

                            PXReportTools.InitReportParameters(_report, packingParms, SettingsProvider.Instance.Default);

                            ReportNode reportNode = ReportProcessor.ProcessReport(_report);

                            // Generation file
                            byte[] data = PX.Reports.Mail.Message.GenerateReport(reportNode, rptFormat).First();

                            PX.SM.FileInfo file = new PX.SM.FileInfo(reportNode.ExportFileName + fileExt, null, data);

                            UploadFileMaintenance graph = PXGraph.CreateInstance<PX.SM.UploadFileMaintenance>();

                            // Save the file with the setting to create a new version if one already exists based on the UID                          
                            graph.SaveFile(file, FileExistsAction.CreateVersion);

                            attachments.Add(file.UID);
                        }
                    }

                    PX.Objects.GL.Branch branch = PXSelectReadonly2<PX.Objects.GL.Branch, InnerJoin<INSite, On<INSite.branchID, Equal<PX.Objects.GL.Branch.branchID>>>,
                                                                                          Where<INSite.siteID, Equal<Optional<SOShipment.destinationSiteID>>,
                                                                                                And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>,
                                                                                                    Or<INSite.siteID, Equal<Optional<SOShipment.siteID>>,
                                                                                                       And<Current<SOShipment.shipmentType>, NotEqual<SOShipmentType.transfer>>>>>>
                                                                                          .SelectSingleBound(Base, new object[] { shipment });

                    Base.Activity.SendNotification(PX.Objects.AR.ARNotificationSource.Customer, notificationCD, (branch != null && branch.BranchID != null) ? branch.BranchID : Base.Accessinfo.BranchID, invoiceParms, attachments);

                    if (destination == SendDestination.Customer)
                    {
                        Base.Document.Cache.SetValue<SOShipmentExt.usrSentCustomer>(shipment, true);
                    }
                    else
                    {
                        Base.Document.Cache.SetValue<SOShipmentExt.usrSentCarrier>(shipment, true);
                    }

                    Base.Document.Cache.Update(shipment);
                }
                catch (PXException)
                {
                    throw;
                }

                yield return shipment;
            }

            Base.Save.Press();
        }

        public PXAction<SOShipment> exportPackingList;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Export Packing List To Excel", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable ExportPackingList(PXAdapter adapter)
        {
            moshiCustomizations.Descriptor.ExportReportToExcel reportToExcel = new moshiCustomizations.Descriptor.ExportReportToExcel();

            reportToExcel._Graph       = Base;
            reportToExcel._ShipmentNbr = Base.CurrentDocument.Current.ShipmentNbr;
            reportToExcel.CreateExcelSheet();

            using (MemoryStream ms = new MemoryStream())
            {
                reportToExcel._Workbook.Write(ms);

                string path = string.Format("Invoice_Packinglist_{0}_Aevoe Corp.xlsx", Base.CurrentDocument.Current.GetExtension<SOShipmentExt>().UsrTrackingNbr);
                
                var info = new PX.SM.FileInfo(path, null, ms.ToArray());

                throw new PXRedirectToFileException(info, true);
            }
        }

        public PXMenuItem<SOShipment> createMonthlyInvoice;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Prepare Monthly Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable CreateMonthlyInvoice(PXAdapter adapter)
        {
            var shipments = adapter.Get<SOShipment>().ToList();
            var adapterSlice = (adapter.MassProcess, adapter.AllowRedirect, adapter.QuickProcessFlow, adapter.Arguments);

            Base.Save.Press();

            PXLongOperation.StartOperation(this, delegate ()
            {
                var shipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
                var invoiceEntry  = PXGraph.CreateInstance<SOInvoiceEntry>();

                InvoiceList createdInvoices = new ShipmentInvoices(shipmentEntry);

                if (!adapterSlice.Arguments.TryGetValue(nameof(SOShipmentFilter.InvoiceDate), out object invoiceDate) || invoiceDate == null)
                {
                    invoiceDate = Base.Accessinfo.BusinessDate;
                }

                foreach (SOShipment shipment in shipments)
                {
                    try
                    {
                        shipmentEntry.SelectTimeStamp();
                        invoiceEntry.SelectTimeStamp();

                        if (adapterSlice.MassProcess)
                        {
                            PXProcessing<SOShipment>.SetCurrentItem(shipment);
                        }

                        InvoiceShipment2(invoiceEntry, shipment, (DateTime)invoiceDate, createdInvoices, adapterSlice.QuickProcessFlow);

                        if (adapterSlice.MassProcess)
                        {
                            // shipment is updated and saved somewhere in InvoiceShipment method
                            shipmentEntry.Document.Cache.RestoreCopy(shipment, SOShipment.PK.Find(shipmentEntry, shipment));
                        }
                    }
                    catch (Exception ex) when (adapterSlice.MassProcess)
                    {
                        PXProcessing<SOShipment>.SetError(ex);
                    }
                }

                if (adapterSlice.AllowRedirect && !adapterSlice.MassProcess && createdInvoices.Count > 0)
                {
                    using (new PXTimeStampScope(null))
                    {
                        ARInvoice firstInvoice = createdInvoices[0];
                        invoiceEntry.Clear();
                        invoiceEntry.Document.Current = invoiceEntry.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(firstInvoice.DocType, firstInvoice.RefNbr, firstInvoice.DocType);
                        throw new PXRedirectRequiredException(invoiceEntry, "Invoice");
                    }
                }
            });

            return shipments;
        }
        #endregion

        #region Delegate Function
        public delegate void DelegatePersite();
        [PXOverride]
        public void Persist(SOShipmentEntry_Extension.DelegatePersite baseMethod)
        {
            baseMethod();
            if (this.Base.Document.Current == null)
                return;
            foreach (PXResult<SOPackageDetail> pxResult in PXSelectBase<SOPackageDetail, PXViewOf<SOPackageDetail>.BasedOn<SelectFromBase<SOPackageDetail, TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<SOPackageDetail.shipmentNbr, IBqlString>.IsEqual<P.AsString>>.Aggregate<To<GroupBy<SOPackageDetailExt.usrShipSplitLineNbr>, Sum<SOPackageDetail.qty>>>>.Config>.Select((PXGraph)this.Base, (object)this.Base.Document.Current.ShipmentNbr))
            {
                SOPackageDetail soPackageDetail = (SOPackageDetail)pxResult;
                SOShipLine soShipLine = SOShipLine.PK.Find((PXGraph)this.Base, soPackageDetail.ShipmentNbr, soPackageDetail.GetExtension<SOPackageDetailExt>().UsrShipSplitLineNbr);
                this.Base.Transactions.Cache.SetValue<SOShipLine.packedQty>((object)soShipLine, (object)soPackageDetail.Qty);
                this.Base.Transactions.Cache.Update((object)soShipLine);
            }
            baseMethod();
        }
        #endregion

        #region CacheAttached
        [PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
        [PXDefault(typeof(AccessInfo.contactID), PersistingCheck = PXPersistingCheck.Nothing)]
        protected void _(Events.CacheAttached<SOShipment.ownerID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [Inventory(Visible = true)]
        protected void _(Events.CacheAttached<SOPackageDetailEx.inventoryID> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<SOShipLine> e, PXRowSelected baseHandler)
        {
            if (baseHandler != null)
                baseHandler(e.Cache, e.Args);
            if (e.Row == null)
                return;
            if (this.Base.Document.Current.Status == "N")
            {
                PXCache cache = e.Cache;
                SOShipLine row = e.Row;
                bool? disableShippedQty = this.Base.sosetup.Current.GetExtension<SOSetupExt>().UsrDisableShippedQty;
                bool flag = false;
                int num = disableShippedQty.GetValueOrDefault() == flag & disableShippedQty.HasValue ? 1 : 0;
                PXUIFieldAttribute.SetEnabled<SOShipLine.shippedQty>(cache, (object)row, num != 0);
            }
            SOShipLineExt extension = e.Row.GetExtension<SOShipLineExt>();
            Decimal? shippedQty = e.Row.ShippedQty;
            Decimal num1 = 0M;
            if (shippedQty.GetValueOrDefault() == num1 & shippedQty.HasValue || string.IsNullOrEmpty(extension.UsrCartonQty))
                return;
            PXAction<SOShipLine> autoPacking = this.autoPacking;
            shippedQty = e.Row.ShippedQty;
            Decimal num2 = Convert.ToDecimal(extension.UsrCartonQty);
            int num3 = shippedQty.GetValueOrDefault() >= num2 & shippedQty.HasValue ? 1 : 0;
            autoPacking.SetEnabled(num3 != 0);
        }

        protected void _(Events.RowDeleted<SOPackageDetail> e)
        {
            SOPackageDetail row = e.Row;

            if (row.GetExtension<SOPackageDetailExt>().UsrShipSplitLineNbr != null)
            {
                SOShipLine shipLine = SOShipLine.PK.Find(Base, row.ShipmentNbr, row.GetExtension<SOPackageDetailExt>().UsrShipSplitLineNbr);

                Base.Transactions.Cache.SetValueExt<SOShipLine.packedQty>(shipLine, (shipLine.PackedQty - row.Qty) < 0m ? 0 : shipLine.PackedQty - row.Qty);
                Base.Transactions.Cache.Update(shipLine);
            }
        }

        protected void _(Events.FieldDefaulting<SOPackageDetail.boxID> e)
        {
            e.NewValue = SelectFrom<CSBox>.Where<CSBox.boxID.IsNotNull>.View.Select(Base).TopFirst.BoxID;
        }

        protected void _(Events.FieldUpdated<SOPackageDetail.qty> e)
        {
            SOPackageDetail row = e.Row as SOPackageDetail;

            SOShipLine shipLine = SOShipLine.PK.Find(Base, row.ShipmentNbr, row.GetExtension<SOPackageDetailExt>().UsrShipSplitLineNbr);
            InventoryItem item = InventoryItem.PK.Find(Base, shipLine == null ? row.InventoryID : shipLine.InventoryID);

            if (item != null)
            {
                row.Weight = (row.Qty * item.BaseItemWeight ?? 1) / 1000;
            }
        }

        protected void _(Events.FieldUpdated<SOPackageDetailExt.usrShipSplitLineNbr> e)
        {
            if (!(e.Row is SOPackageDetail row))
                return;
            SOPackageDetailExt extension = row.GetExtension<SOPackageDetailExt>();
            SOShipLine soShipLine = SOShipLine.PK.Find((PXGraph)this.Base, row.ShipmentNbr, extension.UsrShipSplitLineNbr);
            row.InventoryID = soShipLine.InventoryID;
            SOPackageDetail soPackageDetail = row;
            Decimal? qty = row.Qty;
            Decimal num = 0M;
            Decimal? nullable = qty.GetValueOrDefault() == num & qty.HasValue ? soShipLine.ShippedQty : row.Qty;
            soPackageDetail.Qty = nullable;
            this.Base.Packages.Cache.RaiseFieldUpdated<SOPackageDetail.qty>((object)row, (object)row.Qty);
        }

        protected void _(Events.FieldVerifying<SOShipLineExt.usrPackingQty> e)
        {
            if (!(e.Row is SOShipLine row))
                return;
            Decimal? usrRemainingQty = row.GetExtension<SOShipLineExt>().UsrRemainingQty;
            Decimal newValue = (Decimal)e.NewValue;
            if (usrRemainingQty.GetValueOrDefault() < newValue & usrRemainingQty.HasValue)
                throw new PXSetPropertyException<SOShipLineExt.usrPackingQty>(PckQtyExceeded);
        }
        #endregion

        #region Methods
        public virtual string GetUniqueFileNumber() => DateTime.Now.ToString("yyyyMMddHHmmssff") ?? string.Empty;

        public virtual int CalcCommonFactor(int row)
        {
            int num = row - 22;
            return num / 4 >= 1 ? num % 4 : num;
        }

        public virtual ExcelTemplates GetExcelTemplateByCountry(int countShipLine = 0)
        {
            string countryId = PXSelectBase<PX.Objects.GL.Branch, PXViewOf<PX.Objects.GL.Branch>.BasedOn<SelectFromBase<PX.Objects.GL.Branch, TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<PX.Objects.GL.Branch.branchID, IBqlInt>.IsEqual<P.AsInt>>>.ReadOnly.Config>.Select((PXGraph)this.Base, (object)this.Base.Accessinfo.BranchID).TopFirst.CountryID;
            if (countryId == "TW")
                return ExcelTemplates.Taiwan;
            if (countryId == "NL")
                return ExcelTemplates.Europe;
            return countryId == "US" && countShipLine < 57 ? ExcelTemplates.USA1 : ExcelTemplates.USA2;
        }

        public virtual void ExportToExcel(System.Collections.Generic.List<SOShipment> shipments)
        {
            if (shipments == null)
                return;

            Package package = new Package();

            Worksheet sheet = package.Workbook.Sheets[1];

            ExcelTemplates templateByCountry = GetExcelTemplateByCountry();

            int titleRow = 1;
            int row = 2;

            switch (templateByCountry)
            {
                case ExcelTemplates.Taiwan:
                    this.SheetTitle_TW(ref sheet, titleRow);
                    break;
                case ExcelTemplates.Europe:
                    this.SheetTitle_NL(ref sheet, titleRow);
                    break;
                case ExcelTemplates.USA1:
                    this.SheetTitle_US1(ref sheet, titleRow);
                    break;
                    //case ExcelTemplates.USA2:
                    //    this.SheetTitle_US2(ref sheet, titleRow);
                    //    break;
            }

            using (PXTransactionScope transactionScope = new PXTransactionScope())
            {
                for (int index = 0; index < shipments.Count; ++index)
                {
                    SOShipment shipment = shipments[index];
                    SOShipmentContact soShipmentContact = SOShipmentEntry_Extension.SelectShipmentContact((PXGraph)this.Base, shipment.ShipContactID);
                    SOShipmentAddress soShipmentAddress = SOShipmentEntry_Extension.SelectShipmentAddress((PXGraph)this.Base, shipment.ShipAddressID);
                    this.count = 22;
                    foreach (PXResult<SOShipLine> pxResult1 in PXSelectBase<SOShipLine, PXViewOf<SOShipLine>.BasedOn<SelectFromBase<SOShipLine, TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<SOShipLine.shipmentType, Equal<P.AsString>>>>>.And<BqlOperand<SOShipLine.shipmentNbr, IBqlString>.IsEqual<P.AsString>>>>.Config>.Select((PXGraph)this.Base, (object)shipment.ShipmentType, (object)shipment.ShipmentNbr))
                    {
                        SOShipLine soShipLine = (SOShipLine)pxResult1;
                        SOOrder soOrder = SOShipmentEntry_Extension.SelectSOOrder((PXGraph)this.Base, soShipLine.OrigOrderType, soShipLine.OrigOrderNbr);
                        PXResult<PX.Objects.IN.InventoryItem, CSAnswers> pxResult2 = SOShipmentEntry_Extension.RetriveItemAttribute((PXGraph)this.Base, soShipLine.InventoryID);
                        BAccountR baccountR = (BAccountR)PXSelectBase<BAccountR, PXViewOf<BAccountR>.BasedOn<SelectFromBase<BAccountR, TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<BAccountR.bAccountID, IBqlInt>.IsEqual<P.AsInt>>>.Config>.Select((PXGraph)this.Base, (object)soShipLine.CustomerID);
                        switch (templateByCountry)
                        {
                            case ExcelTemplates.Taiwan:
                                sheet.Add(row, 1, soShipLine.ShipmentNbr);
                                sheet.Add(row, 2, soShipLine.OrigOrderNbr);
                                if (shipment.ShipmentType == SOShipmentType.Transfer)
                                {
                                    INSite inSite = INSite.PK.Find((PXGraph)this.Base, shipment.DestinationSiteID);
                                    sheet.Add(row, 3, inSite?.SiteCD);
                                    sheet.Add(row, 4, inSite?.Descr);
                                    sheet.Add(row, 5, soShipmentContact?.FullName);
                                }
                                else
                                {
                                    sheet.Add(row, 3, baccountR.AcctCD);
                                    sheet.Add(row, 4, soShipmentContact?.FullName);
                                    sheet.Add(row, 5, soShipmentContact?.Attention);
                                }
                                sheet.Add(row, 6, soShipmentContact?.Phone1);
                                sheet.Add(row, 7, soShipmentAddress?.AddressLine1 + soShipmentAddress?.AddressLine2);
                                sheet.Add(row, 8, soOrder.OrderDate.Value.ToShortDateString());
                                sheet.Add(row, 9, pxResult2.GetItem<CSAnswers>()?.Value);
                                sheet.Add(row, 10, pxResult2.GetItem<PX.Objects.IN.InventoryItem>().InventoryCD);
                                sheet.Add(row, 11, Convert.ToDouble(Math.Round(soShipLine.ShippedQty.Value, 0)));
                                break;
                            case ExcelTemplates.Europe:
                                sheet.Add(row, 1, soShipLine.ShipmentNbr);
                                sheet.Add(row, 2, soOrder.CustomerOrderNbr);
                                sheet.Add(row, 3, soShipLine.OrigOrderNbr);
                                sheet.Add(row, 4, soShipmentContact?.FullName);
                                sheet.Add(row, 5, soShipmentContact?.Attention);
                                sheet.Add(row, 6, soShipmentContact?.Phone1);
                                sheet.Add(row, 7, soShipmentAddress?.AddressLine1 + soShipmentAddress?.AddressLine2);
                                sheet.Add(row, 8, soShipmentAddress?.PostalCode);
                                sheet.Add(row, 9, soShipmentAddress?.City);
                                sheet.Add(row, 10, soShipmentAddress?.CountryID);
                                sheet.Add(row, 11, soOrder.OrderDate.Value);
                                sheet.Add(row, 12, pxResult2.GetItem<PX.Objects.IN.InventoryItem>().InventoryCD);
                                sheet.Add(row, 13, Convert.ToDouble(Math.Round(soShipLine.ShippedQty.Value, 0)));
                                sheet.Add(row, 14, "FREE");
                                sheet.Add(row, 15, string.Empty);
                                PX.Objects.CR.Standalone.Location location = (PX.Objects.CR.Standalone.Location)PXSelectBase<PX.Objects.CR.Standalone.Location, PXViewOf<PX.Objects.CR.Standalone.Location>.BasedOn<SelectFromBase<PX.Objects.CR.Standalone.Location, TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<PX.Objects.CR.Standalone.Location.bAccountID, Equal<P.AsInt>>>>>.And<BqlOperand<PX.Objects.CR.Standalone.Location.locationID, IBqlInt>.IsEqual<P.AsInt>>>>.Config>.Select((PXGraph)this.Base, (object)soShipLine.CustomerID, (object)shipment.CustomerLocationID);
                                sheet.Add(row, 16, location?.CShipTermsID);
                                sheet.Add(row, 17, soShipmentAddress?.PostalCode);
                                break;
                            case ExcelTemplates.USA1:
                                ++index;
                                sheet.Add(index + titleRow, 1, "1");
                                sheet.Add(index + titleRow, 2, "USLAX01");
                                sheet.Add(index + titleRow, 3, soShipLine.ShipmentNbr);
                                sheet.Add(index + titleRow, 4, shipment.ShipVia != null ? PX.Objects.CS.Carrier.PK.Find((PXGraph)this.Base, shipment.ShipVia).Description : "");
                                sheet.Add(index + titleRow, 5, soOrder.OrderType);
                                sheet.Add(index + titleRow, 10, soShipmentContact?.Attention);
                                sheet.Add(index + titleRow, 11, soShipmentContact?.FullName);
                                sheet.Add(index + titleRow, 12, soShipmentAddress?.CountryID);
                                sheet.Add(index + titleRow, 13, soShipmentAddress?.State);
                                sheet.Add(index + titleRow, 14, soShipmentAddress?.City);
                                sheet.Add(index + titleRow, 15, soShipmentAddress?.AddressLine1);
                                sheet.Add(index + titleRow, 16, soShipmentAddress?.AddressLine2);
                                sheet.Add(index + titleRow, 17, string.Empty);
                                sheet.Add(index + titleRow, 18, soShipmentAddress?.PostalCode);
                                sheet.Add(index + titleRow, 19, soShipmentContact?.Email);
                                sheet.Add(index + titleRow, 20, soShipmentContact?.Phone1);
                                sheet.Add(index + titleRow, 22, soOrder.CustomerOrderNbr);
                                sheet.Add(index + titleRow, ++this.count, pxResult2.GetItem<PX.Objects.IN.InventoryItem>().InventoryCD);
                                sheet.Add(index + titleRow, ++this.count, Convert.ToDouble(Math.Round(soShipLine.ShippedQty.Value, 0)));
                                sheet.Add(index + titleRow, 251, baccountR?.AcctReferenceNbr);
                                --index;
                                this.count += 2;
                                break;
                        }
                        ++row;
                    }
                    Base.Document.SetValueExt<SOShipmentExt.usrPacking>(shipment, true);
                    Base.Document.Cache.MarkUpdated(shipment);
                }

                Base.Actions.PressSave();

                transactionScope.Complete();
            }
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                package.Write((System.IO.Stream)memoryStream);

                throw new PXRedirectToFileException(new PX.SM.FileInfo(string.Format("Stockout_{0}.xlsx", GetUniqueFileNumber()), null, memoryStream.ToArray()), true);
            }
        }

        public virtual void SheetTitle_TW(ref PX.Export.Excel.Core.Worksheet sheet, int titleRow)
        {
            sheet.SetColumnWidth(1, 15.0);
            sheet.SetColumnWidth(2, 15.0);
            sheet.SetColumnWidth(3, 13.0);
            sheet.SetColumnWidth(4, 20.0);
            sheet.SetColumnWidth(5, 19.0);
            sheet.SetColumnWidth(6, 19.0);
            sheet.SetColumnWidth(7, 40.0);
            sheet.SetColumnWidth(8, 20.0);
            sheet.SetColumnWidth(9, 15.0);
            sheet.SetColumnWidth(10, 15.0);
            sheet.SetColumnWidth(11, 12.0);
            sheet.SetColumnWidth(12, 12.0);
            sheet.SetColumnWidth(13, 9.0);
            sheet.SetColumnWidth(14, 6.0);
            sheet.SetColumnWidth(15, 11.0);
            sheet.SetColumnWidth(16, 9.0);
            sheet.SetColumnWidth(17, 11.0);
            sheet.SetColumnWidth(18, 5.0);
            sheet.SetColumnWidth(19, 8.0);
            sheet.Add(titleRow, 1, "Stock out No.");
            sheet.Add(titleRow, 2, "SO No.");
            sheet.Add(titleRow, 3, "Customer No.");
            sheet.Add(titleRow, 4, "Customer Name");
            sheet.Add(titleRow, 5, "Contact person");
            sheet.Add(titleRow, 6, "Tel No.");
            sheet.Add(titleRow, 7, "Address");
            sheet.Add(titleRow, 8, "date");
            sheet.Add(titleRow, 9, "EAN code");
            sheet.Add(titleRow, 10, "Part No.");
            sheet.Add(titleRow, 11, "Quantity");
            sheet.Add(titleRow, 12, "SAMPLE ONLY");
            sheet.Add(titleRow, 13, "PO inside");
            sheet.Add(titleRow, 14, "scan");
            sheet.Add(titleRow, 15, "security tag");
            sheet.Add(titleRow, 16, "price tag");
            sheet.Add(titleRow, 17, "COO sticker");
            sheet.Add(titleRow, 18, "OPP");
            sheet.Add(titleRow, 19, "remark");
            sheet.SetStyle(1, 1, 1, 19, 30);
            sheet.AddBorder(1, 1, 1, 19, StyleSheet.BorderStyles.thin, "Black", StyleSheet.BorderTypes.Bottom);
        }

        public virtual void SheetTitle_NL(ref PX.Export.Excel.Core.Worksheet sheet, int titleRow)
        {
            sheet.SetColumnWidth(1, 15.0);
            sheet.SetColumnWidth(2, 15.0);
            sheet.SetColumnWidth(3, 14.0);
            sheet.SetColumnWidth(4, 20.0);
            sheet.SetColumnWidth(5, 19.0);
            sheet.SetColumnWidth(6, 19.0);
            sheet.SetColumnWidth(7, 40.0);
            sheet.SetColumnWidth(8, 20.0);
            sheet.SetColumnWidth(9, 15.0);
            sheet.SetColumnWidth(10, 15.0);
            sheet.SetColumnWidth(11, 12.0);
            sheet.SetColumnWidth(12, 12.0);
            sheet.SetColumnWidth(13, 9.0);
            sheet.SetColumnWidth(14, 8.0);
            sheet.SetColumnWidth(15, 11.0);
            sheet.SetColumnWidth(16, 9.0);
            sheet.SetColumnWidth(17, 11.0);
            sheet.Add(titleRow, 1, "SO No.");
            sheet.Add(titleRow, 2, "PO No.");
            sheet.Add(titleRow, 3, "Stock out No.");
            sheet.Add(titleRow, 4, "Customer Name");
            sheet.Add(titleRow, 5, "Contact person");
            sheet.Add(titleRow, 6, "Tel No.");
            sheet.Add(titleRow, 7, "Street");
            sheet.Add(titleRow, 8, "Postal code");
            sheet.Add(titleRow, 9, "City");
            sheet.Add(titleRow, 10, "COUNTRY");
            sheet.Add(titleRow, 11, "date");
            sheet.Add(titleRow, 12, "Part No.");
            sheet.Add(titleRow, 13, "Quantity");
            sheet.Add(titleRow, 14, "Stor.Loc.");
            sheet.Add(titleRow, 15, "TEXT");
            sheet.Add(titleRow, 16, "INCO1");
            sheet.Add(titleRow, 17, "INCO2");
            sheet.SetStyle(1, 1, 1, 17, 30);
            sheet.AddBorder(1, 1, 1, 17, StyleSheet.BorderStyles.thin, "Black", StyleSheet.BorderTypes.Bottom);
        }

        public virtual void SheetTitle_US1(ref PX.Export.Excel.Core.Worksheet sheet, int titleRow)
        {
            sheet.SetColumnWidth(1, 15.0);
            sheet.SetColumnWidth(2, 23.0);
            sheet.SetColumnWidth(3, 23.0);
            sheet.SetColumnWidth(4, 23.0);
            sheet.SetColumnWidth(5, 23.0);
            sheet.SetColumnWidth(6, 22.0);
            sheet.SetColumnWidth(7, 10.0);
            sheet.SetColumnWidth(8, 20.0);
            sheet.SetColumnWidth(9, 15.0);
            sheet.SetColumnWidth(10, 24.0);
            sheet.SetColumnWidth(11, 27.0);
            sheet.SetColumnWidth(12, 25.0);
            sheet.SetColumnWidth(13, 10.0);
            sheet.SetColumnWidth(14, 8.0);
            sheet.SetColumnWidth(15, 11.0);
            sheet.SetColumnWidth(16, 12.0);
            sheet.SetColumnWidth(17, 15.0);
            sheet.SetColumnWidth(18, 12.0);
            sheet.SetColumnWidth(19, 25.0);
            sheet.SetColumnWidth(20, 24.0);
            sheet.SetColumnWidth(21, 12.0);
            sheet.SetColumnWidth(22, 16.0);
            for (int column = 23; column <= 246; ++column)
                sheet.SetColumnWidth(column, 30.0);
            sheet.SetColumnWidth(247, 25.0);
            sheet.SetColumnWidth(248, 25.0);
            sheet.SetColumnWidth(249, 25.0);
            sheet.SetColumnWidth(250, 25.0);
            sheet.SetColumnWidth(251, 25.0);
            sheet.Add(titleRow, 1, "自动分配仓库");
            sheet.Add(titleRow, 2, "仓库代码/Warehouse Code");
            sheet.Add(titleRow, 3, "参考编号/Reference Code");
            sheet.Add(titleRow, 4, "派送方式/Delivery Style");
            sheet.Add(titleRow, 5, "销售平台/Sales Platform");
            sheet.Add(titleRow, 6, "跟踪号/Tracking number");
            sheet.Add(titleRow, 7, "COD Value");
            sheet.Add(titleRow, 8, "币种/Currency");
            sheet.Add(titleRow, 9, "年龄/Age");
            sheet.Add(titleRow, 10, "收件人姓名/Consignee Name");
            sheet.Add(titleRow, 11, "收件人公司/Consignee Company");
            sheet.Add(titleRow, 12, "收件人国家/Consignee Country");
            sheet.Add(titleRow, 13, "州/Province");
            sheet.Add(titleRow, 14, "城市/City");
            sheet.Add(titleRow, 15, "街道/Street");
            sheet.Add(titleRow, 16, "街道2/Street2");
            sheet.Add(titleRow, 17, "门牌号/Doorplate");
            sheet.Add(titleRow, 18, "邮编/Zip Code");
            sheet.Add(titleRow, 19, "收件人Email/Consignee Email");
            sheet.Add(titleRow, 20, "收件人电话/Consignee Phone");
            sheet.Add(titleRow, 22, "备注 / Remark");
            this.count = 1;
            for (int index = 23; index <= 246; ++index)
            {
                switch (this.CalcCommonFactor(index))
                {
                    case 0:
                        sheet.Add(titleRow, index, string.Format("申报价值/Declared Value  {0}", (object)this.count));
                        ++this.count;
                        break;
                    case 1:
                        sheet.Add(titleRow, index, string.Format("SKU{0}", (object)this.count));
                        break;
                    case 2:
                        sheet.Add(titleRow, index, string.Format("数量{0}/Quantity {0}", (object)this.count));
                        break;
                    case 3:
                        sheet.Add(titleRow, index, string.Format("英文申报名称/Product Name En {0}", (object)this.count));
                        break;
                }
            }
            sheet.Add(titleRow, 247, "保险服务/Insurance");
            sheet.Add(titleRow, 248, "投保金额/Insurance Amount");
            sheet.Add(titleRow, 249, "签名服务/Signature");
            sheet.Add(titleRow, 250, "平台店铺/Platform Shop");
            sheet.Add(titleRow, 251, "买家ID/Buyers Id");
            sheet.SetStyle(1, 1, 1, 251, 30);
            sheet.AddBorder(1, 1, 1, 251, StyleSheet.BorderStyles.thin, "Black", StyleSheet.BorderTypes.Bottom);
        }

        public virtual void VerifyCustomerMailInfo(string reportID)
        {
            var customer = Base.customer.Current;

            if (string.IsNullOrEmpty(SelectFrom<Contact>.Where<Contact.contactID.IsEqual<@P.AsInt>
                                                               .And<Contact.bAccountID.IsEqual<@P.AsInt>>>.View.Select(Base, customer.DefContactID, customer.BAccountID).TopFirst.EMail) &&
                fileExt != PX.Objects.CN.Common.Descriptor.Constants.ExcelFileExtension)
            {
                throw new PXException(NoCustContAddr, Base.customer.Current.AcctCD);
            }
            if (SelectFrom<NotificationSource>.Where<NotificationSource.refNoteID.IsEqual<@P.AsGuid>
                                                     .And<NotificationSource.reportID.IsEqual<@P.AsString>>>.View.Select(Base, customer.NoteID, reportID).Count == 0)
            {
                CreateCRMailingSetting();
                //throw new PXException(NonNotifSrc, Base.customer.Current.AcctCD);
            }
        }

        public virtual void CreateCRMailingSetting()
        {
            CustomerMaint maint = PXGraph.CreateInstance<CustomerMaint>();

            foreach (NotificationSetup setup in SelectFrom<NotificationSetup>.Where<NotificationSetup.reportID.IsIn<@P.AsString, @P.AsString>>.View.Select(Base, CommInvRpt, CommInvRpt2))
            {
                NotificationSource source = maint.NotificationSources.Cache.CreateInstance() as NotificationSource;

                source.SetupID = setup.SetupID;
               
                source = maint.NotificationSources.Insert(source);

                source.ReportID           = setup.ReportID;
                source.Format             = setup.Format;
                source.Active             = setup.Active;
                source.RecipientsBehavior = setup.RecipientsBehavior;

                maint.NotificationSources.Update(source);
            }

            maint.CurrentCustomer.Current = Base.customer.Current;

            maint.Actions.PressSave();
        }

        /// <summary>
        /// Copy from SOShipmentEntr InvoiceShipment() and only replace standard method [InvoiceOrder] with custom method [InvoiceOrder2].
        /// </summary>
        public virtual void InvoiceShipment2(SOInvoiceEntry docgraph, SOShipment shiporder, DateTime invoiceDate, InvoiceList list, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            Base.Clear();

            Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
            Base.Document.Current.Status = shiporder.Status;

            Base.Document.Cache.MarkUpdated(Base.Document.Current);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                Base.Save.Press();

                foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, 
                                  SOAddress, SOContact, SOOrderType> order in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, 
                                                                                                                                  And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
                                                                                                            InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
                                                                                                            InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
                                                                                                            InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
                                                                                                            InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
                                                                                                            InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
                                                                                                                                               And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>>>>>,
                                                                                                            Where<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>,
                                                                                                                  And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
                                                                                                                      And<SOOrderShipment.invoiceNbr, IsNull>>>>.Select(Base))
                {
                    ((SOOrderShipment)order).BillShipmentSeparately = shiporder.BillSeparately;

                    docgraph.Clear();
                    docgraph.ARSetup.Current.RequireControlTotal = false;

                    SOInvoiceEntry_Extension graphExt = docgraph.GetExtension<SOInvoiceEntry_Extension>();

                    var shipmentInvoices = list as ShipmentInvoices;

                    if (shipmentInvoices != null)
                    {
                        var orderType = SOOrderType.PK.Find(Base, ((SOOrder)order).OrderType);
                        var docType = docgraph.GetInvoiceDocType(orderType, ((SOOrderShipment)order).Operation);
                        var subList = new InvoiceList(docgraph);

                        subList.AddRange(shipmentInvoices.GetInvoices(docType));

                        int oldCount = subList.Count;

                        graphExt.InvoiceOrder2(invoiceDate, order, Base.customer.Current, subList, quickProcessFlow);

                        if (subList.Count > oldCount)
                        {
                            list.Add((ARInvoice)subList[oldCount], (SOInvoice)subList[oldCount], (CurrencyInfo)subList[oldCount][typeof(CurrencyInfo)]);
                        }
                    }
                    else
                    {
                        graphExt.InvoiceOrder2(invoiceDate, order, Base.customer.Current, list, quickProcessFlow);
                    }
                }

                ts.Complete();
            }
        }
        #endregion

        #region Static Methods
        protected static SOShipmentContact SelectShipmentContact(PXGraph graph, int? contactID)
        {
            return SelectFrom<SOShipmentContact>.Where<SOShipmentContact.contactID.IsEqual<@P.AsInt>>.View.SelectSingleBound(graph, null, contactID);
        }

        protected static SOShipmentAddress SelectShipmentAddress(PXGraph graph, int? addressID)
        {
            return SelectFrom<SOShipmentAddress>.Where<SOShipmentAddress.addressID.IsEqual<@P.AsInt>>.View.SelectSingleBound(graph, null, addressID);
        }

        protected static SOOrder SelectSOOrder(PXGraph graph, string orderType, string orderNbr)
        {
            return SelectFrom<SOOrder>.Where<SOOrder.orderType.IsEqual<@P.AsString>
                                            .And<SOOrder.orderNbr.IsEqual<@P.AsString>>>.View.Select(graph, orderType, orderNbr);
        }

        protected static PXResult<InventoryItem, CSAnswers> RetriveItemAttribute(PXGraph graph, int? inventoryID)
        {
            return (PXResult<InventoryItem, CSAnswers>)SelectFrom<InventoryItem>.LeftJoin<CSAnswers>.On<CSAnswers.refNoteID.IsEqual<InventoryItem.noteID>
                                                                                                        .And<CSAnswers.attributeID.StartsWith<EAN13Attr>>>
                                                                                .Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(graph, inventoryID);
        }

        protected static void PackageCreation(string buttonType, SOShipLine shipLine)
        {
            SOShipmentEntry graph = PXGraph.CreateInstance<SOShipmentEntry>();

            SOPackageDetail packageDetail = SelectFrom<SOPackageDetail>.Where<SOPackageDetail.shipmentNbr.IsEqual<@P.AsString>>
                                                                       .AggregateTo<Max<SOPackageDetail.customRefNbr1,
                                                                                        Count<SOPackageDetail.lineNbr>>>.View.ReadOnly.SelectSingleBound(graph, null, shipLine.ShipmentNbr);
            switch (buttonType)
            {
                case SOPackageType.Auto:
                    int num = (int)(shipLine.ShippedQty.Value / Convert.ToDecimal(shipLine.GetExtension<SOShipLineExt>().UsrCartonQty));

                    for (int i = 1; i <= num; i++)
                    {
                        SOPackageDetailEx newRow = new SOPackageDetailEx();

                        int customNbr = Convert.ToInt32(packageDetail.CustomRefNbr1) + i;

                        newRow.CustomRefNbr1 = customNbr.ToString().PadLeft(3, '0');
                        newRow.ShipmentNbr = shipLine.ShipmentNbr;
                        newRow.LineNbr = packageDetail.LineNbr + i;
                        newRow.Qty = Convert.ToDecimal(shipLine.GetExtension<SOShipLineExt>().UsrCartonQty);

                        newRow = graph.Packages.Insert(newRow);

                        graph.Packages.Cache.SetValueExt<SOPackageDetailExt.usrShipSplitLineNbr>(newRow, shipLine.LineNbr);
                        graph.Packages.Update(newRow);
                    }
                    break;
                case SOPackageType.Manual:
                    int count = 1;
                    int custNbr = Convert.ToInt32(packageDetail.CustomRefNbr1) + 1;

                    foreach (SOShipLine row in SelectFrom<SOShipLine>.Where<SOShipLine.shipmentType.IsEqual<P.AsString>
                                                                            .And<SOShipLine.shipmentNbr.IsEqual<P.AsString>
                                                                                 .And<SOShipLineExt.usrPackingQty.IsNotEqual<decimal0>>>>.View.Select(graph, shipLine.ShipmentType, shipLine.ShipmentNbr))
                    {
                        SOPackageDetailEx newPackage = new SOPackageDetailEx();

                        newPackage.CustomRefNbr1 = custNbr.ToString().PadLeft(3, '0');
                        newPackage.ShipmentNbr = row.ShipmentNbr;
                        newPackage.LineNbr = packageDetail.LineNbr + count++;
                        newPackage.Qty = row.GetExtension<SOShipLineExt>().UsrPackingQty;

                        newPackage = graph.Packages.Insert(newPackage);

                        graph.Packages.Cache.SetValueExt<SOPackageDetailExt.usrShipSplitLineNbr>(newPackage, row.LineNbr);
                        graph.Packages.Update(newPackage);

                        graph.Transactions.Cache.SetValue<SOShipLineExt.usrPackingQty>(row, 0m);
                        graph.Transactions.Cache.MarkUpdated(row);

                    }
                    break;
            }

            graph.Actions.PressSave();
        }

        public static string GetCarrierTrackURL(string carrier, string trackNbr)
        {
            string uRL = string.Empty;

            switch (carrier)
            {
                case "DHL":
                    uRL = string.Format("http://www.dhl.com.tw/en/express/tracking.html?AWB={0}&brand=DHL", trackNbr);
                    break;
                case "FEDEX":
                    uRL = string.Format("https://www.fedex.com/fedextrack/?trknbr={0}", trackNbr);
                    break;
                default:
                    uRL = string.Format("https://www.ups.com/track?loc=en_tw&tracknum={0}&requester=WT/trackdetails", trackNbr);
                    break;
            }

            return uRL;
        }
        #endregion

        #region enum
        public enum ExcelTemplates
        {
            Taiwan, Europe, USA1, USA2,
        }

        public enum SendDestination
        {
            Customer, UPS
        }
        #endregion
    }

    #region Inherited Class
    public class SOShipmentScreenExt : SOInvoiceShipment.WellKnownActions.SOShipmentScreen
    {
        public const string PrintStockoutForm = "SO302000$printStockoutForm";
        public const string SendToCustomer    = "SO302000$emailToCustomer";
        public const string SendToUPS         = "SO302000$emailToUPS";
        public const string CreateMonInv      = "SO302000$createMonthlyInvoice";
    }
    #endregion
}
