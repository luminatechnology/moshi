using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using static PX.Objects.SO.SOInvoiceShipment.WellKnownActions;

namespace PX.Objects.SO
{
    public class SOInvoiceShipment_Extension : PXGraphExtension<SOInvoiceShipment>
    {
        #region Delegate Method        
        public delegate void ApplyShipmentFiltersDelegate(PXSelectBase<SOShipment> shCmd, SOShipmentFilter filter);
        [PXOverride]
        public void ApplyShipmentFilters(PXSelectBase<SOShipment> shCmd, SOShipmentFilter filter, ApplyShipmentFiltersDelegate baseMethod)
        {
            switch (filter.Action)
            {
                case SOShipmentScreenExt.PrintStockoutForm:
                    if (filter.ShowPrinted == false)
                    {
                        shCmd.WhereAnd<Where<SOShipment.status, Equal<SOShipmentStatus.open>>>();
                        shCmd.WhereAnd<Where<SOShipmentExt.usrPacking, IsNull, Or<SOShipmentExt.usrPacking, Equal<False>>>>();
                    }
                    else
                    {
                        shCmd.WhereNew<Where<SOShipment.status, Equal<SOShipmentStatus.open>, Or<SOShipment.status, Equal<SOShipmentStatus.confirmed>>>>();
                    }

                    shCmd.WhereAnd<Where<SOShipment.ownerID, Equal<Current<SOShipmentFilterExt.usrOwnerID>>>>();
                    break;

                case SOShipmentScreenExt.CreateMonInv:
                    shCmd.WhereAnd<Where<SOShipment.confirmed.IsEqual<True>.
                                         And<Match<Customer, AccessInfo.userName.FromCurrent>>.
                                             And<Match<INSite, AccessInfo.userName.FromCurrent>>.
                                                 And<Exists<SelectFrom<SOOrderShipment>.Where<SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
                                                                                              And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>.
                                                                                                  And<SOOrderShipment.invoiceNbr.IsNull>.
                                                                                                      And<SOOrderShipment.createARDoc.IsEqual<True>>>>>>>();
                    break;
            }

            baseMethod(shCmd, filter);
        }
        #endregion

        #region Event Handler
        protected void _(Events.RowSelected<SOShipmentFilter> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            SOShipmentFilter row = e.Row;

            if (e.Row != null)
            {
                PXUIFieldAttribute.SetVisible<SOShipmentFilter.showPrinted>  (e.Cache, e.Row, row.Action.Contains(SOShipmentScreen.PrintLabels.Substring(2)) ||
                                                                                              row.Action.Contains(SOShipmentScreen.PrintPickList.Substring(2)) ||
                                                                                              row.Action.Contains(SOShipmentScreenExt.PrintStockoutForm));
                PXUIFieldAttribute.SetVisible<SOShipmentFilterExt.usrOwnerID>(e.Cache, e.Row, row.Action.Contains(SOShipmentScreenExt.PrintStockoutForm));
            }
        }
        #endregion
    }

    #region Extension DAC
    public class SOShipmentFilterExt : PXCacheExtension<SOShipmentFilter>
    {
        [PX.TM.Owner(typeof(SOShipment.workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? UsrOwnerID { get; set; }
        public abstract class usrOwnerID : PX.Data.BQL.BqlInt.Field<SOShipmentFilterExt.usrOwnerID> { }
    }
    #endregion
}
