<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO502015.aspx.cs" Inherits="Page_MO502015" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOSOOrderProcess"
        PrimaryView="Filter">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="40px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXDropDown Width="200px" runat="server" ID="CstPXDropDown1" DataField="Action" CommitChanges="True" ></px:PXDropDown></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="Records">
			    <Columns>
				<px:PXGridColumn DataField="Selected" Width="30" Type="CheckBox" AllowCheckAll="True" TextAlign="Center" ></px:PXGridColumn>
				<px:PXGridColumn DataField="OrderType" Width="70" />
				<px:PXGridColumn DataField="OrderNbr" Width="140" />
				<px:PXGridColumn DataField="OrderDate" Width="90" />
				<px:PXGridColumn DataField="Status" Width="70" />
				<px:PXGridColumn DataField="DestinationSiteID" Width="140" />
				<px:PXGridColumn DataField="CustomerID" Width="140" />
				<px:PXGridColumn DataField="TaxZoneID" Width="120" />
				<px:PXGridColumn DataField="OrderDesc" Width="280" />
				<px:PXGridColumn DataField="CustomerOrderNbr" Width="180" />
				<px:PXGridColumn DataField="OwnerID" Width="70" />
				<px:PXGridColumn DataField="TermsID" Width="120" />
				<px:PXGridColumn DataField="ShipVia" Width="140" /></Columns>
			
				<RowTemplate>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask2" DataField="CustomerID" ></px:PXSegmentMask>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask3" DataField="DestinationSiteID" ></px:PXSegmentMask>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector4" DataField="OrderNbr" ></px:PXSelector>
					<px:PXSelector runat="server" ID="CstPXSelector5" DataField="OrderType" AllowEdit="True" />
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector6" DataField="OwnerID" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector7" DataField="ShipVia" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector8" DataField="TaxZoneID" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector9" DataField="TermsID" ></px:PXSelector></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>