using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using System;

namespace PX.Objects.SO
{
    public class SOPackageDetailExt : PXCacheExtension<SOPackageDetail>
    {
        [PXDBInt]
        [PXUIField(DisplayName = "Shipment Split Line Nbr.")]
        [PXSelector(typeof (Search<SOShipLine.lineNbr, Where<SOShipLine.shipmentNbr, Equal<Current<SOPackageDetail.shipmentNbr>>>>), 
                    typeof (SOShipLine.lineNbr), 
                    typeof (SOShipLine.origOrderType),
                    typeof (SOShipLine.origOrderNbr), 
                    typeof (SOShipLine.inventoryID), 
                    typeof (SOShipLine.shippedQty), 
                    typeof (SOShipLine.packedQty), 
                    typeof (SOShipLine.uOM), 
                    ValidateValue = false)]
        public virtual int? UsrShipSplitLineNbr { get; set; }
        public abstract class usrShipSplitLineNbr : PX.Data.BQL.BqlInt.Field<SOPackageDetailExt.usrShipSplitLineNbr> { }

        #region Override DAC Field       
            #region Qty
            public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
            protected Decimal? _Qty;
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
                                // Temporarily override this field attribute, because it can't work properly on extension graph.
            [PXUIField(DisplayName = "Qty", Enabled = true /*false*/)]
            public virtual Decimal? Qty
            {
                get
                {
                    return this._Qty;
                }
                set
                {
                    this._Qty = value;
                }
            }
            #endregion
        #endregion
    }
}
