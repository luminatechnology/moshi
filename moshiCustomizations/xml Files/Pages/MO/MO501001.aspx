<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO501001.aspx.cs" Inherits="Page_MO501001" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOShipTrackingProc"
        PrimaryView="Filter"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXDropDown runat="server" ID="CstPXDropDown1" DataField="Action" CommitChanges="True" ></px:PXDropDown></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="Records">
			    <Columns>
				<px:PXGridColumn DataField="Selected" Width="30" Type="CheckBox" AllowCheckAll="True" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerLocationID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SiteID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="DestinationSiteID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentQty" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrTrackingNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="UsrPacking" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrCarrier" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerOrderNbr" Width="180" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSegmentMask runat="server" ID="CstPXSegmentMask2" DataField="CustomerID" AllowEdit="True" />
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector3" DataField="ShipmentNbr" ></px:PXSelector></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>