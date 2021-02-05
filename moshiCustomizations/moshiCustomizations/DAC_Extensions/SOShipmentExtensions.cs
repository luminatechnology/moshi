using PX.Data;
using PX.Data.BQL;
using static PX.Objects.SO.SOShipmentEntry_Extension;

namespace PX.Objects.SO
{
    public class SOShipmentExt : PXCacheExtension<SOShipment>
    {
        #region UsrTrackingNbr
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tracking Nbr.")]
        public virtual string UsrTrackingNbr { get; set; }
        public abstract class usrTrackingNbr : PX.Data.BQL.BqlString.Field<usrTrackingNbr> { }
        #endregion

        #region UsrPacking
        [PXDBBool]
        [PXUIField(DisplayName = "Packing")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrPacking { get; set; }
        public abstract class usrPacking : PX.Data.BQL.BqlBool.Field<usrPacking> { }
        #endregion

        #region UsrCarrier
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Carrier")]
        [PXSelector(typeof(Search<PX.Objects.CS.CarrierPlugin.carrierPluginID>), CacheGlobal = true)]
        public virtual string UsrCarrier { get; set; }
        public abstract class usrCarrier : PX.Data.BQL.BqlString.Field<usrCarrier> { }
        #endregion

        #region UsrSentCustomer
        [PXDBBool]
        [PXUIField(DisplayName = "Sent Customer", IsReadOnly = true)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrSentCustomer { get; set; }
        public abstract class usrSentCustomer : PX.Data.BQL.BqlBool.Field<usrSentCustomer> { }
        #endregion

        #region UsrSentCarrier
        [PXDBBool]
        [PXUIField(DisplayName = "Sent Carrier", IsReadOnly = true)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrSentCarrier { get; set; }
        public abstract class usrSentCarrier : PX.Data.BQL.BqlBool.Field<usrSentCarrier> { }
        #endregion

        #region UsrCarrierURL
        [PXString(255)]
        [PXUIField(DisplayName = "Carrier Tracking URL", IsReadOnly = true)]
        public virtual string UsrCarrierURL
        {
            [PXDependsOnFields(typeof(SOShipmentExt.usrCarrier), typeof(SOShipmentExt.usrTrackingNbr))]
            get
            {
                return GetCarrierTrackURL(this.UsrCarrier, this.UsrTrackingNbr);
            }
        }
        public abstract class usrCarrierURL : PX.Data.BQL.BqlString.Field<usrCarrierURL> { }
        #endregion
    }
}
