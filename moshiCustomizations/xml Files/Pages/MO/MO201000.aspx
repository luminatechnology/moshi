<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO201000.aspx.cs" Inherits="Page_MO201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOShipmentTrkMaint" PrimaryView="Shipment" >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid FastFilterFields="ShipmentNbr,CustomerID,SiteID,UsrTrackingNbr" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="Shipment">
			    <Columns>
				<px:PXGridColumn DataField="Selected" Width="100" Type="CheckBox" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="UsrSentCustomer" Width="80" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="UsrSentCarrier" Width="70" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerID_Customer_acctName" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipmentQty" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SiteID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipVia" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipTermsID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ShipZoneID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrTrackingNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrCarrier" Width="140" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
				<px:PXSegmentMask runat="server" ID="CstPXSegmentMask2" DataField="CustomerID" AllowEdit="True" ></px:PXSegmentMask></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	
		<Mode AllowAddNew="False" ></Mode>
		<Mode AllowDelete="False" ></Mode></px:PXGrid>
</asp:Content>