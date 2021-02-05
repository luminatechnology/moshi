using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.SO
{
    public class SOShipLineExt : PXCacheExtension<SOShipLine>
    {
        [PXDBQuantity]
        [PXUIField(DisplayName = "Packing Qty.")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? UsrPackingQty { get; set; }
        public abstract class usrPackingQty : PX.Data.BQL.BqlDecimal.Field<SOShipLineExt.usrPackingQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Remaining Qty.", IsReadOnly = true)]
        [PXFormula(typeof (PX.Data.Sub<SOShipLine.shippedQty, SOShipLine.packedQty>))]
        public virtual decimal? UsrRemainingQty { get; set; }
        public abstract class usrRemainingQty : PX.Data.BQL.BqlDecimal.Field<SOShipLineExt.usrRemainingQty> { }

        [PXString]
        [PXUIField(DisplayName = "Carton Qty.", Enabled = false)]
        [PXDBScalar(typeof (Search2<CSAnswers.value, InnerJoin<InventoryItem, On<InventoryItem.noteID, Equal<CSAnswers.refNoteID>, 
                                                                                 And<CSAnswers.attributeID, Equal<SOShipmentEntry_Extension.CartonQtyAttr>>>>, 
                                                     Where<InventoryItem.inventoryID, Equal<SOShipLine.inventoryID>>>))]
        public virtual string UsrCartonQty { get; set; }
        public abstract class usrCartonQty : PX.Data.BQL.BqlString.Field<SOShipLineExt.usrCartonQty> { }
    }
}
