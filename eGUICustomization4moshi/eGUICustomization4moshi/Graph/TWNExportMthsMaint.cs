using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4moshi.DAC;

namespace eGUICustomization4moshi.Graph
{
    public class TWNExportMthsMaint : PXGraph<TWNExportMthsMaint>
    {
        public PXSavePerRow<TWNExportMethods> Save;
        public PXCancel<TWNExportMethods> Cancel;

        [PXImport(typeof(TWNExportMethods))]
        public SelectFrom<TWNExportMethods>.View ExportMethods;
    }
}