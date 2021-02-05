using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common.Collection;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CM;
using PX.Objects.Common.Discount;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.TX;
using ExternalTax = PX.Objects.AR.ARInvoiceEntryExternalTax;

namespace PX.Objects.SO
{
	public class SOInvoiceEntry_Extension : PXGraphExtension<SOInvoiceEntry>
	{
		#region Methods
		public virtual void InvoiceOrder2(DateTime invoiceDate, PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> order, 
										  Customer customer, InvoiceList list, PXQuickProcess.ActionFlow quickProcessFlow)
						 => InvoiceOrder2(invoiceDate, order, null, customer, list, quickProcessFlow, false);

		public virtual void InvoiceOrder2(DateTime invoiceDate, PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> order, PXResultset<SOShipLine, SOLine> details, 
										  Customer customer, InvoiceList list, PXQuickProcess.ActionFlow quickProcessFlow, bool groupByDefaultOperation)
		{
			ARInvoice newdoc;
			SOOrderShipment orderShipment = order;
			SOOrder soOrder = order;
			CurrencyInfo soCuryInfo = order;
			SOAddress soBillAddress = order;
			SOContact soBillContact = order;
			SOOrderType soOrderType = SOOrderType.PK.Find(Base, soOrder.OrderType);
			SOShipment soShipment = Base.GetShipment(orderShipment);

			if (soOrder.HasLegacyCCTran == true)
			{
				throw new PXException(Messages.CantProcessSOBecauseItHasLegacyCCTran, soOrder.OrderType, soOrder.OrderNbr);
			}

			//TODO: Temporary solution. Review when AC-80210 is fixed
			if (orderShipment.ShipmentNbr != Constants.NoShipmentNbr && orderShipment.ShipmentType != SOShipmentType.DropShip && orderShipment.Confirmed != true)
			{
				throw new PXException(Messages.UnableToProcessUnconfirmedShipment, orderShipment.ShipmentNbr);
			}

			decimal ApprovedBalance = 0;
			HashSet<SOOrder> accountedForOrders = new HashSet<SOOrder>(new LSSOLine.Comparer<SOOrder>(Base));

			PXRowUpdated ApprovedBalanceCollector = delegate (PXCache sender, PXRowUpdatedEventArgs e)
			{
				ARInvoice ARDoc = (ARInvoice)e.Row;

				//Discounts can reduce the balance - adjust the creditHold if it was wrongly set:
				if ((decimal)ARDoc.DocBal <= ApprovedBalance && ARDoc.CreditHold == true)
				{
					object OldRow = sender.CreateCopy(ARDoc);
					sender.SetValueExt<ARInvoice.creditHold>(ARDoc, false);
					sender.RaiseRowUpdated(ARDoc, OldRow);
				}

				//Maximum approved balance for an invoice is the sum of all approved order amounts:
				if ((bool)soOrder.ApprovedCredit)
				{
					if (!accountedForOrders.Contains(soOrder))
					{
						ApprovedBalance += soOrder.ApprovedCreditAmt.GetValueOrDefault();
						accountedForOrders.Add(soOrder);
					}

					ARDoc.ApprovedCreditAmt = ApprovedBalance;
					ARDoc.ApprovedCredit = true;
				}
			};

			var customerCreditExtension = Base.FindImplementation<AR.GraphExtensions.ARInvoiceEntry_ARInvoiceCustomerCreditExtension>();

			if (customerCreditExtension != null)
			{
				customerCreditExtension.AppendPreUpdatedEvent(typeof(ARInvoice), ApprovedBalanceCollector);
			}

			SOOpenPeriodAttribute.SetValidatePeriod<ARInvoice.finPeriodID>(Base.Document.Cache, null, PeriodValidation.Nothing);

			if (list != null)
			{
				DateTime orderInvoiceDate = Base.GetOrderInvoiceDate(invoiceDate, soOrder, orderShipment);

				/// <remark> 
				/// Replace the standard method [FindOrCreateInvoice] with the custom method [FindOrCreateInvoice2] for specify logic on Jira 
				/// </remark>
				newdoc = FindOrCreateInvoice2(orderInvoiceDate, order, list, groupByDefaultOperation);

				if (newdoc.RefNbr != null)
				{
					Base.Document.Current = newdoc = Base.Document.Search<ARInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					Base.Clear();

					newdoc.DocType = Base.GetInvoiceDocType(soOrderType, orderShipment.Operation);

					newdoc.DocDate = orderInvoiceDate;
					newdoc.BranchID = soOrder.BranchID;

					if (string.IsNullOrEmpty(soOrder.FinPeriodID) == false)
					{
						newdoc.FinPeriodID = soOrder.FinPeriodID;
					}

					if (soOrder.InvoiceNbr != null)
					{
						newdoc.RefNbr = soOrder.InvoiceNbr;
						newdoc.RefNoteID = soOrder.NoteID;
					}

					if (soOrderType.UserInvoiceNumbering == true && string.IsNullOrEmpty(newdoc.RefNbr))
					{
						throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOOrder.invoiceNbr>(Base.soorder.Cache));
					}

					if (soOrderType.UserInvoiceNumbering == false && !string.IsNullOrEmpty(newdoc.RefNbr))
					{
						throw new PXException(Messages.MustBeUserNumbering, soOrderType.InvoiceNumberingID);
					}

					AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(Base.Document.Cache, soOrderType.ARDocType, soOrderType.InvoiceNumberingID);

					newdoc = (ARInvoice)Base.Document.Cache.CreateCopy(Base.Document.Insert(newdoc));

					newdoc.CustomerID = soOrder.CustomerID;
					newdoc.CustomerLocationID = soOrder.CustomerLocationID;

					if (newdoc.DocType != ARDocType.CreditMemo)
					{
						newdoc.TermsID = soOrder.TermsID;
						newdoc.DiscDate = soOrder.DiscDate;
						newdoc.DueDate = soOrder.DueDate;
					}

					newdoc.TaxZoneID = soOrder.TaxZoneID;
					newdoc.TaxCalcMode = soOrder.TaxCalcMode;
					newdoc.AvalaraCustomerUsageType = soOrder.AvalaraCustomerUsageType;
					newdoc.SalesPersonID = soOrder.SalesPersonID;
					newdoc.DocDesc = soOrder.OrderDesc;
					newdoc.InvoiceNbr = soOrder.CustomerOrderNbr;
					newdoc.CuryID = soOrder.CuryID;
					newdoc.ProjectID = soOrder.ProjectID ?? PM.ProjectDefaultAttribute.NonProject();
					newdoc.Hold = quickProcessFlow != PXQuickProcess.ActionFlow.HasNextInFlow && soOrderType.InvoiceHoldEntry == true;

					if (soOrderType.MarkInvoicePrinted == true)
					{
						newdoc.Printed = true;
					}

					if (soOrderType.MarkInvoiceEmailed == true)
					{
						newdoc.Emailed = true;
					}

					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						newdoc.PMInstanceID = soOrder.PMInstanceID;
						newdoc.PaymentMethodID = soOrder.PaymentMethodID;
						newdoc.CashAccountID = soOrder.CashAccountID;
					}

					var cancel_defaulting = new PXFieldDefaulting((cache, e) =>
					{
						e.NewValue = cache.GetValue<ARInvoice.branchID>(e.Row);
						e.Cancel = true;
					});
					Base.FieldDefaulting.AddHandler<ARInvoice.branchID>(cancel_defaulting);

					try
					{
						using (new PXReadDeletedScope())
						{
							newdoc = Base.Document.Update(newdoc);
							if (newdoc.TaxCalcMode != soOrder.TaxCalcMode)
							{
								newdoc.TaxCalcMode = soOrder.TaxCalcMode;
								newdoc = Base.Document.Update(newdoc);
							}
						}
					}
					finally
					{
						Base.FieldDefaulting.RemoveHandler<ARInvoice.branchID>(cancel_defaulting);
					}

					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						if (Base.SODocument.Current.DocType != ARDocType.CreditMemo)
						{
							Base.SODocument.Current.PMInstanceID = soOrder.PMInstanceID;
							Base.SODocument.Current.PaymentMethodID = soOrder.PaymentMethodID;
							if (Base.SODocument.Current.CashAccountID != soOrder.CashAccountID)
								Base.SODocument.SetValueExt<SOInvoice.cashAccountID>(Base.SODocument.Current, soOrder.CashAccountID);
							if (Base.SODocument.Current.CashAccountID == null)
								Base.SODocument.Cache.SetDefaultExt<SOInvoice.cashAccountID>(Base.SODocument.Current);
							if (Base.SODocument.Current.ARPaymentDepositAsBatch == true && Base.SODocument.Current.DepositAfter == null)
								Base.SODocument.Current.DepositAfter = Base.SODocument.Current.AdjDate;
							Base.SODocument.Current.ExtRefNbr = soOrder.ExtRefNbr;
						}
						//clear error in case invoice currency different from default cash account for customer
						Base.SODocument.Cache.RaiseExceptionHandling<SOInvoice.cashAccountID>(Base.SODocument.Current, null, null);
					}

					CurrencyInfo info = Base.currencyinfo.Select();
					bool curyRateNotDefined = (info.CuryRate ?? 0m) == 0m;
					if (curyRateNotDefined || soOrderType.UseCuryRateFromSO == true)
					{
						PXCache<CurrencyInfo>.RestoreCopy(info, soCuryInfo);
						info.CuryInfoID = newdoc.CuryInfoID;
					}
					else
					{
						info.CuryRateTypeID = soCuryInfo.CuryRateTypeID;
						Base.currencyinfo.Update(info);
					}

					AddressAttribute.CopyRecord<ARInvoice.billAddressID>(Base.Document.Cache, newdoc, soBillAddress, true);
					ContactAttribute.CopyRecord<ARInvoice.billContactID>(Base.Document.Cache, newdoc, soBillContact, true);
					
					/// <Remark> Comment out the shipping contact logic. </Remark>
					//var soShipContact = SOContact.PK.Find(Base, orderShipment.ShipContactID);
					//ARShippingContactAttribute.CopyRecord<ARInvoice.shipContactID>(Base.Document.Cache, newdoc, soShipContact, true);
				}
			}
			else
			{
				newdoc = (ARInvoice)Base.Document.Cache.CreateCopy(Base.Document.Current);
				bool newInvoice = (Base.Transactions.SelectSingle() == null);

				if (newInvoice)
				{
					newdoc.CustomerID = soOrder.CustomerID;
					newdoc.ProjectID = soOrder.ProjectID;
					newdoc.CustomerLocationID = soOrder.CustomerLocationID;
					newdoc.SalesPersonID = soOrder.SalesPersonID;
					newdoc.TaxZoneID = soOrder.TaxZoneID;
					newdoc.TaxCalcMode = soOrder.TaxCalcMode;
					newdoc.AvalaraCustomerUsageType = soOrder.AvalaraCustomerUsageType;
					newdoc.DocDesc = soOrder.OrderDesc;
					newdoc.InvoiceNbr = soOrder.CustomerOrderNbr;
					newdoc.TermsID = soOrder.TermsID;
				}

				CurrencyInfo info = Base.currencyinfo.Select();
				bool curyRateNotDefined = (info.CuryRate ?? 0m) == 0m;
				bool copyCuryInfoFromSO = (curyRateNotDefined || newInvoice && soOrderType.UseCuryRateFromSO == true);
				if (copyCuryInfoFromSO)
				{
					PXCache<CurrencyInfo>.RestoreCopy(info, soCuryInfo);
					info.CuryInfoID = newdoc.CuryInfoID;
					Base.currencyinfo.Update(info);
					newdoc.CuryID = info.CuryID;
				}
				else
				{
					if (!Base.currencyinfo.Cache.ObjectsEqual<CurrencyInfo.curyID>(info, soCuryInfo)
						|| !Base.currencyinfo.Cache.ObjectsEqual<
							CurrencyInfo.curyRateTypeID,
							CurrencyInfo.curyMultDiv,
							CurrencyInfo.curyRate>(info, soCuryInfo)
						&& soOrderType.UseCuryRateFromSO == true)
					{
						throw new PXException(Messages.CurrencyRateDiffersInSO, soOrder.RefNbr);
					}
				}

				newdoc = Base.Document.Update(newdoc);

				AddressAttribute.CopyRecord<ARInvoice.billAddressID>(Base.Document.Cache, newdoc, soBillAddress, true);
				ContactAttribute.CopyRecord<ARInvoice.billContactID>(Base.Document.Cache, newdoc, soBillContact, true);

				/// <Remark> Comment out the shipping contact logic. </Remark>
				//var soShipContact = SOContact.PK.Find(Base, orderShipment.ShipContactID);
				//ARShippingContactAttribute.CopyRecord<ARInvoice.shipContactID>(Base.Document.Cache, newdoc, soShipContact, true);
			}

			Base.SODocument.Current = (SOInvoice)Base.SODocument.Select() ?? (SOInvoice)Base.SODocument.Cache.Insert();
			if (Base.SODocument.Current.ShipAddressID == null)
			{
				DefaultShippingAddress(newdoc, orderShipment, soShipment);
			}
			/// <Remark> Comment out the shipping address logic. </Remark>
			//else if (Base.SODocument.Current.ShipAddressID != orderShipment.ShipAddressID && newdoc.MultiShipAddress != true)
			//{
			//	newdoc.MultiShipAddress = true;
			//	ARShippingAddressAttribute.DefaultRecord<ARInvoice.shipAddressID>(Base.Document.Cache, newdoc);
			//}

			bool prevHoldState = newdoc.Hold == true;
			if (newdoc.Hold != true)
			{
				newdoc.Hold = true;
				newdoc = Base.Document.Update(newdoc);
			}
			InvoiceCreated(newdoc, soOrder);

			PXSelectBase<ARInvoiceDiscountDetail> selectInvoiceDiscounts = new PXSelect<ARInvoiceDiscountDetail,
			Where<ARInvoiceDiscountDetail.docType, Equal<Current<SOInvoice.docType>>,
			And<ARInvoiceDiscountDetail.refNbr, Equal<Current<SOInvoice.refNbr>>,
			And<ARInvoiceDiscountDetail.orderType, Equal<Required<ARInvoiceDiscountDetail.orderType>>,
			And<ARInvoiceDiscountDetail.orderNbr, Equal<Required<ARInvoiceDiscountDetail.orderNbr>>>>>>>(Base);

			foreach (ARInvoiceDiscountDetail detail in selectInvoiceDiscounts.Select(orderShipment.OrderType, orderShipment.OrderNbr))
			{
				ARDiscountEngine.DeleteDiscountDetail(Base.ARDiscountDetails.Cache, Base.ARDiscountDetails, detail);
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

			if (details != null)
			{
				PXCache cache = Base.Caches[typeof(SOShipLine)];
				foreach (PXResult<SOShipLine, SOLine> det in details)
				{
					SOShipLine shipline = det;
					SOLine soline = det;
					//there should be no parent record of SOLineSplit2 type.
					var insertedshipline = (SOShipLine)cache.Insert(shipline);

					if (insertedshipline == null)
						continue;

					if (insertedshipline.LineType == SOLineType.Inventory)
					{
						var ii = IN.InventoryItem.PK.Find(Base, insertedshipline.InventoryID);
						if (ii.StkItem == false && ii.KitItem == true)
						{
							insertedshipline.RequireINUpdate = ((SOLineSplit)PXSelectJoin<SOLineSplit,
								InnerJoin<IN.InventoryItem,
									On2<SOLineSplit.FK.InventoryItem,
									And<IN.InventoryItem.stkItem, Equal<True>>>>,
								Where<SOLineSplit.orderType, Equal<Current<SOLine.orderType>>, And<SOLineSplit.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOLineSplit.lineNbr, Equal<Current<SOLine.lineNbr>>, And<SOLineSplit.qty, Greater<Zero>>>>>>.SelectSingleBound(Base, new object[] { soline })) != null;
						}
						else
						{
							insertedshipline.RequireINUpdate = ii.StkItem;
						}
					}
					else
					{
						insertedshipline.RequireINUpdate = false;
					}
				}
			}

			//DropShip Receipt/Shipment cannot be invoiced twice thats why we have to be sure that all SOPO links at this point in that Receipt are valid:

			if (orderShipment.ShipmentType == SOShipmentType.DropShip)
			{
				PXSelectBase<POReceiptLine> selectUnlinkedDropShips = new PXSelectJoin<POReceiptLine,
					InnerJoin<PO.POLine, On<PO.POLine.orderType, Equal<POReceiptLine.pOType>, And<PO.POLine.orderNbr, Equal<POReceiptLine.pONbr>, And<PO.POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
					LeftJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>, And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>, And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>>>,
					Where<POReceiptLine.receiptType, Equal<PO.POReceiptType.poreceipt>,
					And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
					And<SOLineSplit.pOLineNbr, IsNull,
					And<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>>>>>>(Base);

				var rs = selectUnlinkedDropShips.Select(orderShipment.ShipmentNbr);
				if (rs.Count > 0)
				{
					foreach (POReceiptLine line in rs)
					{
						InventoryItem item = IN.InventoryItem.PK.Find(Base, line.InventoryID);
						PXTrace.WriteError(Messages.SOPOLinkIsIvalidInPOOrder, line.PONbr, item?.InventoryCD);
					}

					throw new PXException(Messages.SOPOLinkIsIvalid);
				}
			}

			orderShipment = PXCache<SOOrderShipment>.CreateCopy(orderShipment);
			orderShipment.InvoiceType = Base.Document.Current.DocType;
			orderShipment.InvoiceNbr = Base.Document.Current.RefNbr;
			if (string.Equals(orderShipment.ShipmentNbr, Constants.NoShipmentNbr))
			{
				orderShipment.ShippingRefNoteID = Base.Document.Current.NoteID;
			}
			orderShipment = Base.shipmentlist.Update(orderShipment);

			DateTime? origInvoiceDate = null;
			bool updateINRequired = (orderShipment.ShipmentType == SOShipmentType.DropShip);

			HashSet<ARTran> set = new HashSet<ARTran>(new LSSOLine.Comparer<ARTran>(Base));
			Dictionary<int, SOSalesPerTran> dctcommisions = new Dictionary<int, SOSalesPerTran>();

			foreach (PXResult<SOShipLine, SOLine, SOSalesPerTran, ARTran, ARTranAccrueCost> res in
				PXSelectJoin<SOShipLine,
					InnerJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
						And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
						And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>,
					LeftJoin<SOSalesPerTran, On<SOLine.orderType, Equal<SOSalesPerTran.orderType>,
						And<SOLine.orderNbr, Equal<SOSalesPerTran.orderNbr>,
						And<SOLine.salesPersonID, Equal<SOSalesPerTran.salespersonID>>>>,
					LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>,
						And<ARTran.sOShipmentType, Equal<SOShipLine.shipmentType>,
						And<ARTran.sOOrderType, Equal<SOShipLine.origOrderType>,
						And<ARTran.sOOrderNbr, Equal<SOShipLine.origOrderNbr>,
						And<ARTran.sOOrderLineNbr, Equal<SOShipLine.origLineNbr>,
						And<ARTran.canceled, NotEqual<True>,
						And<ARTran.isCancellation, NotEqual<True>>>>>>>>,
					LeftJoin<ARTranAccrueCost, On<ARTranAccrueCost.tranType, Equal<SOLine.invoiceType>,
						  And<ARTranAccrueCost.refNbr, Equal<SOLine.invoiceNbr>,
						  And<ARTranAccrueCost.lineNbr, Equal<SOLine.invoiceLineNbr>>>>>>>>,
					Where<SOShipLine.shipmentNbr, Equal<Required<SOShipLine.shipmentNbr>>,
						And<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
						And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>>>>>
					.Select(Base, orderShipment.ShipmentNbr, orderShipment.OrderType, orderShipment.OrderNbr))
			{
				ARTran artran = (ARTran)res;
				ARTranAccrueCost artranAccrueCost = (ARTranAccrueCost)res;
				SOSalesPerTran sspt = (SOSalesPerTran)res;

				if (sspt != null && sspt.SalespersonID != null && !dctcommisions.ContainsKey(sspt.SalespersonID.Value))
				{
					dctcommisions[sspt.SalespersonID.Value] = sspt;
				}
				if (artran.RefNbr == null || (artran.RefNbr != null && Base.Transactions.Cache.GetStatus(artran) == PXEntryStatus.Deleted))

				{
					SOLine orderline = (SOLine)res;
					SOShipLine shipline = (SOShipLine)res;

					//TODO: Temporary solution. Review when AC-80210 is fixed
					if (shipline.ShipmentNbr != null && orderShipment.ShipmentType != SOShipmentType.DropShip && orderShipment.ShipmentNbr != Constants.NoShipmentNbr && (shipline.Confirmed != true || shipline.UnassignedQty != 0))
					{
						throw new PXException(Messages.UnableToProcessUnconfirmedShipment, shipline.ShipmentNbr);
					}

					if (Math.Abs((decimal)shipline.BaseShippedQty) < 0.0000005m && !string.Equals(shipline.ShipmentNbr, Constants.NoShipmentNbr))
					{
						continue;
					}

					if (origInvoiceDate == null && orderline.InvoiceDate != null)
						origInvoiceDate = orderline.InvoiceDate;

					ARTran newtran = Base.CreateTranFromShipLine(newdoc, soOrderType, orderline.Operation, orderline, ref shipline);
					foreach (ARTran existing in Base.Transactions.Cache.Inserted)
					{
						if (Base.Transactions.Cache.ObjectsEqual<ARTran.sOShipmentNbr, ARTran.sOShipmentType, ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOOrderLineNbr>(newtran, existing))
						{
							Base.Transactions.Cache.RestoreCopy(newtran, existing);
							break;
						}
					}

					foreach (ARTran existing in Base.Transactions.Cache.Updated)
					{
						if (Base.Transactions.Cache.ObjectsEqual<ARTran.sOShipmentNbr, ARTran.sOShipmentType, ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOOrderLineNbr>(newtran, existing))
						{
							Base.Transactions.Cache.RestoreCopy(newtran, existing);
							break;
						}
					}

					if (artranAccrueCost != null && artranAccrueCost.AccrueCost == true)
					{
						newtran.AccrueCost = artranAccrueCost.AccrueCost;
						newtran.CostBasis = artranAccrueCost.CostBasis;
						newtran.ExpenseAccrualAccountID = artranAccrueCost.ExpenseAccrualAccountID;
						newtran.ExpenseAccrualSubID = artranAccrueCost.ExpenseAccrualSubID;
						newtran.ExpenseAccountID = artranAccrueCost.ExpenseAccountID;
						newtran.ExpenseSubID = artranAccrueCost.ExpenseSubID;

						if (newtran.Qty != 0 && artranAccrueCost.Qty != 0)
							newtran.AccruedCost = PXPriceCostAttribute.Round((decimal)(artranAccrueCost.AccruedCost ?? 0m * (newtran.Qty / artranAccrueCost.Qty)));

					}

					if (newtran.LineNbr == null)
					{
						try
						{
							Base.cancelUnitPriceCalculation = true;
							newtran = Base.Transactions.Insert(newtran);
							set.Add(newtran);
						}
						catch (PXSetPropertyException e)
						{
							throw new PXErrorContextProcessingException(Base, PXParentAttribute.SelectParent(Base.Caches[typeof(ARTran)], newtran, typeof(SOLine2)), e);
						}
						finally
						{
							Base.cancelUnitPriceCalculation = false;
						}

						PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(SOLine)], orderline, Base.Caches[typeof(ARTran)], newtran,
							soOrderType.CopyLineNotesToInvoice == true && (soOrderType.CopyLineNotesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory),
							soOrderType.CopyLineFilesToInvoice == true && (soOrderType.CopyLineFilesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory));
					}
					else
					{
						newtran = Base.Transactions.Update(newtran);
						TaxAttribute.Calculate<ARTran.taxCategoryID>(Base.Transactions.Cache, new PXRowUpdatedEventArgs(newtran, null, true));
					}

					if (newtran.RequireINUpdate == true && newtran.Qty != 0m)
					{
						updateINRequired = true;
					}

				}
			}
			PXSelectBase<ARTran> cmd = new PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ARTran.sOOrderType, Equal<Current<SOMiscLine2.orderType>>,
					And<ARTran.sOOrderNbr, Equal<Current<SOMiscLine2.orderNbr>>,
					And<ARTran.sOOrderLineNbr, Equal<Current<SOMiscLine2.lineNbr>>>>>>>>(Base);


			foreach (PXResult<SOMiscLine2, SOSalesPerTran> res in PXSelectJoin<SOMiscLine2,
																LeftJoin<SOSalesPerTran, On<SOMiscLine2.orderType, Equal<SOSalesPerTran.orderType>,
																	And<SOMiscLine2.orderNbr, Equal<SOSalesPerTran.orderNbr>,
																	And<SOMiscLine2.salesPersonID, Equal<SOSalesPerTran.salespersonID>>>>>,
				Where<SOMiscLine2.orderType, Equal<Required<SOMiscLine2.orderType>>,
					And<SOMiscLine2.orderNbr, Equal<Required<SOMiscLine2.orderNbr>>,
																	And<
																		Where2<
																			Where<SOMiscLine2.curyUnbilledAmt, Greater<decimal0>,   //direct billing process with positive amount
																			And<SOMiscLine2.curyLineAmt, Greater<decimal0>>>,
																		Or2<
																			Where<SOMiscLine2.curyUnbilledAmt, Less<decimal0>,      //billing process with negative amount
																			And<SOMiscLine2.curyLineAmt, Less<decimal0>>>,
																		Or<
																			Where<SOMiscLine2.curyLineAmt, Equal<decimal0>,         //special case with zero line amount, e.g. discount = 100% or unit price=0
																			And<SOMiscLine2.unbilledQty, Greater<decimal0>>>>>>>>>>
																.Select(Base, orderShipment.OrderType, orderShipment.OrderNbr))
			{
				SOMiscLine2 orderline = res;
				SOSalesPerTran sspt = res;

				if (sspt != null && sspt.SalespersonID != null && !dctcommisions.ContainsKey(sspt.SalespersonID.Value))
				{
					dctcommisions[sspt.SalespersonID.Value] = sspt;
				}
				if (cmd.View.SelectSingleBound(new object[] { Base.Document.Current, orderline }) == null)
				{
					ARTran newtran = Base.CreateTranFromMiscLine(orderShipment, orderline);
					if (Base.Document.Current != null && ((Base.Document.Current.CuryLineTotal ?? 0m) + (newtran.CuryTranAmt ?? 0m)) < 0m)
						continue;

					ChangeBalanceSign(newtran, newdoc, orderline.Operation);
					newtran = Base.Transactions.Insert(newtran);
					set.Add(newtran);
					PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(SOMiscLine2)], orderline, Base.Caches[typeof(ARTran)], newtran,
													 soOrderType.CopyLineNotesToInvoice, soOrderType.CopyLineFilesToInvoice);
				}
			}

			foreach (SOSalesPerTran sspt in dctcommisions.Values)
			{
				ARSalesPerTran aspt = new ARSalesPerTran();
				aspt.DocType = newdoc.DocType;
				aspt.RefNbr = newdoc.RefNbr;
				aspt.SalespersonID = sspt.SalespersonID;
				Base.commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjNbr>(aspt);
				Base.commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjdRefNbr>(aspt);
				Base.commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjdDocType>(aspt);
				aspt = Base.commisionlist.Locate(aspt);
				if (aspt != null && aspt.CommnPct != sspt.CommnPct)
				{
					aspt.CommnPct = sspt.CommnPct;
					Base.commisionlist.Update(aspt);
				}
			}

			if (Base.UnattendedMode == true)
			{
				//Total resort and orderNumber assignments:

				List<Tuple<string, ARTran>> invoiceLines = new List<Tuple<string, ARTran>>();
				foreach (PXResult<ARTran> res in Base.Transactions.Select())
				{
					ARTran tran = res;

					string sortkey = string.Format("{0}.{1}.{2:D7}.{3}", tran.SOOrderType, tran.SOOrderNbr, tran.SOOrderSortOrder, tran.SOShipmentNbr);
					invoiceLines.Add(new Tuple<string, ARTran>(sortkey, tran));
				}

				invoiceLines.Sort((x, y) => x.Item1.CompareTo(y.Item1));

				for (int i = 0; i < invoiceLines.Count; i++)
				{
					if (invoiceLines[i].Item2.SortOrder != i + 1)
					{
						invoiceLines[i].Item2.SortOrder = i + 1;
						if (Base.Transactions.Cache.GetStatus(invoiceLines[i].Item2) != PXEntryStatus.Inserted)
						{
							Base.Transactions.Cache.SetStatus(invoiceLines[i].Item2, PXEntryStatus.Updated);
						}
					}
				}
			}
			else
			{
				//Appending to the end sorted soordershipment transactions.

				int lastSortOrderNbr = 0;
				List<Tuple<string, ARTran>> tail = new List<Tuple<string, ARTran>>();
				foreach (PXResult<ARTran> res in Base.Transactions.Select())
				{
					ARTran tran = res;

					if (set.Contains(tran))
					{
						string sortkey = string.Format("{0}.{1:D7}.{2}", tran.SOOrderNbr, tran.SOOrderSortOrder, tran.SOShipmentNbr);
						tail.Add(new Tuple<string, ARTran>(sortkey, tran));
					}
					else
					{
						lastSortOrderNbr = Math.Max(lastSortOrderNbr, tran.SortOrder.GetValueOrDefault());
					}
				}

				tail.Sort((x, y) => x.Item1.CompareTo(y.Item1));

				for (int i = 0; i < tail.Count; i++)
				{
					lastSortOrderNbr++;
					if (tail[i].Item2.SortOrder != lastSortOrderNbr)
					{
						tail[i].Item2.SortOrder = lastSortOrderNbr;

						if (Base.Transactions.Cache.GetStatus(tail[i].Item2) != PXEntryStatus.Inserted)
						{
							Base.Transactions.Cache.SetStatus(tail[i].Item2, PXEntryStatus.Updated);
						}
					}
				}
			}

			Base.SODocument.Current.BillAddressID = soOrder.BillAddressID;
			Base.SODocument.Current.BillContactID = soOrder.BillContactID;

			Base.SODocument.Current.ShipAddressID = orderShipment.ShipAddressID;
			Base.SODocument.Current.ShipContactID = orderShipment.ShipContactID;

			Base.SODocument.Current.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject();

			if (ExternalTranHelper.IsOrderSelfCaptured(Base, soOrder))
			{
				Base.SODocument.Current.IsCCCaptured = soOrder.IsCCCaptured;
				Base.SODocument.Current.IsCCCaptureFailed = soOrder.IsCCCaptureFailed;
				if (soOrder.IsCCCaptured == true)
				{
					Base.SODocument.Current.CuryCCCapturedAmt = soOrder.CuryCCCapturedAmt;
					Base.SODocument.Current.CCCapturedAmt = soOrder.CCCapturedAmt;
				}
			}

			Base.SODocument.Current.RefTranExtNbr = soOrder.RefTranExtNbr;
			Base.SODocument.Current.CreateINDoc |= (updateINRequired && orderShipment.InvtRefNbr == null);

			orderShipment = PXCache<SOOrderShipment>.CreateCopy(orderShipment);
			orderShipment.CreateINDoc = updateINRequired;

			SOFreightDetail fd = Base.FillFreightDetails(soOrder, orderShipment);

			orderShipment = Base.shipmentlist.Update(orderShipment);
			orderShipment = orderShipment.LinkInvoice(Base.SODocument.Current, Base);

			if (string.Equals(orderShipment.ShipmentNbr, Constants.NoShipmentNbr))
			{
				SOOrder cached = Base.soorder.Locate(soOrder);
				if (cached != null)
				{
					if ((cached.Behavior == SOBehavior.SO || cached.Behavior == SOBehavior.RM) && cached.OpenLineCntr == 0)
					{
						cached.MarkCompleted();
					}
					cached.ShipmentCntr++;
					Base.soorder.Update(cached);
				}
			}

			PXSelectBase<SOLine> transactions = new PXSelect<SOLine,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(Base);

			PXSelectBase<SOOrderDiscountDetail> discountdetail = new PXSelect<SOOrderDiscountDetail,
			Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>,
			And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(Base);

			Lazy<bool> fullOrderInvoicing = new Lazy<bool>(() => Base.IsFullOrderInvoicing(soOrder, soOrderType, transactions));

			Lazy<TwoWayLookup<SOOrderDiscountDetail, SOLine>> discountCodesWithApplicableSOLines =
				new Lazy<TwoWayLookup<SOOrderDiscountDetail, SOLine>>(() => DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>()
					.GetListOfLinksBetweenDiscountsAndDocumentLines(Base.Caches[typeof(SOLine)], transactions, discountdetail));

			Lazy<bool> hasManualDiscounts = new Lazy<bool>(() => discountCodesWithApplicableSOLines.Value.LeftValues.Any(dd => dd.IsManual == true));

			/*In case Discounts were not recalculated add prorated discounts */
			if (soOrderType.RecalculateDiscOnPartialShipment != true || fullOrderInvoicing.Value && hasManualDiscounts.Value)
			{
				decimal? defaultRate = 1m;
				if (soOrder.LineTotal > 0m)
					defaultRate = orderShipment.LineTotal / soOrder.LineTotal;

				TwoWayLookup<ARInvoiceDiscountDetail, ARTran> discountCodesWithApplicableARLines = new TwoWayLookup<ARInvoiceDiscountDetail, ARTran>(leftComparer: new ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer());

				foreach (SOOrderDiscountDetail docGroupDisc in discountCodesWithApplicableSOLines.Value.LeftValues)
				{
					if (soOrderType.RecalculateDiscOnPartialShipment == true && docGroupDisc.IsManual != true)
						continue;

					var dd = new ARInvoiceDiscountDetail
					{
						SkipDiscount = docGroupDisc.SkipDiscount,
						Type = docGroupDisc.Type,
						DiscountID = docGroupDisc.DiscountID,
						DiscountSequenceID = docGroupDisc.DiscountSequenceID,
						OrderType = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() ? docGroupDisc.OrderType : null,
						OrderNbr = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() ? docGroupDisc.OrderNbr : null,
						DocType = newdoc.DocType,
						RefNbr = newdoc.RefNbr,
						IsManual = docGroupDisc.IsManual,
						DiscountPct = docGroupDisc.DiscountPct,
						FreeItemID = docGroupDisc.FreeItemID,
						FreeItemQty = docGroupDisc.FreeItemQty,
						ExtDiscCode = docGroupDisc.ExtDiscCode,
						Description = docGroupDisc.Description
					};

					decimal? rate = defaultRate;
					decimal invoicedCuryGroupAmt = 0m;
					decimal invoicedMiscAmt = 0m;
					foreach (SOLine soLine in discountCodesWithApplicableSOLines.Value.RightsFor(docGroupDisc))
					{
						foreach (ARTran tran in Base.Transactions.Select())
						{
							if ((soLine.OrderType == tran.SOOrderType && soLine.OrderNbr == tran.SOOrderNbr && soLine.LineNbr == tran.SOOrderLineNbr) ||
								!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
							{
								if (docGroupDisc.Type == DiscountType.Group)
									invoicedCuryGroupAmt += (tran.CuryTranAmt ?? 0m);
								else if (tran.LineType == SOLineType.MiscCharge && tran.OrigDocumentDiscountRate == 1m)
								{
									invoicedMiscAmt += (tran.TranAmt ?? 0m);
								}

								discountCodesWithApplicableARLines.Link(dd, tran);
							}
						}
					}

					bool fullOrderDiscAllocation = (fullOrderInvoicing.Value && docGroupDisc.Type == DiscountType.Document);
					if (fullOrderDiscAllocation)
					{
						rate = 1m;
					}
					else if (docGroupDisc.CuryDiscountableAmt > 0m)
					{
						if (docGroupDisc.Type == DiscountType.Group)
							rate = invoicedCuryGroupAmt / docGroupDisc.CuryDiscountableAmt;
						else if (soOrder.LineTotal != 0m || soOrder.MiscTot != 0)
							rate = (orderShipment.LineTotal + invoicedMiscAmt) / (soOrder.LineTotal + soOrder.MiscTot);
					}

					ARInvoiceDiscountDetail located = Base.ARDiscountDetails.Locate(dd);
					//RecordID prevents Locate() from work as intended. To review.
					if (located == null)
					{
						List<ARInvoiceDiscountDetail> discountDetails = new List<ARInvoiceDiscountDetail>();

						//TODO: review this part
						if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
						{
							foreach (ARInvoiceDiscountDetail detail in Base.ARDiscountDetails.Cache.Cached)
							{
								discountDetails.Add(detail);
							}
						}
						else
						{
							foreach (ARInvoiceDiscountDetail detail in Base.ARDiscountDetails.Select())
							{
								discountDetails.Add(detail);
							}
						}

						foreach (ARInvoiceDiscountDetail detail in discountDetails)
						{
							if (detail.DiscountID == dd.DiscountID && detail.DiscountSequenceID == dd.DiscountSequenceID && detail.OrderType == dd.OrderType
								&& detail.OrderNbr == dd.OrderNbr && detail.DocType == dd.DocType && detail.RefNbr == dd.RefNbr && detail.Type == dd.Type)
								located = detail;
						}
					}
					if (located != null)
					{
						if (docGroupDisc.Type == DiscountType.Group || fullOrderDiscAllocation)
						{
							located.DiscountAmt = docGroupDisc.DiscountAmt * rate;
							located.CuryDiscountAmt = docGroupDisc.CuryDiscountAmt * rate;
							located.DiscountableAmt = docGroupDisc.DiscountableAmt * rate;
							located.CuryDiscountableAmt = docGroupDisc.CuryDiscountableAmt * rate;
							located.DiscountableQty = docGroupDisc.DiscountableQty * rate;
						}
						else
						{
							located.DiscountAmt += docGroupDisc.DiscountAmt * rate;
							located.CuryDiscountAmt += docGroupDisc.CuryDiscountAmt * rate;
							located.DiscountableAmt += docGroupDisc.DiscountableAmt * rate;
							located.CuryDiscountableAmt += docGroupDisc.CuryDiscountableAmt * rate;
							located.DiscountableQty += docGroupDisc.DiscountableQty * rate;
						}
						if (Base.ARDiscountDetails.Cache.GetStatus(located) == PXEntryStatus.Deleted)
							located = ARDiscountEngine.InsertDiscountDetail(Base.ARDiscountDetails.Cache, Base.ARDiscountDetails, located);
						else
							located = ARDiscountEngine.UpdateDiscountDetail(Base.ARDiscountDetails.Cache, Base.ARDiscountDetails, located);
					}
					else
					{
						dd.DiscountAmt = docGroupDisc.DiscountAmt * rate;
						dd.CuryDiscountAmt = docGroupDisc.CuryDiscountAmt * rate;
						dd.DiscountableAmt = docGroupDisc.DiscountableAmt * rate;
						dd.CuryDiscountableAmt = docGroupDisc.CuryDiscountableAmt * rate;
						dd.DiscountableQty = docGroupDisc.DiscountableQty * rate;

						located = ARDiscountEngine.InsertDiscountDetail(Base.ARDiscountDetails.Cache, Base.ARDiscountDetails, dd);
					}

					ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer discountDetailComparer = new ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer();
					foreach (ARInvoiceDiscountDetail newDiscount in discountCodesWithApplicableARLines.LeftValues)
					{
						if (discountDetailComparer.Equals(newDiscount, located))
						{
							newDiscount.DiscountAmt = located.DiscountAmt;
							newDiscount.CuryDiscountableAmt = located.CuryDiscountableAmt;
							newDiscount.CuryDiscountAmt = located.CuryDiscountAmt;
							newDiscount.DiscountableQty = located.DiscountableQty;
							newDiscount.DiscountableAmt = located.DiscountableAmt;
							newDiscount.IsOrigDocDiscount = located.IsOrigDocDiscount;
							newDiscount.LineNbr = located.LineNbr;
						}
					}
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				{
					Base.RecalculateTotalDiscount();
				}

				PXSelectBase<ARTran> orderLinesSelect = new PXSelectJoin<ARTran, LeftJoin<SOLine, On<SOLine.orderType, Equal<ARTran.sOOrderType>,
																									 And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
																										 And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>,
																				 Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
																					   And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
																						   And<ARTran.sOOrderType, Equal<Current<SOOrder.orderType>>,
																							   And<ARTran.sOOrderNbr, Equal<Current<SOOrder.orderNbr>>>>>>,
																				 OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>(Base);

				PXSelectBase<ARInvoiceDiscountDetail> orderDiscountDetailsSelect = new PXSelect<ARInvoiceDiscountDetail, Where<ARInvoiceDiscountDetail.docType, Equal<Current<SOInvoice.docType>>, 
																															   And<ARInvoiceDiscountDetail.refNbr, Equal<Current<SOInvoice.refNbr>>,
																																   And<ARInvoiceDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>, 
																																	   And<ARInvoiceDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>>>(Base);

				ARDiscountEngine.CalculateGroupDiscountRate(Base.Transactions.Cache, orderLinesSelect, null, discountCodesWithApplicableARLines, false, forceFormulaCalculation: true);

				ARDiscountEngine.CalculateDocumentDiscountRate(Base.Transactions.Cache, discountCodesWithApplicableARLines, null, documentDetails: Base.Transactions, forceFormulaCalculation: true);

				if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				{
					Base.RecalculateTotalDiscount();
				}
			}

			if (soOrderType.RecalculateDiscOnPartialShipment == true)
			{
				//Recalculate all discounts
				ARTran firstLine = null;

				//Recalculate line discounts on each line
				foreach (ARTran tran in Base.Transactions.Select())
				{
					if (firstLine == null)
						firstLine = tran;

					Base.RecalculateDiscounts(Base.Transactions.Cache, tran, true);
					Base.Transactions.Update(tran);
				}

				//Recalculate group and document discounts
				if (firstLine != null)
				{
					Base.RecalculateDiscounts(Base.Transactions.Cache, firstLine);
					Base.Transactions.Update(firstLine);
				}
			}

			Base.AddOrderTaxes(orderShipment);

			if (!Base.IsExternalTax(Base.Document.Current.TaxZoneID))
			{
				if (soShipment != null && soShipment.TaxCategoryID != soOrder.FreightTaxCategoryID)
				{
					// if freight tax category is changed on the shipment we need to recalculate freight tax
					// because the tax added in the shipment may be absent in the sales order
					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualLineCalc);
					try
					{
						fd.TaxCategoryID = soShipment.TaxCategoryID;
						Base.FreightDetails.Update(fd);
					}
					finally
					{
						TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);
					}
				}
			}

			decimal? CuryApplAmt = 0m;
			bool Calculated = false;

			foreach (SOAdjust soadj in PXSelectJoin<SOAdjust, InnerJoin<AR.ARPayment, On<AR.ARPayment.docType, Equal<SOAdjust.adjgDocType>, 
																						 And<AR.ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>>>>, 
															  Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, 
																	And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>, 
																		And<AR.ARPayment.openDoc, Equal<True>>>>>.Select(Base, orderShipment.OrderType, orderShipment.OrderNbr))
			{
				ARAdjust2 prev_adj = null;
				bool found = false;

				PXResultset<ARAdjust2> resultset = null;

				try
				{
					Base.TransferApplicationFromSalesOrder = true;
					resultset = Base.Adjustments.Select();
				}
				finally
				{
					Base.TransferApplicationFromSalesOrder = false;
				}

				foreach (ARAdjust2 adj in resultset)
				{
					if (Calculated)
					{
						CuryApplAmt -= adj.CuryAdjdAmt;
					}

					if (string.Equals(adj.AdjgDocType, soadj.AdjgDocType) && string.Equals(adj.AdjgRefNbr, soadj.AdjgRefNbr))
					{
						if (soadj.CuryAdjdAmt > 0m)
						{
							ARAdjust2 copy = PXCache<ARAdjust2>.CreateCopy(adj);
							copy.CuryAdjdAmt += (soadj.CuryAdjdAmt > adj.CuryDocBal) ? adj.CuryDocBal : soadj.CuryAdjdAmt;
							copy.CuryAdjdOrigAmt = copy.CuryAdjdAmt;
							copy.AdjdOrderType = soadj.AdjdOrderType;
							copy.AdjdOrderNbr = soadj.AdjdOrderNbr;
							prev_adj = Base.Adjustments.Update(copy);
						}

						found = true;

						if (Calculated)
						{
							CuryApplAmt += adj.CuryAdjdAmt;
							break;
						}
					}

					CuryApplAmt += adj.CuryAdjdAmt;
				}

				//if soadjust is not available in adjustments mark as billed
				if (!found)
				{
					/*
                        soadj.Billed = true;
                        soadjustments.Cache.SetStatus(soadj, PXEntryStatus.Updated);
                    */
				}

				Calculated = true;

				if (!Base.IsExternalTax(Base.Document.Current.TaxZoneID) && prev_adj != null)
				{
					prev_adj = PXCache<ARAdjust2>.CreateCopy(prev_adj);

					decimal curyDocBalance = (Base.Document.Current.CuryDocBal ?? 0m);
					decimal curyApplDifference = (CuryApplAmt ?? 0m) - curyDocBalance;

					if (CuryApplAmt > curyDocBalance)
					{
						if (prev_adj.CuryAdjdAmt > curyApplDifference)
						{
							prev_adj.CuryAdjdAmt -= curyApplDifference;
							CuryApplAmt = curyDocBalance;
						}
						else
						{
							CuryApplAmt -= prev_adj.CuryAdjdAmt;
							prev_adj.CuryAdjdAmt = 0m;
						}
						prev_adj = Base.Adjustments.Update(prev_adj);
					}
				}
			}

			newdoc = (ARInvoice)Base.Document.Cache.CreateCopy(Base.Document.Current);
			newdoc.OrigDocDate = origInvoiceDate;

			PXFormulaAttribute.CalcAggregate<ARAdjust2.adjdRefNbr>(Base.Adjustments.Cache, newdoc, false);
			PXFormulaAttribute.CalcAggregate<ARAdjust2.curyAdjdAmt>(Base.Adjustments.Cache, newdoc, false);
			PXFormulaAttribute.CalcAggregate<ARAdjust2.curyAdjdWOAmt>(Base.Adjustments.Cache, newdoc, false);

			List<string> ordersdistinct = new List<string>();
			foreach (SOOrderShipment shipments in PXSelect<SOOrderShipment, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(Base))
			{
				string key = string.Format("{0}|{1}", shipments.OrderType, shipments.OrderNbr);
				if (!ordersdistinct.Contains(key))
				{
					ordersdistinct.Add(key);
				}

				if (list != null && ordersdistinct.Count > 1)
				{
					newdoc.InvoiceNbr = null;
					newdoc.SalesPersonID = null;
					newdoc.DocDesc = null;
					break;
				}

				#region Update FreeItemQty for DiscountDetails based on shipments

				PXSelectBase<SOShipmentDiscountDetail> selectShipmentDiscounts = new PXSelect<SOShipmentDiscountDetail,
						Where<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
						And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
						And<SOShipmentDiscountDetail.shipmentNbr, Equal<Required<SOShipmentDiscountDetail.shipmentNbr>>>>>>(Base);

				foreach (SOShipmentDiscountDetail sdd in selectShipmentDiscounts.Select(shipments.OrderType, shipments.OrderNbr, shipments.ShipmentNbr))
				{
					bool discountDetailLineExist = false;

					foreach (ARInvoiceDiscountDetail idd in Base.ARDiscountDetails.Select())
					{
						if (idd.DocType == newdoc.DocType && idd.RefNbr == newdoc.RefNbr
							&& idd.OrderType == shipments.OrderType && idd.OrderNbr == shipments.OrderNbr
							&& idd.DiscountID == sdd.DiscountID && idd.DiscountSequenceID == sdd.DiscountSequenceID)
						{
							discountDetailLineExist = true;
							if (idd.FreeItemID == null)
							{
								idd.FreeItemID = sdd.FreeItemID;
								idd.FreeItemQty = sdd.FreeItemQty;
							}
							else
								idd.FreeItemQty = sdd.FreeItemQty;
						}
					}

					if (!discountDetailLineExist)
					{
						var idd = new ARInvoiceDiscountDetail
						{
							Type = DiscountType.Group,
							DocType = newdoc.DocType,
							RefNbr = newdoc.RefNbr,
							OrderType = sdd.OrderType,
							OrderNbr = sdd.OrderNbr,
							DiscountID = sdd.DiscountID,
							DiscountSequenceID = sdd.DiscountSequenceID,
							FreeItemID = sdd.FreeItemID,
							FreeItemQty = sdd.FreeItemQty
						};

						ARDiscountEngine.InsertDiscountDetail(Base.ARDiscountDetails.Cache, Base.ARDiscountDetails, idd);
					}
				}

				#endregion
			}

			Base.Document.Update(newdoc);
			SOOpenPeriodAttribute.SetValidatePeriod<ARInvoice.finPeriodID>(Base.Document.Cache, null, PeriodValidation.DefaultSelectUpdate);

			void RestoreHold(bool toValue)
			{
				var doc = Base.Document.Current;
				if (doc.CuryDocBal >= 0 && doc.Hold != toValue)
				{
					doc.Hold = toValue;
					Base.Document.Update(doc);
				}
			}

			if (list == null)
			{
				RestoreHold(prevHoldState);
			}
			else
			{
				if (Base.Transactions.Search<ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOShipmentType, ARTran.sOShipmentNbr>(
						orderShipment.OrderType, orderShipment.OrderNbr, orderShipment.ShipmentType, orderShipment.ShipmentNbr).Count > 0)
				{
					try
					{
						Base.Document.Current.ApplyPaymentWhenTaxAvailable = true;

						if (soOrderType.AutoWriteOff == true)
						{
							AutoWriteOffBalance(customer);
						}

						RestoreHold(prevHoldState);

						Base.Save.Press();
					}
					finally
					{
						Base.Document.Current.ApplyPaymentWhenTaxAvailable = false;
					}


					if (list.Find(Base.Document.Current) == null)
					{
						list.Add(Base.Document.Current, Base.SODocument.Current, Base.currencyinfo.Select());
					}
				}
				else
				{
					Base.Clear();
				}
			}

			if (customerCreditExtension != null)
			{
				customerCreditExtension.RemovePreUpdatedEvent(typeof(ARInvoice), ApprovedBalanceCollector);
			}
			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualLineCalc);
		}

		public ARInvoice FindOrCreateInvoice2(DateTime orderInvoiceDate, PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> order, 
											  InvoiceList list, Boolean groupByDefaultOperation)
		{
			SOOrder soOrder = order;
			SOOrderShipment orderShipment = order;

			if (orderShipment.BillShipmentSeparately == true)
			{
				ARInvoice newdoc = list.Find(new FieldLookup<ARInvoice.hidden>(false),
							 				 new FieldLookup<ARInvoice.hiddenByShipment>(true),
							 			   	 new FieldLookup<ARInvoice.hiddenShipmentType>(orderShipment.ShipmentType),
											 new FieldLookup<ARInvoice.hiddenShipmentNbr>(orderShipment.ShipmentNbr),
											 new FieldLookup<ARInvoice.taxCalcMode>(soOrder.TaxCalcMode));
				return newdoc ?? new ARInvoice()
				{
					HiddenShipmentType = orderShipment.ShipmentType,
					HiddenShipmentNbr = orderShipment.ShipmentNbr,
					HiddenByShipment = true
				};
			}
			else if (soOrder.PaymentCntr != 0 || soOrder.BillSeparately == true || Base.IsCreditCardProcessing(soOrder))
			{
				ARInvoice newdoc = list.Find(new FieldLookup<ARInvoice.hidden>(true),
											 new FieldLookup<ARInvoice.hiddenByShipment>(false),
											 new FieldLookup<ARInvoice.hiddenOrderType>(soOrder.OrderType),
											 new FieldLookup<ARInvoice.hiddenOrderNbr>(soOrder.OrderNbr));
				return newdoc ?? new ARInvoice()
				{
					HiddenOrderType = soOrder.OrderType,
					HiddenOrderNbr = soOrder.OrderNbr,
					Hidden = true
				};
			}
			else
			{
				SOOrderType soOrderType = SOOrderType.PK.Find(Base, soOrder.OrderType);

				string invoiceDocType = Base.GetInvoiceDocType(soOrderType, groupByDefaultOperation ? soOrderType.DefaultOperation : orderShipment.Operation);
				string orderTermsID = invoiceDocType == ARDocType.CreditMemo ? null : soOrder.TermsID;

				var invoiceSearchValues = new List<FieldLookup>()
				{
					new FieldLookup<ARInvoice.hidden>(false),
					new FieldLookup<ARInvoice.hiddenByShipment>(false),
					new FieldLookup<ARInvoice.docType>(invoiceDocType),
					new FieldLookup<ARInvoice.docDate>(orderInvoiceDate),
					new FieldLookup<ARInvoice.branchID>(soOrder.BranchID),
					new FieldLookup<ARInvoice.customerID>(soOrder.CustomerID),
					//new FieldLookup<ARInvoice.customerLocationID>(soOrder.CustomerLocationID),
					new FieldLookup<ARInvoice.taxZoneID>(soOrder.TaxZoneID),
					new FieldLookup<ARInvoice.taxCalcMode>(soOrder.TaxCalcMode),
					new FieldLookup<ARInvoice.curyID>(soOrder.CuryID),
					//new FieldLookup<ARInvoice.termsID>(orderTermsID),
					//new FieldLookup<SOInvoice.billAddressID>(soOrder.BillAddressID),
					//new FieldLookup<SOInvoice.billContactID>(soOrder.BillContactID),
				};

				if (invoiceDocType != ARDocType.CreditMemo)
				{
					invoiceSearchValues.Add(new FieldLookup<SOInvoice.extRefNbr>(soOrder.ExtRefNbr));
					if (soOrder.PaymentMethodID != null)
					{
						invoiceSearchValues.Add(new FieldLookup<SOInvoice.pMInstanceID>(soOrder.PMInstanceID));
					}
					if (soOrder.CashAccountID != null)
					{
						invoiceSearchValues.Add(new FieldLookup<SOInvoice.cashAccountID>(soOrder.CashAccountID));
					}
				}

				CurrencyInfo orderCuryInfo = order;

				invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyRateTypeID>(orderCuryInfo.CuryRateTypeID));

				if (soOrderType.UseCuryRateFromSO == true)
				{
					invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyMultDiv>(orderCuryInfo.CuryMultDiv));
					invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyRate>(orderCuryInfo.CuryRate));
				}

				return list.Find(invoiceSearchValues.ToArray()) ?? new ARInvoice();
			}
		}
        #endregion

        #region Copy Standard Protection Level Methods
        protected DiscountEngine<ARTran, ARInvoiceDiscountDetail> ARDiscountEngine => DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>();

		public delegate void InvoiceCreatedDelegate(ARInvoice invoice, SOOrder source);
		protected virtual void InvoiceCreated(ARInvoice invoice, SOOrder source) { }

        protected virtual void DefaultShippingAddress(ARInvoice newdoc, SOOrderShipment orderShipment, SOShipment soShipment)
        {
            var soShipAddress = SOAddress.PK.Find(Base, orderShipment.ShipAddressID);
            if (!ExternalTax.IsExternalTax(Base, newdoc.TaxZoneID)
                || !ExternalTax.IsEmptyAddress(soShipAddress))
            {
                ARShippingAddressAttribute.CopyRecord<ARInvoice.shipAddressID>(Base.Document.Cache, newdoc, soShipAddress, true);
                return;
            }

            if (soShipment?.WillCall == true)
            {
                var site = INSite.PK.Find(Base, soShipment.SiteID);
                var shipToAddress = PX.Objects.CR.Address.PK.Find(Base, site.AddressID);
                if (!ExternalTax.IsEmptyAddress(shipToAddress))
                {
                    ARShippingAddressAttribute.DefaultAddress<ARShippingAddress, ARShippingAddress.addressID>(Base.Document.Cache,
                                                                                                              nameof(ARInvoice.shipAddressID), newdoc, null,
                                                                                                              new PXResult<PX.Objects.CR.Address, ARShippingAddress>(shipToAddress, new ARShippingAddress()));
                    return;
                }
            }

            var order = SOOrderShipment.FK.Order.FindParent(Base, orderShipment);
            var soOrderAddress = SOAddress.PK.Find(Base, order?.ShipAddressID);
            if (!ExternalTax.IsEmptyAddress(soOrderAddress))
            {
                ARShippingAddressAttribute.CopyRecord<ARInvoice.shipAddressID>(Base.Document.Cache, newdoc, soOrderAddress, true);
            }
        }

        protected virtual void ChangeBalanceSign(ARTran tran, ARInvoice newdoc, string soLineOperation)
		{
			if (newdoc.DrCr == DrCr.Credit && soLineOperation == SOOperation.Receipt ||
				newdoc.DrCr == DrCr.Debit && soLineOperation == SOOperation.Issue)
			{
				//keep BaseQty positive for PXFormula
				tran.Qty = -tran.Qty;
				tran.CuryDiscAmt = -tran.CuryDiscAmt;
				tran.CuryTranAmt = -tran.CuryTranAmt;
				tran.CuryExtPrice = -tran.CuryExtPrice;
			}
		}

		protected virtual void AutoWriteOffBalance(Customer customer)
		{
			foreach (ARAdjust2 adjustment in Base.Adjustments_Inv.Select())
			{
				decimal applDifference = (adjustment.CuryAdjdAmt ?? 0m) - (adjustment.CuryAdjdOrigAmt ?? 0m);
				if (customer != null && customer.SmallBalanceAllow == true && applDifference != 0m && Math.Abs(customer.SmallBalanceLimit ?? 0m) >= Math.Abs(applDifference))
				{
					ARAdjust2 upd_adj = PXCache<ARAdjust2>.CreateCopy(adjustment);
					upd_adj.CuryAdjdAmt = upd_adj.CuryAdjdOrigAmt;
					upd_adj.CuryAdjdWOAmt = applDifference;
					upd_adj = Base.Adjustments.Update(upd_adj);
				}
			}

			if (Base.Document.Current.CuryApplicationBalance != 0m)
			{
				ARAdjust2 firstAdjustment = Base.Adjustments_Inv.SelectSingle();
				if (firstAdjustment != null)
				{
					ARAdjust2 upd_adj = PXCache<ARAdjust2>.CreateCopy(firstAdjustment);

					decimal applDifference = Base.Document.Current.CuryApplicationBalance ?? 0m;

					if (customer != null && customer.SmallBalanceAllow == true && Math.Abs(customer.SmallBalanceLimit ?? 0m) >= Math.Abs(applDifference))
					{
						upd_adj.CuryAdjdWOAmt = -applDifference;
						upd_adj = Base.Adjustments.Update(upd_adj);
					}
				}
			}
		}
		#endregion
	}
}