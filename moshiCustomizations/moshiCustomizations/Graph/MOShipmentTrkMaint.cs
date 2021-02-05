using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Linq;

namespace moshiCustomizations.Graph
{
    public class MOShipmentTrkMaint : PXGraph<MOShipmentTrkMaint>
    {
        #region Select & Features        
        public PXSavePerRow<SOShipment> Save;
        public PXCancel<SOShipment> Cancel;
        [PXFilterable()]
        public SelectFrom<SOShipment>.View Shipment;
        #endregion

        #region Ctor
        public MOShipmentTrkMaint()
        {
            ActionMenu.AddMenuAction(Reset2UnsentCus);
            ActionMenu.AddMenuAction(Reset2UnsentCar);
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<SOShipment> e)
        {
            PXUIFieldAttribute.SetEnabled<SOShipment.shipmentType>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<SOShipment.shipmentNbr> (e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<SOShipment.customerID>  (e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<SOShipment.shipmentQty> (e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<SOShipment.shipDate>    (e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<SOShipment.siteID>      (e.Cache, e.Row, false);
            //PXUIFieldAttribute.SetEnabled<SOShipment.selected>    (e.Cache, e.Row, !string.IsNullOrEmpty(e.Row.GetExtension<SOShipmentExt>().UsrTrackingNbr));
        }
        #endregion

        #region Actions & Menu
        public PXAction<SOShipment> ActionMenu;
        [PXButton(MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Actions")]
        public IEnumerable actionMenu(PXAdapter adapter) => adapter.Get();

        public PXAction<SOShipment> Reset2UnsentCus;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Reset to unsent_Customer", MapEnableRights = PXCacheRights.Select)]
        public IEnumerable reset2UnsentCus(PXAdapter adapter)
        {
            foreach (SOShipment row in adapter.Get<SOShipment>().RowCast<SOShipment>().Where(ship => ship.GetExtension<SOShipmentExt>().UsrSentCustomer == true && ship.Selected == true))
            {
                this.Shipment.Cache.SetValue<SOShipmentExt.usrSentCustomer>(row, false);
                this.Shipment.Cache.MarkUpdated(row);
            }

            Actions.PressSave();

            return adapter.Get();
        }

        public PXAction<SOShipment> Reset2UnsentCar;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Reset to unsent_Carrier", MapEnableRights = PXCacheRights.Select)]
        public IEnumerable reset2UnsentCar(PXAdapter adapter)
        {
            foreach (SOShipment row in adapter.Get<SOShipment>().RowCast<SOShipment>().Where(ship => ship.GetExtension<SOShipmentExt>().UsrSentCarrier == true && ship.Selected == true))
            {
                this.Shipment.Cache.SetValue<SOShipmentExt.usrSentCarrier>(row, false);
                this.Shipment.Cache.MarkUpdated(row);
            }

            Actions.PressSave();

            return adapter.Get();
        }
        #endregion
    }
}
