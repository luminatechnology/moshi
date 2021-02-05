using PX.Data;
using PX.Objects.SO;

namespace moshiCustomizations.Graph
{
    public class MOSOOrderProcess : PXGraph<MOSOOrderProcess>
    {
        public PXCancel<SOProcessFilter> Cancel;
        public PXFilter<SOProcessFilter> Filter;

        [PXFilterable()]
        public PXFilteredProcessing<SOOrder, SOProcessFilter, Where<SOOrder.destinationSiteID, IsNotNull>> Records;

        protected void _(Events.RowSelected<SOProcessFilter> e)
        {
          if (string.IsNullOrEmpty(e.Row.Action)) { return; }
            
          Records.SetProcessWorkflowAction(e.Row.Action, this.Filter.Cache.ToDictionary(e.Row));
        }
    }
}
