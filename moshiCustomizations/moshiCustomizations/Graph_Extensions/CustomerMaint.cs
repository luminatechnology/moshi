using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AR
{
    public class CustomerMaint_Extension : PXGraphExtension<CustomerMaint>
    {
        protected void _(Events.FieldUpdated<Customer.status> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            Customer row = e.Row as Customer;

            if (row.Type == BAccountType.ProspectType)
            {
                row.Status = BAccount.status.Hold;
            }
        }
    }
}
