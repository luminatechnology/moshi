using PX.Data;
using System;

namespace PX.Objects.SO
{
    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
    {
        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault("1")]
        protected void _(Events.CacheAttached<SOLine.orderQty> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<SOLine> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null)
                return;
            PXCache cache = e.Cache;
            SOLine row = e.Row;

            bool? disableFreeItemCtrl = this.Base.soordertype.Current.GetExtension<SOOrderTypeExt>().UsrDisableFreeItemCtrl;
            bool? nullable = disableFreeItemCtrl.HasValue ? new bool?(!disableFreeItemCtrl.GetValueOrDefault()) : new bool?();

            int num = nullable.HasValue ? (nullable.GetValueOrDefault() ? 1 : 0) : 1;

            PXUIFieldAttribute.SetEnabled<SOLine.isFree>(cache, (object)row, num != 0);
        }

        protected void _(Events.FieldUpdated<SOLine.inventoryID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            SOLine row = e.Row as SOLine;

            bool? usrFreeItemCtrl = this.Base.soordertype.Current.GetExtension<SOOrderTypeExt>().UsrFreeItemCtrl;

            if (row == null || !usrFreeItemCtrl.HasValue)
                return;

            row.ManualDisc = row.IsFree = usrFreeItemCtrl;

            Base.Transactions.Cache.RaiseFieldUpdated<SOLine.isFree>(row, false);
        }

        protected void _(Events.FieldUpdated<SOLine.curyUnitPrice> e)
        {
            if (!(e.Row is SOLine row) || e.OldValue == null || !e.ExternalCall) { return; }
            
            e.Cache.SetValueExt<SOLineExt.usrPriceWarn>(row, (decimal)e.OldValue != 0M && (decimal)e.OldValue > (decimal)e.NewValue);
        }
        #endregion
    }
}
