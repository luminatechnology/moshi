using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.SO
{
    public class SOOrderProcessSelectedExt : PXCacheExtension<SOOrderProcessSelected>
    {
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Shipping Attention", Visibility = PXUIVisibility.SelectorVisible)]
        [PXPersonalDataField]
        [PXDBScalar(typeof (Search<SOContact.attention, Where<SOContact.contactID, Equal<SOOrder.shipContactID>>>))]
        public virtual string UsrShipContactAtten { get; set; }
        public abstract class usrShipContactAtten : PX.Data.BQL.BqlString.Field<SOOrderProcessSelectedExt.usrShipContactAtten> { }
    }
}
