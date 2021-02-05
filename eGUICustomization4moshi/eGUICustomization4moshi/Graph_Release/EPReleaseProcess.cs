﻿using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.TX;
using eGUICustomization4moshi.DAC;
using eGUICustomization4moshi.Graph_Release;
using eGUICustomization4moshi.Descriptor;
using static eGUICustomization4moshi.Descriptor.TWNStringList;

namespace PX.Objects.EP
{
    public class EPReleaseProcess_Extension : PXGraphExtension<EPReleaseProcess>
    {
        #region Delegate Functions
        public delegate void ReleaseDocProcDelegate(EPExpenseClaim claim);
        [PXOverride]
        public void ReleaseDocProc(EPExpenseClaim claim, ReleaseDocProcDelegate baseMethod)
        {
            baseMethod(claim);

            if (TWNGUIValidation.ActivateTWGUI(Base) == true &&
                claim != null &&
                claim.Released == true )
            {
                ExpenseClaimEntry graph = PXGraph.CreateInstance<ExpenseClaimEntry>();
                TWNReleaseProcess rp    = PXGraph.CreateInstance<TWNReleaseProcess>();

                Vendor vendor = new Vendor();

                foreach (TWNManualGUIExpense manualGUIExp in graph.GetExtension<ExpenseClaimEntry_Extension>().manGUIExpense.Cache.Cached)
                {
                    if (PXCache<Tax>.GetExtension<TaxExt>(Tax.PK.Find(Base, manualGUIExp.TaxID)).UsrTWNGUI == false) { continue; }

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        if (manualGUIExp.VendorID != null)
                        {
                            vendor = SelectFrom<Vendor>.Where<Vendor.bAccountID.IsEqual<@P.AsInt>>.View.SelectSingleBound(Base, null, manualGUIExp.VendorID);
                        }

                        rp.CreateGUITrans(new STWNGUITran()
                        {
                            VATCode       = manualGUIExp.VATInCode,
                            GUINbr        = manualGUIExp.GUINbr,
                            GUIStatus     = TWNGUIStatus.Used,
                            BranchID      = graph.ExpenseClaimDetailsCurrent.Current.BranchID,
                            GUIDirection  = TWNGUIDirection.Receipt,
                            GUIDate       = manualGUIExp.GUIDate,
                            GUITitle      = vendor.AcctName,
                            TaxZoneID     = manualGUIExp.TaxZoneID,
                            TaxCategoryID = manualGUIExp.TaxCategoryID,
                            TaxID         = manualGUIExp.TaxID,
                            TaxNbr        = manualGUIExp.TaxNbr,
                            OurTaxNbr     = manualGUIExp.OurTaxNbr,
                            NetAmount     = manualGUIExp.NetAmt,
                            TaxAmount     = manualGUIExp.TaxAmt,
                            AcctCD        = vendor.AcctCD,
                            AcctName      = vendor.AcctName,
                            DeductionCode = manualGUIExp.Deduction,
                            Remark        = manualGUIExp.Remark,
                            OrderNbr      = manualGUIExp.RefNbr
                        });

                        ts.Complete(Base);
                    }
                }
            }
        }
        #endregion
    }
}