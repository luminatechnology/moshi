using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.SO
{
  public class SOOrderTypeExt : PXCacheExtension<SOOrderType>
  {
        [PXDBBool]
        [PXUIField(DisplayName = "Disable Free Item Control")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrDisableFreeItemCtrl { get; set; }
        public abstract class usrDisableFreeItemCtrl : PX.Data.BQL.BqlBool.Field<SOOrderTypeExt.usrDisableFreeItemCtrl> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Free Item")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrFreeItemCtrl { get; set; }
        public abstract class usrFreeItemCtrl : PX.Data.BQL.BqlBool.Field<SOOrderTypeExt.usrFreeItemCtrl> { }
  }
}
