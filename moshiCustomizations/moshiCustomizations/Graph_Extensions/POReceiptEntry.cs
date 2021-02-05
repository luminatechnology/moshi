using PX.SM;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PO
{
    public class POReceiptEntry_Extension : PXGraphExtension<POReceiptEntry>
    {
        public const string revokeRoleName = "Subcon Remove PR add row";

        protected void _(Events.RowSelected<POReceipt> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row != null && e.Row.Status.IsIn(POReceiptStatus.Hold, POReceiptStatus.Balanced))
            {
                bool enabled = SelectFrom<UsersInRoles>.Where<UsersInRoles.username.IsEqual<@P.AsString>
                                                              .And<UsersInRoles.rolename.IsEqual<@P.AsString>>>
                                                       .View.ReadOnly.Select(Base, Base.Accessinfo.UserName, revokeRoleName).Count == 0;

                Base.addPOOrder.SetEnabled(enabled);
                Base.addPOReceiptLine.SetEnabled(enabled);
                Base.addPOReceiptReturn.SetEnabled(enabled);

                Base.transactions.AllowInsert = enabled;
            }
        }
    }
}
