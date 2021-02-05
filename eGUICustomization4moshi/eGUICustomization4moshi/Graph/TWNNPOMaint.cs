using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4moshi.DAC;

namespace eGUICustomization4moshi.Graph
{
    public class TWNNPOMaint : PXGraph<TWNNPOMaint>
    { 
        public PXSavePerRow<TWNNPOTable> Save;
        public PXCancel<TWNNPOTable> Cancel;

        [PXImport(typeof(TWNNPOTable))]
        public SelectFrom<TWNNPOTable>.View NPOTable;
    }
}