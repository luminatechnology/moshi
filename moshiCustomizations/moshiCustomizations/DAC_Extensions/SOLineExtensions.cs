using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.SO
{
    public class SOLineExt : PXCacheExtension<SOLine>
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Price Warning", Enabled = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrPriceWarn { get; set; }
        public abstract class usrPriceWarn : BqlType<IBqlBool, bool>.Field<SOLineExt.usrPriceWarn> { }
    }
}
