using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.IN;
using moshiCustomizations.Graph;

namespace moshiCustomizations.DAC
{
    [Serializable]
    [PXCacheName("Production Info Table")]
    [PXPrimaryGraph(typeof(MOProductInfoEntry))]
    public class MOProdInfoTable : IBqlTable
    {
        #region InventoryID
        [Inventory(IsKey = true)]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion
    
        #region P1_ProdLen_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Product Length (cm)")]
        public virtual Decimal? P1_ProdLen_cm { get; set; }
        public abstract class p1_ProdLen_cm : PX.Data.BQL.BqlDecimal.Field<p1_ProdLen_cm> { }
        #endregion
    
        #region P1_ProdWdth_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Product Width (cm)")]
        public virtual Decimal? P1_ProdWdth_cm { get; set; }
        public abstract class p1_ProdWdth_cm : PX.Data.BQL.BqlDecimal.Field<p1_ProdWdth_cm> { }
        #endregion
    
        #region P1_ProdHgt_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Product Height (cm)")]
        public virtual Decimal? P1_ProdHgt_cm { get; set; }
        public abstract class p1_ProdHgt_cm : PX.Data.BQL.BqlDecimal.Field<p1_ProdHgt_cm> { }
        #endregion
    
        #region P1_ProdWgt_grams
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Product Weight (grams)")]
        public virtual Decimal? P1_ProdWgt_grams { get; set; }
        public abstract class p1_ProdWgt_grams : PX.Data.BQL.BqlDecimal.Field<p1_ProdWgt_grams> { }
        #endregion
    
        #region P1_ProdLen_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Product Length (inches)")]
        public virtual Decimal? P1_ProdLen_inch { get; set; }
        public abstract class p1_ProdLen_inch : PX.Data.BQL.BqlDecimal.Field<p1_ProdLen_inch> { }
        #endregion
    
        #region P1_ProdWdth_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Product Width (inches)")]
        public virtual Decimal? P1_ProdWdth_inch { get; set; }
        public abstract class p1_ProdWdth_inch : PX.Data.BQL.BqlDecimal.Field<p1_ProdWdth_inch> { }
        #endregion
    
        #region P1_ProdHgt_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Product Height (inches)")]
        public virtual Decimal? P1_ProdHgt_inch { get; set; }
        public abstract class p1_ProdHgt_inch : PX.Data.BQL.BqlDecimal.Field<p1_ProdHgt_inch> { }
        #endregion
    
        #region P1_ProdWgt_lbs
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Product Weight (lbs)")]
        public virtual Decimal? P1_ProdWgt_lbs { get; set; }
        public abstract class p1_ProdWgt_lbs : PX.Data.BQL.BqlDecimal.Field<p1_ProdWgt_lbs> { }
        #endregion
    
        #region P2_PkgLen_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Package Length (cm)")]
        public virtual Decimal? P2_PkgLen_cm { get; set; }
        public abstract class p2_PkgLen_cm : PX.Data.BQL.BqlDecimal.Field<p2_PkgLen_cm> { }
        #endregion
    
        #region P2_PkgWdth_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Package Width (cm)")]
        public virtual Decimal? P2_PkgWdth_cm { get; set; }
        public abstract class p2_PkgWdth_cm : PX.Data.BQL.BqlDecimal.Field<p2_PkgWdth_cm> { }
        #endregion
    
        #region P2_PkgHgt_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Package Height (cm)")]
        public virtual Decimal? P2_PkgHgt_cm { get; set; }
        public abstract class p2_PkgHgt_cm : PX.Data.BQL.BqlDecimal.Field<p2_PkgHgt_cm> { }
        #endregion
    
        #region P2_PkgWgt_Ipd_grams
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Package Weight (include product, grams)")]
        public virtual Decimal? P2_PkgWgt_Ipd_grams { get; set; }
        public abstract class p2_PkgWgt_Ipd_grams : PX.Data.BQL.BqlDecimal.Field<p2_PkgWgt_Ipd_grams> { }
        #endregion
    
        #region P2_PkgLen_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Package Length (inches)")]
        public virtual Decimal? P2_PkgLen_inch { get; set; }
        public abstract class p2_PkgLen_inch : PX.Data.BQL.BqlDecimal.Field<p2_PkgLen_inch> { }
        #endregion
    
        #region P2_PkgWdth_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Package Width (inches)")]
        public virtual Decimal? P2_PkgWdth_inch { get; set; }
        public abstract class p2_PkgWdth_inch : PX.Data.BQL.BqlDecimal.Field<p2_PkgWdth_inch> { }
        #endregion
    
        #region P2_PkgHgt_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Package Height (inches)")]
        public virtual Decimal? P2_PkgHgt_inch { get; set; }
        public abstract class p2_PkgHgt_inch : PX.Data.BQL.BqlDecimal.Field<p2_PkgHgt_inch> { }
        #endregion
    
        #region P2_PkgWgt_Ipd_lbs
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Package Weight (include product, lbs)")]
        public virtual Decimal? P2_PkgWgt_Ipd_lbs { get; set; }
        public abstract class p2_PkgWgt_Ipd_lbs : PX.Data.BQL.BqlDecimal.Field<p2_PkgWgt_Ipd_lbs> { }
        #endregion
    
        #region P2_Pkg_PlasticWgt_grams
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Packaging (Plastic Weight, grams)")]
        public virtual Decimal? P2_Pkg_PlasticWgt_grams { get; set; }
        public abstract class p2_Pkg_PlasticWgt_grams : PX.Data.BQL.BqlDecimal.Field<p2_Pkg_PlasticWgt_grams> { }
        #endregion
    
        #region P2_Pkg_PaperWgt_grams
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Packaging (Paper Weight, grams)")]
        public virtual Decimal? P2_Pkg_PaperWgt_grams { get; set; }
        public abstract class p2_Pkg_PaperWgt_grams : PX.Data.BQL.BqlDecimal.Field<p2_Pkg_PaperWgt_grams> { }
        #endregion
    
        #region P3_CtnLen_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Carton Length (cm)")]
        public virtual Decimal? P3_CtnLen_cm { get; set; }
        public abstract class p3_CtnLen_cm : PX.Data.BQL.BqlDecimal.Field<p3_CtnLen_cm> { }
        #endregion
    
        #region P3_CtnWdth_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Carton Width (cm)")]
        public virtual Decimal? P3_CtnWdth_cm { get; set; }
        public abstract class p3_CtnWdth_cm : PX.Data.BQL.BqlDecimal.Field<p3_CtnWdth_cm> { }
        #endregion
    
        #region P3_CtnHgt_cm
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Carton Height (cm)")]
        public virtual Decimal? P3_CtnHgt_cm { get; set; }
        public abstract class p3_CtnHgt_cm : PX.Data.BQL.BqlDecimal.Field<p3_CtnHgt_cm> { }
        #endregion
    
        #region P3_CtnWgt_kg
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Carton Weight (kg)")]
        public virtual Decimal? P3_CtnWgt_kg { get; set; }
        public abstract class p3_CtnWgt_kg : PX.Data.BQL.BqlDecimal.Field<p3_CtnWgt_kg> { }
        #endregion
    
        #region P3_CtnLen_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Carton Length (inches)")]
        public virtual Decimal? P3_CtnLen_inch { get; set; }
        public abstract class p3_CtnLen_inch : PX.Data.BQL.BqlDecimal.Field<p3_CtnLen_inch> { }
        #endregion
    
        #region P3_CtnWdth_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Carton Width (inches)")]
        public virtual Decimal? P3_CtnWdth_inch { get; set; }
        public abstract class p3_CtnWdth_inch : PX.Data.BQL.BqlDecimal.Field<p3_CtnWdth_inch> { }
        #endregion
    
        #region P3_CtnHgt_inch
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Carton Height (inches)")]
        public virtual Decimal? P3_CtnHgt_inch { get; set; }
        public abstract class p3_CtnHgt_inch : PX.Data.BQL.BqlDecimal.Field<p3_CtnHgt_inch> { }
        #endregion
    
        #region P3_CtnWgt_lbs
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Carton Weight (lbs)")]
        public virtual Decimal? P3_CtnWgt_lbs { get; set; }
        public abstract class p3_CtnWgt_lbs : PX.Data.BQL.BqlDecimal.Field<p3_CtnWgt_lbs> { }
        #endregion
    
        #region P6_MasterPackQty
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Master Pack Qty.")]
        public virtual string P6_MasterPackQty { get; set; }
        public abstract class p6_MasterPackQty : PX.Data.BQL.BqlString.Field<p6_MasterPackQty> { }
        #endregion
    
        #region P6_PkgContent
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Package Content")]
        public virtual string P6_PkgContent { get; set; }
        public abstract class p6_PkgContent : PX.Data.BQL.BqlString.Field<p6_PkgContent> { }
        #endregion
    
        #region P6_PkgVersion
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Package Version")]
        public virtual string P6_PkgVersion { get; set; }
        public abstract class p6_PkgVersion : PX.Data.BQL.BqlString.Field<p6_PkgVersion> { }
        #endregion
    
        #region P6_CountryofOrigin
        [PXDBString(2, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Country of Origin", Enabled = false)]
        [Country]
        [PXFormula(typeof(Default<MOProdInfoTable.inventoryID>))]
        [PXDefault(typeof(Search<InventoryItem.countryOfOrigin, Where<InventoryItem.inventoryID, Equal<Current<MOProdInfoTable.inventoryID>>>>))]
        public virtual string P6_CountryofOrigin { get; set; }
        public abstract class p6_CountryofOrigin : PX.Data.BQL.BqlString.Field<p6_CountryofOrigin> { }
        #endregion
    
        #region P6_ProdMaterial
        [PXDBString(1024, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Product Material")]
        public virtual string P6_ProdMaterial { get; set; }
        public abstract class p6_ProdMaterial : PX.Data.BQL.BqlString.Field<p6_ProdMaterial> { }
        #endregion

        #region Published
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Published")]
        public virtual bool? Published { get; set; }
        public abstract class published : PX.Data.BQL.BqlBool.Field<published> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion
    
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion
    
        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
    
        #region LastModifiedByID
        [PXDBLastModifiedByID(DisplayName = "Last Revision By")]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion
    
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion
    
        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        [PXUIField(DisplayName = "Last Revision Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
    
        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    
        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}