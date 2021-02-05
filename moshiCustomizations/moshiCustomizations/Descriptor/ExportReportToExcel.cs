using System;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;

namespace moshiCustomizations.Descriptor
{
    public class ExcelHelper
    {
        public void CreateBlankCell(ISheet sheet, int rowNum, int startCell, int endCell, ICellStyle style = null)
        {
            for (int i = startCell; i <= endCell; i++)
            {
                sheet.GetRow(rowNum).CreateCell(i);
                if (style != null)
                    sheet.GetRow(rowNum).GetCell(i).CellStyle = style;
            }
        }

        public void AutoSizeColumn(ISheet sheet, int rowOffset)
        {
            int numberOfColum = sheet.GetRow(rowOffset).PhysicalNumberOfCells;
            for (int i = 1; i <= numberOfColum; i++)
            {
                sheet.AutoSizeColumn(i);
                GC.Collect();
            }
        }

    }

    public class ExcelStyle
    {
        protected IWorkbook _workBook;
        protected ICellStyle _style;
        protected IFont _font;

        public ExcelStyle(XSSFWorkbook workBook)
        {
            this._workBook = workBook;
            this._style = this._workBook.CreateCellStyle();
            this._font = this._workBook.CreateFont();
        }

        public ExcelStyle(HSSFWorkbook workBook)
        {
            this._workBook = workBook;
            this._style = this._workBook.CreateCellStyle();
            this._font = this._workBook.CreateFont();
        }

        /// <summary>
        /// Alignment = HorizontalAlignment.Center;
        /// VerticalAlignment = VerticalAlignment.Center
        /// FontName = Microsoft YaHei UI
        /// FontHeightInPoints = 14(Default)
        /// Boldweight = FontBoldWeight.Bold(Default: false)
        /// </summary>
        public virtual (ICellStyle ExcelStyle, IFont ExcelFont) HeaderStyle(int FontSize = 14, bool Bold = true, bool Border = false)
        {
            this._style.Alignment = HorizontalAlignment.Center;
            this._style.VerticalAlignment = VerticalAlignment.Center;
            this._font.FontName = "Microsoft YaHei UI";
            this._font.FontHeightInPoints = FontSize;
            this._style.SetFont(this._font);

            if (Bold) { this._font.Boldweight = (short)FontBoldWeight.Bold; }
            if (Border) { this._style.BorderBottom = BorderStyle.Thin; }

            return (_style, _font);
        }

        /// <summary>
        /// Alignment = Parameter
        /// VerticalAlignment = VerticalAlignment.Center
        /// FontName = "Microsoft YaHei UI"
        /// FontHeightInPoints = 12(Default)
        /// </summary>
        public virtual (ICellStyle ExcelStyle, IFont ExcelFont) NormalStyle(int FontSize = 12, bool Bold = false, bool Border = false, HorizontalAlignment Alignment = HorizontalAlignment.Center)
        {
            this._style.Alignment = Alignment;
            this._style.VerticalAlignment = VerticalAlignment.Center;
            this._font.FontName = "Microsoft YaHei UI";
            this._font.FontHeightInPoints = FontSize;
            this._style.SetFont(this._font);

            if (Bold)   { this._font.Boldweight = (short)FontBoldWeight.Bold; }                
            if (Border) { this._style = AddBorder(_style); }
                
            return (_style, _font);
        }

        /// <summary>
        /// Alignment = HorizontalAlignment.Center
        /// VerticalAlignment = VerticalAlignment.Center
        /// FontName = "Microsoft YaHei UI"
        /// FontHeightInPoints = 12(Default)
        /// </summary>
        public virtual (ICellStyle ExcelStyle, IFont ExcelFont) TableHeaderStyle(int FontSize = 12, bool Bold = false, bool Border = false)
        {
            this._style.Alignment = HorizontalAlignment.Center;
            this._style.VerticalAlignment = VerticalAlignment.Center;
            this._font.FontName = "Microsoft YaHei UI";
            this._font.FontHeightInPoints = FontSize;
            this._style.SetFont(this._font);
            if (Bold)
                this._font.Boldweight = (short)FontBoldWeight.Bold;
            if (Border)
                this._style = AddBorder(_style);
            return (_style, _font);
        }

        /// <summary>
        /// WrapText = true
        /// Alignment = paramter
        /// VerticalAlignment = VerticalAlignment.Center
        /// FontName = "Microsoft YaHei UI"
        /// FontHeightInPoints = 12(Default)
        /// StyleBorder = parameter
        /// </summary>
        public virtual (ICellStyle ExcelStyle, IFont ExcelFont) WrapTextStyle(int FontSize = 12, bool Border = false, HorizontalAlignment Alignment = HorizontalAlignment.Center)
        {
            this._style.WrapText = true;
            this._style.Alignment = Alignment;
            this._style.VerticalAlignment = VerticalAlignment.Center;
            this._font.FontName = "Microsoft YaHei UI";
            this._font.FontHeightInPoints = FontSize;
            this._style.SetFont(this._font);

            if (Border) { this._style = AddBorder(_style); }

            return (_style, _font);
        }

        /// <summary> 
        /// Set Cell Color(IndexedColors.Color) 
        /// </summary>
        public virtual (ICellStyle ExcelStyle, IFont ExcelFont) SetCellColor(short colorIndex, ICellStyle myStyle = null, bool manulFont = false)
        {
            this._style = this._workBook.CreateCellStyle();
            if (myStyle != null)
                this._style = myStyle;
            this._style.FillForegroundColor = colorIndex;
            this._style.FillPattern = FillPattern.SolidForeground;

            if (manulFont == true)
            {
                this._font.Boldweight = (short)FontBoldWeight.Bold;
                this._font.FontName = "Microsoft YaHei UI";
                this._font.FontHeightInPoints = 12;
                this._font.Color = IndexedColors.White.Index;
            }

            this._style.SetFont(this._font);
            
            return (_style, _font);
        }

        public ICellStyle AddBorder(ICellStyle style)
        {
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            return style;
        }

    }

    public class ExportReportToExcel
    {
        public PXGraph _Graph;
        public XSSFWorkbook _Workbook;

        public int _RowCount;
        public string _ShipmentNbr;

        public void CreateExcelSheet()
        {
            XSSFWorkbook workBook = new XSSFWorkbook();
            ExcelHelper excelHelper = new ExcelHelper();

            #region Style Definitions
            ICellStyle headerStyle              = new ExcelStyle(workBook).HeaderStyle().ExcelStyle;
            ICellStyle normal_Center            = new ExcelStyle(workBook).NormalStyle().ExcelStyle;
            ICellStyle normal_Left              = new ExcelStyle(workBook).NormalStyle(12, false, false, HorizontalAlignment.Left).ExcelStyle;
            ICellStyle normal_Center_Bold_Border= new ExcelStyle(workBook).NormalStyle(12, true, true).ExcelStyle;
            ICellStyle normal_Left_Bold_Border  = new ExcelStyle(workBook).NormalStyle(12, true, true, HorizontalAlignment.Left).ExcelStyle;
            ICellStyle normal_Right_Bold_Border = new ExcelStyle(workBook).NormalStyle(12, true, true, HorizontalAlignment.Right).ExcelStyle;
            ICellStyle normal_Left_Bold         = new ExcelStyle(workBook).NormalStyle(12, true, false, HorizontalAlignment.Left).ExcelStyle;
            ICellStyle normal_Right_Bold        = new ExcelStyle(workBook).NormalStyle(12, true, false, HorizontalAlignment.Right).ExcelStyle;
            ICellStyle normal_Center_Border     = new ExcelStyle(workBook).NormalStyle(12, false, true).ExcelStyle;
            ICellStyle normal_Left_Border       = new ExcelStyle(workBook).NormalStyle(12, false, true, HorizontalAlignment.Left).ExcelStyle;
            ICellStyle normal_Right_Border      = new ExcelStyle(workBook).NormalStyle(12, false, true, HorizontalAlignment.Right).ExcelStyle;
            ICellStyle wrapText_Center_Border   = new ExcelStyle(workBook).WrapTextStyle(12, true).ExcelStyle;
            ICellStyle wrapText_Left            = new ExcelStyle(workBook).WrapTextStyle(12, false, HorizontalAlignment.Left).ExcelStyle;
            ICellStyle tableHeader_Bold_Border  = new ExcelStyle(workBook).TableHeaderStyle(12, true, true).ExcelStyle;
            ICellStyle tableContent_Border      = new ExcelStyle(workBook).TableHeaderStyle(11, false, true).ExcelStyle;

            ICellStyle normal_BorderBottom = workBook.CreateCellStyle();
            normal_BorderBottom.CloneStyleFrom(normal_Center);
            normal_BorderBottom.BorderTop = BorderStyle.None;
            normal_BorderBottom.BorderLeft = BorderStyle.None;
            normal_BorderBottom.BorderRight = BorderStyle.None;
            normal_BorderBottom.BorderBottom = BorderStyle.Thin;

            // Black Cell Style
            var blackCellStyle = new ExcelStyle(workBook).NormalStyle().ExcelStyle;
            blackCellStyle.Alignment = HorizontalAlignment.Center;
            new ExcelStyle(workBook).SetCellColor(IndexedColors.Black.Index, blackCellStyle, true);
            #endregion

            var bAccount = PX.Objects.CR.BAccount2.PK.Find(_Graph, PX.Objects.GL.Branch.PK.Find(_Graph, _Graph.Accessinfo.BranchID).BAccountID.Value);

            ISheet sheet = workBook.CreateSheet("Sheet1");

            ReSize(ref sheet);

            #region Report Info
            _RowCount = 1;
            Header_PackingList(ref sheet, bAccount, headerStyle, wrapText_Left, normal_Left_Bold, normal_Right_Bold, normal_Left, blackCellStyle);
            Details_PackingList(ref sheet, normal_Center_Border, normal_Left_Border, wrapText_Center_Border, normal_Right_Border, normal_Left_Bold, normal_Right_Bold, normal_BorderBottom);
            #endregion

            #region Logo Image
            byte[] data = GetImageBytes(_Graph, bAccount.NoteID);

            int pictureIndex = workBook.AddPicture(data, PictureType.PNG);

            ICreationHelper helper = workBook.GetCreationHelper();
            IDrawing drawing = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = helper.CreateClientAnchor();

            anchor.Col1 = 1;
            anchor.Row1 = 1;
            IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
            picture.Resize(2.8, 1);
            #endregion

            _Workbook = workBook;

            // Update Excel Formula
            sheet.ForceFormulaRecalculation = true;
            sheet.FitToPage = true;
        }

        public void ReSize(ref ISheet sheet)
        {
            sheet.SetColumnWidth(0, 10 * 256);
            sheet.SetColumnWidth(1, 15 * 256);
            sheet.SetColumnWidth(2, 16 * 256);
            sheet.SetColumnWidth(3, 55 * 256);
            sheet.SetColumnWidth(4, 16 * 256);
            sheet.SetColumnWidth(5, 15 * 256);
            sheet.SetColumnWidth(6, 15 * 256);
        }

        public void Header_PackingList(ref ISheet sheet, BAccount2 bAccount, ICellStyle headerStyle, ICellStyle wrapText_Left, ICellStyle normal_Left_Bold, ICellStyle normal_Right_Bold, ICellStyle normal_Left, ICellStyle blackCellStyle)
        {
            var shipment   = SOShipment.PK.Find(_Graph, _ShipmentNbr);
            var customer   = Customer.PK.Find(_Graph, shipment.CustomerID);
            var branchAddr = Address.PK.Find(_Graph, bAccount.DefAddressID);
            var branchCont = Contact.PK.Find(_Graph, bAccount.DefContactID);

            sheet.CreateRow(_RowCount);
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue(string.Format("{0}\n{1}\n{2}{3}{4}{5}{6}\nTEL: {7} FAX: {8}", bAccount.AcctName, branchAddr.AddressLine1, 
                                                                                                                             (branchAddr.AddressLine2 == null) ? string.Empty : branchAddr.AddressLine2 + ", ", 
                                                                                                                             (branchAddr.City == null) ? string.Empty : branchAddr.City + ", ", 
                                                                                                                             (branchAddr.State == null) ? string.Empty : branchAddr.State + ", ", 
                                                                                                                             (branchAddr.PostalCode == null) ? string.Empty : branchAddr.PostalCode + ", ", 
                                                                                                                             branchAddr.CountryID, branchCont.Phone1, branchCont.Fax));
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = wrapText_Left;
            sheet.GetRow(_RowCount).Height = 1500; // Row height = 75
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 4, 6));

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("PACKING LIST");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = headerStyle;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 0, 6));

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("INVOICE INFORMATION");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = blackCellStyle;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 0, 6));

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("BILL TO");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Left_Bold;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 0, 2));
            sheet.GetRow(_RowCount).CreateCell(3).SetCellValue("SHIP TO");
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = normal_Left_Bold;
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue("INVOICE #:");
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Bold;
            sheet.GetRow(_RowCount).CreateCell(5).SetCellValue(shipment.ShipmentNbr);
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Left_Bold;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 5, 6));

            var billingCont = Contact.PK.Find(_Graph, customer.DefBillContactID);
            var billingAddr = Address.PK.Find(_Graph, customer.DefBillAddressID);

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue(string.Format("{0}\n{1}{2}{3}{4}{5}{6}", billingCont.FullName, 
                                                                                                        billingCont.Attention?.Insert(billingCont.Attention.Length, "\n"),
                                                                                                        billingAddr.AddressLine1,
                                                                                                        (billingAddr.AddressLine2 == null) ? string.Empty : billingAddr.AddressLine2 + ", ",
                                                                                                        (billingAddr.City == null) ? string.Empty : billingAddr.City + ", ",
                                                                                                        (billingAddr.State == null) ? string.Empty : billingAddr.State + ", ",
                                                                                                        (billingAddr.PostalCode == null) ? string.Empty : billingAddr.PostalCode + ", ",
                                                                                                        billingAddr.CountryID));
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = wrapText_Left;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, 8, 0, 2));

            var shippingCont = SOContact.PK.Find(_Graph, shipment.ShipContactID);
            var shippingAddr = SOAddress.PK.Find(_Graph, shipment.ShipAddressID);

            sheet.GetRow(_RowCount).CreateCell(3).SetCellValue(string.Format("{0}\n{1}{2}{3}{4}{5}{6}", shippingCont.FullName,
                                                                                                        shippingCont.Attention.Insert(shippingCont.Attention.Length, "\n"),
                                                                                                        shippingAddr.AddressLine1,
                                                                                                        (shippingAddr.AddressLine2 == null) ? string.Empty : shippingAddr.AddressLine2 + ", ",
                                                                                                        (shippingAddr.City == null) ? string.Empty : shippingAddr.City + ", ",
                                                                                                        (shippingAddr.State == null) ? string.Empty : shippingAddr.State + ", ",
                                                                                                        (shippingAddr.PostalCode == null) ? string.Empty : shippingAddr.PostalCode + ", ",
                                                                                                        shippingAddr.CountryID));
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = wrapText_Left;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, 8, 3, 3));

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue("INVOICE DATE:");
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Bold;
            sheet.GetRow(_RowCount).CreateCell(5).SetCellValue(shipment.ShipDate.Value.ToString("yyyy/MM/dd"));
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Left_Bold;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 5, 6));

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue("ETA");
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Bold;

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue("CUSTOMER PO:");
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Bold;
            var sOOrder = (SOOrder)SelectFrom<SOOrder>.InnerJoin<SOOrderShipment>.On<SOOrderShipment.orderType.IsEqual<SOOrder.orderType>
                                                                                     .And<SOOrderShipment.orderNbr.IsEqual<SOOrder.orderNbr>>>
                                                                                  .Where<SOOrderShipment.shipmentType.IsEqual<@P.AsString>
                                                                                         .And<SOOrderShipment.shipmentNbr.IsEqual<@P.AsString>>>
                                                                                  .View.SelectSingleBound(_Graph, null, shipment.ShipmentType, shipment.ShipmentNbr);
            sheet.GetRow(_RowCount).CreateCell(5).SetCellValue(sOOrder.CustomerOrderNbr);
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Left_Bold;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 5, 6));

            _RowCount = 9;
            sheet.CreateRow(_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("ORDER DETAILS");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = blackCellStyle;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 0, 6));
            _RowCount++;
        }

        public void Details_PackingList(ref ISheet sheet, ICellStyle normal_Center_Border, ICellStyle normal_Left_Border, ICellStyle wrapText_Center_Border, ICellStyle normal_Right_Border, 
                                                          ICellStyle normal_Left_Bold, ICellStyle normal_Right_Bold, ICellStyle normal_BorderBottom)
        {
            decimal? totalNW = 0m;
            decimal? totalGW = 0m;
            string preCustNbr1 = null;

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("CTN NO.");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(1).SetCellValue("PART #");
            sheet.GetRow(_RowCount).GetCell(1).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(2).SetCellValue("EAN");
            sheet.GetRow(_RowCount).GetCell(2).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(3).SetCellValue("PRODUCT DESCRIPTION");
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(4).SetCellValue("QTY.");
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(5).SetCellValue("N.W.(KG)");
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Center_Border;
            sheet.GetRow(_RowCount).CreateCell(6).SetCellValue("G.W.(KG)");
            sheet.GetRow(_RowCount).GetCell(6).CellStyle = normal_Center_Border;

            foreach (PXResult<SOPackageDetail, InventoryItem> res in SelectFrom<SOPackageDetail>.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<SOPackageDetail.inventoryID>>
                                                                                                .Where<SOPackageDetail.shipmentNbr.IsEqual<@P.AsString>>
                                                                                                .OrderBy<Asc<SOPackageDetail.shipmentNbr, Asc<SOPackageDetail.customRefNbr1>>>.View.Select(_Graph, _ShipmentNbr) )
            {
                SOPackageDetail package = res;
                InventoryItem   item = res;

                sheet.CreateRow(++_RowCount);
                sheet.GetRow(_RowCount).CreateCell(0).SetCellValue(package.CustomRefNbr1);
                sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Left_Border;
                sheet.GetRow(_RowCount).CreateCell(1).SetCellValue(item.InventoryCD);
                sheet.GetRow(_RowCount).GetCell(1).CellStyle = normal_Left_Border;
                sheet.GetRow(_RowCount).CreateCell(2).SetCellValue(CSAnswers.PK.Find(_Graph, item.NoteID, "EAN13").Value ?? string.Empty);
                sheet.GetRow(_RowCount).GetCell(2).CellStyle = normal_Left_Border;
                sheet.GetRow(_RowCount).CreateCell(3).SetCellValue(string.Format("{0}\n{1}", item.Descr, CSAnswers.PK.Find(_Graph, item.NoteID, "ITEMDEC").Value ?? string.Empty) );
                sheet.GetRow(_RowCount).GetCell(3).CellStyle = wrapText_Center_Border;
                sheet.GetRow(_RowCount).CreateCell(4).SetCellValue((double)Math.Round(package.Qty.Value, 2));
                sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Border;
                sheet.GetRow(_RowCount).CreateCell(5).SetCellValue((double)Math.Round(package.Weight.Value, 2));
                sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Right_Border;
                sheet.GetRow(_RowCount).CreateCell(6).SetCellValue(preCustNbr1 == package.CustomRefNbr1 ? (double)package.Weight : (double)package.Weight + 1);
                sheet.GetRow(_RowCount).GetCell(6).CellStyle = normal_Right_Border;

                totalNW += package.Weight;
                totalGW += preCustNbr1 == package.CustomRefNbr1 ? package.Weight : package.Weight + 1;
                preCustNbr1 = package.CustomRefNbr1;
            }

            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0);
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Right_Border;
            sheet.GetRow(_RowCount).CreateCell(1);
            sheet.GetRow(_RowCount).GetCell(1).CellStyle = normal_Right_Border;
            sheet.GetRow(_RowCount).CreateCell(2);
            sheet.GetRow(_RowCount).GetCell(2).CellStyle = normal_Right_Border;
            sheet.GetRow(_RowCount).CreateCell(3);
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = normal_Right_Border;
            sheet.GetRow(_RowCount).CreateCell(4);
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_Right_Border;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 0, 4));
            sheet.GetRow(_RowCount).CreateCell(5).SetCellValue((double)totalNW);
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_Right_Border;
            sheet.GetRow(_RowCount).CreateCell(6).SetCellValue((double)totalGW);
            sheet.GetRow(_RowCount).GetCell(6).CellStyle = normal_Right_Border;

            Footer_PackingList(ref sheet, normal_Left_Bold, normal_Right_Bold,normal_BorderBottom, preCustNbr1);
        }

        public void Footer_PackingList(ref ISheet sheet, ICellStyle normal_Left_Bold, ICellStyle normal_Right_Bold, ICellStyle normal_BorderBottom, string maxCarton)
        {
            sheet.CreateRow(++_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("REMARK");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Left_Bold;
            sheet.GetRow(_RowCount).CreateCell(3).SetCellValue(string.Format("Total {0} Cartons", maxCarton));
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = normal_Right_Bold;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 3, 4));

            _RowCount += 2;
            sheet.CreateRow(_RowCount);
            sheet.GetRow(_RowCount).CreateCell(0).SetCellValue("Signature:");
            sheet.GetRow(_RowCount).GetCell(0).CellStyle = normal_Left_Bold;
            sheet.GetRow(_RowCount).CreateCell(1);
            sheet.GetRow(_RowCount).GetCell(1).CellStyle = normal_BorderBottom;
            sheet.GetRow(_RowCount).CreateCell(2);
            sheet.GetRow(_RowCount).GetCell(2).CellStyle = normal_BorderBottom;
            sheet.GetRow(_RowCount).CreateCell(3);
            sheet.GetRow(_RowCount).GetCell(3).CellStyle = normal_BorderBottom;
            sheet.GetRow(_RowCount).CreateCell(4);
            sheet.GetRow(_RowCount).GetCell(4).CellStyle = normal_BorderBottom;
            sheet.GetRow(_RowCount).CreateCell(5);
            sheet.GetRow(_RowCount).GetCell(5).CellStyle = normal_BorderBottom;
            sheet.GetRow(_RowCount).CreateCell(6);
            sheet.GetRow(_RowCount).GetCell(6).CellStyle = normal_BorderBottom;
            sheet.AddMergedRegion(new CellRangeAddress(_RowCount, _RowCount, 1, 6));
        }

        #region Static Method
        public static byte[] GetImageBytes(PXGraph graph, Guid? noteID)
        {
            return PXSelectJoin<PX.SM.UploadFileRevision, InnerJoin<PX.SM.UploadFile, On<PX.SM.UploadFile.fileID, Equal<PX.SM.UploadFileRevision.fileID>,
                                                                                         And<PX.SM.UploadFile.lastRevisionID, Equal<PX.SM.UploadFileRevision.fileRevisionID>>>,
                                                          InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<PX.SM.UploadFile.fileID>>>>,
                                                          Where<NoteDoc.noteID, Equal<Required<PX.Objects.CR.BAccount2.noteID>>>>.Select(graph, noteID).TopFirst.Data;
        }
        #endregion
    }
}
