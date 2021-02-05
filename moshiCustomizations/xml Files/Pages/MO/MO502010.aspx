<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO502010.aspx.cs" Inherits="Page_MO502010" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOUploadFileByInventProc" PrimaryView="InventoryProc" >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="InventoryProc">
			    <Columns>
			        <px:PXGridColumn DataField="Selected" Width="60" Type="CheckBox" AllowCheckAll="True" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn DataField="InventoryCD" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Descr" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ItemType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="KitItem" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ItemClassID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="PostClassID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaxCalcMode" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ProductManagerID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="BasePrice" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="LastStdCost" Width="100" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask1" DataField="InventoryCD" ></px:PXSegmentMask>
					<px:PXSegmentMask runat="server" ID="CstPXSegmentMask2" DataField="ItemClassID" AllowEdit="True" ></px:PXSegmentMask></RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>