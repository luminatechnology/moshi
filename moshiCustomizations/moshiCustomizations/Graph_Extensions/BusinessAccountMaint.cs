using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;

namespace PX.Objects.CR
{
    public class BusinessAccountMaint_Extension : PXGraphExtension<BusinessAccountMaint>
    {
        public SelectFrom<ARInvoice>.Where<ARInvoice.customerID.IsEqual<BAccount.bAccountID.FromCurrent>>.View ARInvoices;

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(BAccount.status.Hold)]
        protected void _(Events.CacheAttached<BAccount.status> e) { }
        #endregion
    }
}
