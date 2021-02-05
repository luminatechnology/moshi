using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.SO
{
    public class SOSetupExt : PXCacheExtension<SOSetup>
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Don't Allow Edit Shipped Qty. In Open")]
        public virtual bool? UsrDisableShippedQty { get; set; }
        public abstract class usrDisableShippedQty : PX.Data.BQL.BqlBool.Field<SOSetupExt.usrDisableShippedQty> { }
    }
}
