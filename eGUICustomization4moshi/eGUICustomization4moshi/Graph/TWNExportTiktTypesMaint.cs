using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4moshi.DAC;

namespace eGUICustomization4moshi.Graph
{
    public class TWNExportTiktTypesMaint : PXGraph<TWNExportTiktTypesMaint>
    {
        public PXSavePerRow<TWNExportTicketTypes> Save;
        public PXCancel<TWNExportTicketTypes> Cancel;

        [PXImport(typeof(TWNExportTicketTypes))]
        public SelectFrom<TWNExportTicketTypes>.View ExportTktTypes;
    }
}