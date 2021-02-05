using System;
using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using moshiCustomizations.DAC;
using System.Data;
using PX.Data.SQLTree;

namespace moshiCustomizations.Graph
{
    public class MOProductInfoEntry : PXGraph<MOProductInfoEntry, MOProdInfoTable>
    {
        public const string NonExistedItem = "The Stock Item Doesn't Exist In {0} Company";

        #region Selects
        public SelectFrom<MOProdInfoTable>.View ProdInfo;
        public SelectFrom<MOProdInfoTable>.Where<MOProdInfoTable.inventoryID.IsEqual<MOProdInfoTable.inventoryID.FromCurrent>>.View ProdInfoCur;
        #endregion

        #region Ctor
        public MOProductInfoEntry()
        {
            Action.AddMenuAction(Copy2Moshi);
            Action.AddMenuAction(Copy2USA);
            Action.AddMenuAction(Copy2Tyrian);
            Action.AddMenuAction(UpdateItemCntryOfOrig);
        }
        #endregion

        #region Menu & Actions
        public PXAction<MOProdInfoTable> Action;
        [PXUIField(DisplayName = PX.Objects.Common.Messages.Actions)]
        [PXButton(MenuAutoOpen = true)]
        public IEnumerable action(PXAdapter adapter) => adapter.Get();

        public PXAction<MOProdInfoTable> Copy2Moshi;
        [PXUIField(DisplayName = "Copy to moshi")]
        [PXButton(CommitChanges = true)]
        public IEnumerable copy2Moshi(PXAdapter adapter)
        { 
            PXLongOperation.StartOperation(this, delegate () { Sync2OtherTenant(TenantType.Moshi, ProdInfoCur.Current); });

            return adapter.Get();
        }

        public PXAction<MOProdInfoTable> Copy2USA;
        [PXUIField(DisplayName = "Copy to Aevoe USA")]
        [PXButton(CommitChanges = true)]
        public IEnumerable copy2USA(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate () { Sync2OtherTenant(TenantType.USA, ProdInfoCur.Current); });

            return adapter.Get();
        }

        public PXAction<MOProdInfoTable> Copy2Tyrian;
        [PXUIField(DisplayName = "Copy to Tyrian International BV")]
        [PXButton(CommitChanges = true)]
        public IEnumerable copy2Tyrian(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate () { Sync2OtherTenant(TenantType.Tyrian, ProdInfoCur.Current); });

            return adapter.Get();
        }

        public PXAction<MOProdInfoTable> UpdateItemCntryOfOrig;
        [PXUIField(DisplayName = "Update Country Of Origin")]
        [PXButton(CommitChanges = true)]
        public IEnumerable updateItemCntryOfOrig(PXAdapter adapter)
        {
            MOProdInfoTable prodInfo = ProdInfoCur.Current;

            if (prodInfo != null)
            {
                var item = InventoryItem.PK.Find(this, prodInfo.InventoryID);

                ProdInfoCur.SetValueExt<MOProdInfoTable.p6_CountryofOrigin>(prodInfo, item.CountryOfOrigin);
                ProdInfoCur.Cache.MarkUpdated(prodInfo);

                this.Save.Press();
            }

            return adapter.Get();
        }
        #endregion

        #region Static Methods
        protected static void Sync2OtherTenant(TenantType type, MOProdInfoTable prodInfo)
        {
            switch (type)
            {
                case TenantType.Moshi:
                    CrossCompany2SyncData("Moshi", prodInfo);
                    break;
                case TenantType.USA:
                    CrossCompany2SyncData("Aevoe USA", prodInfo);
                    break;
                case TenantType.Tyrian:
                    CrossCompany2SyncData("Tyrian International BV", prodInfo);
                    break;
            }
        }

        protected static void CrossCompany2SyncData(string tenantName, MOProdInfoTable prodInfo)
        {
            try
            {
                MOProductInfoEntry graph = PXGraph.CreateInstance<MOProductInfoEntry>();

                string loginStr = string.Format("{0}@{1}", graph.Accessinfo.UserName, tenantName);
                
                using (PXLoginScope ls = new PXLoginScope(loginStr))
                {
                    InventoryItem item = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.ReadOnly.SelectSingleBound(graph, null, prodInfo.InventoryID);

                    if (item == null) { throw new PXException(NonExistedItem, tenantName); }

                    MOProdInfoTable newProdInfo = graph.ProdInfo.Cache.CreateCopy(prodInfo) as MOProdInfoTable;
                    
                    newProdInfo.NoteID = null;

                    graph.ProdInfo.Update(newProdInfo);

                    PXNoteAttribute.CopyNoteAndFiles(graph.ProdInfo.Cache, prodInfo, graph.ProdInfo.Cache, newProdInfo, true, true);

                    graph.Save.Press();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region enum
        public enum TenantType
        {
            Moshi, USA, Tyrian
        }
        #endregion
    }
}