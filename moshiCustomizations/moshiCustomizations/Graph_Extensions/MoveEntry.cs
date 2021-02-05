using PX.Data;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.AM
{
    public class MoveEntry_Extension : PXGraphExtension<MoveEntry>
    {
        public PXAction<AMBatch> report;
        public PXAction<AMBatch> printShipMark;

        public override void Initialize()
        {
            base.Initialize();
            this.report.AddMenuAction((PXAction)this.printShipMark);
        }

        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable Report(PXAdapter adapter, [PXString(8, InputMask = "CC.CC.CC.CC")] string reportID) => adapter.Get();

        [PXUIField(DisplayName = "Print Shipping Mark", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual void PrintShipMark()
        {
            if (Base.batch.Current != null)
            {
                throw new PXReportRequiredException(new Dictionary<string, string>()
                {
                    ["BatNbr"] = Base.batch.Current.BatNbr
                }, "AM603020");
            }
        }
    }
}
