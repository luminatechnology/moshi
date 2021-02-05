using PX.Data;

namespace PX.Objects.IN
{
    public class InventoryItemMaint_Extension : PXGraphExtension<InventoryItemMaint>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(InventoryItemStatus.NoSales)]
        protected void _(Events.CacheAttached<InventoryItem.itemStatus> e) { }
    }
}
