using PX.Data;

namespace PX.Objects.SO
{
    public class SOOrderTypeMaint_Extension : PXGraphExtension<SOOrderTypeMaint>
    {
        protected void _(PX.Data.Events.RowSelected<SOOrderType> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            PXUIFieldAttribute.SetEnabled<SOOrderTypeExt.usrFreeItemCtrl>(e.Cache, e.Row, e.Row.GetExtension<SOOrderTypeExt>().UsrDisableFreeItemCtrl?? false);
        }
    }
}
