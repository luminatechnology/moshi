<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO301000.aspx.cs" Inherits="Page_MO301000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOProductInfoEntry" PrimaryView="ProdInfo" >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="ProdInfo" Width="100%" Height="80px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXSegmentMask runat="server" Width="200px" ID="CstPXSegmentMask1" DataField="InventoryID" CommitChanges="True" AllowEdit="True" ></px:PXSegmentMask>
			<px:PXTextEdit Width="500px" runat="server" ID="CstPXTextEdit2" DataField="InventoryID_description" ></px:PXTextEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXTab DataMember="ProdInfoCur" ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" AllowAutoHide="false">
		<Items>
			<px:PXTabItem Text="Product Info (Numeric Type)">
				<Template>
					<px:PXLayoutRule runat="server" ID="CstPXLayoutRule4" StartColumn="True" ></px:PXLayoutRule>
					<px:PXLayoutRule LabelsWidth="L" runat="server" ID="CstPXLayoutRule3" StartGroup="True" GroupCaption="Additional Product Data Info" ></px:PXLayoutRule>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit8" DataField="P1_ProdLen_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit9" DataField="P1_ProdWdth_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit10" DataField="P1_ProdHgt_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit17" DataField="P1_ProdWgt_grams" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit18" DataField="P1_ProdLen_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit19" DataField="P1_ProdWdth_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit20" DataField="P1_ProdHgt_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit21" DataField="P1_ProdWgt_lbs" ></px:PXNumberEdit>
					<px:PXLayoutRule GroupCaption="Carton Info" LabelsWidth="L" runat="server" ID="CstPXLayoutRule6" StartGroup="True" ></px:PXLayoutRule>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit11" DataField="P3_CtnLen_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit12" DataField="P3_CtnWdth_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit13" DataField="P3_CtnHgt_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit22" DataField="P3_CtnWgt_kg" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit23" DataField="P3_CtnLen_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit24" DataField="P3_CtnWdth_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit25" DataField="P3_CtnHgt_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit26" DataField="P3_CtnWgt_lbs" ></px:PXNumberEdit>
					<px:PXLayoutRule runat="server" ID="CstPXLayoutRule5" StartColumn="True" ></px:PXLayoutRule>
					<px:PXLayoutRule GroupCaption="Packing Info" LabelsWidth="L" runat="server" ID="CstPXLayoutRule7" StartGroup="True" ></px:PXLayoutRule>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit14" DataField="P2_PkgLen_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit15" DataField="P2_PkgWdth_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit16" DataField="P2_PkgHgt_cm" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit27" DataField="P2_PkgWgt_Ipd_grams" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit28" DataField="P2_PkgLen_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit29" DataField="P2_PkgWdth_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit30" DataField="P2_PkgHgt_inch" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit31" DataField="P2_PkgWgt_Ipd_lbs" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit32" DataField="P2_Pkg_PlasticWgt_grams" ></px:PXNumberEdit>
					<px:PXNumberEdit runat="server" ID="CstPXNumberEdit33" DataField="P2_Pkg_PaperWgt_grams" ></px:PXNumberEdit>
					<px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartGroup="True" GroupCaption="Publish Info" ></px:PXLayoutRule>
					<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox2" DataField="Published" ></px:PXCheckBox></Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Product Info (Text Type)">
				<Template>
					<px:PXSelector runat="server" Width="80%" ID="CstPXSelector34" DataField="P6_CountryofOrigin" Height ="100px"/>
					<br/>
					<px:PXTextEdit Width="80%" TextMode="MultiLine" runat="server" ID="CstPXTextEdit35" DataField="P6_MasterPackQty" Height ="100px"></px:PXTextEdit>
					<br/>
					<px:PXTextEdit Width="80%" TextMode="MultiLine" runat="server" ID="CstPXTextEdit36" DataField="P6_PkgContent" Height ="100px"></px:PXTextEdit>
					<br/>
					<px:PXTextEdit Width="80%" TextMode="MultiLine" runat="server" ID="CstPXTextEdit37" DataField="P6_PkgVersion" Height ="100px"></px:PXTextEdit>
					<br/>
					<px:PXTextEdit Width="80%" TextMode="MultiLine" runat="server" ID="CstPXTextEdit38" DataField="P6_ProdMaterial" Height ="100px"></px:PXTextEdit></Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
	</px:PXTab>
</asp:Content>