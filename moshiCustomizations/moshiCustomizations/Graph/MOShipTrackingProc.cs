using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.SO;

namespace moshiCustomizations.Graph
{
    public class MOShipTrackingProc : PXGraph<MOShipTrackingProc>
    {
        #region Constant Class
        public partial class Carrier_UPS : PX.Data.BQL.BqlString.Constant<Carrier_UPS>
        {
            public Carrier_UPS() : base("UPS") { }
        }
        #endregion

        #region Features
        public PXCancel<SOProcessFilter> Cancel;
        public PXFilter<SOProcessFilter> Filter;

        [PXFilterable()]
        public PXFilteredProcessingJoinGroupBy<SOShipment, SOProcessFilter, InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentType, Equal<SOShipment.shipmentType>,
                                                                                                          And<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>>>,
                                                                            Where<SOShipmentExt.usrTrackingNbr, IsNotNull,
                                                                                  And<SOShipment.shipmentType, NotEqual<SOShipmentType.transfer>,
                                                                                      And<SOOrderShipment.invoiceNbr, IsNotNull>>>,
                                                                            Aggregate<GroupBy<SOOrderShipment.shipmentNbr>>> Records;
        //public PXFilteredProcessing<SOShipment, SOProcessFilter, Where<SOShipmentExt.usrTrackingNbr, IsNotNull,
        //                                                               And<SOShipment.shipmentType, NotEqual<SOShipmentType.transfer>>>> Records;
        #endregion

        #region Delegate Data View
        public virtual IEnumerable records()
        {
            SOProcessFilter filter = Filter.Current;

            List<object> lists = new List<object>();

            if (filter.Action == PXAutomationMenuAttribute.Undefined) { yield break; }
            
            PXView view = new PXView(this, true, Records.View.BqlSelect);

            int totalRow = 0;
            int startRow = PXView.StartRow;

            switch (filter.Action)
            {
                case SOShipmentScreenExt.SendToCustomer:
                    view.WhereAnd<Where<SOShipmentExt.usrSentCustomer, Equal<False>, Or<SOShipmentExt.usrSentCustomer, IsNull>>>();
                    break;
                case SOShipmentScreenExt.SendToUPS:
                    view.WhereAnd<Where<SOShipmentExt.usrSentCarrier, Equal<False>, Or<SOShipmentExt.usrSentCarrier, IsNull>>>();
                    //view.WhereAnd<Where<SOShipmentExt.usrCarrier, Equal<Carrier_UPS>>>();
                    break;
            }

            foreach (var row in view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRow) )
            {
                yield return row;
            }

            PXView.StartRow = 0;
        }
        #endregion

        #region Event Handler
        protected void _(Events.RowSelected<SOProcessFilter> e)
        {
            if (string.IsNullOrEmpty(e.Row.Action)) { return; }

            Records.SetProcessWorkflowAction(e.Row.Action, this.Filter.Cache.ToDictionary(e.Row));
        }
        #endregion
    }
}