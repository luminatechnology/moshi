using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4moshi.DAC;

namespace eGUICustomization4moshi.Graph
{
    public class TWNGUIPrefMaint : PXGraph<TWNGUIPrefMaint>
    { 
        public PXSave<TWNGUIPreferences> Save;
        public PXCancel<TWNGUIPreferences> Cancel;

        public SelectFrom<TWNGUIPreferences>.View GUIPreferences;
    }
}