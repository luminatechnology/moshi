<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="MO502000.aspx.cs" Inherits="Page_MO502000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="moshiCustomizations.Graph.MOUploadFile2SOInvProc" PrimaryView="InvoiceProc" >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="InvoiceProc">
			    <Columns>
				<px:PXGridColumn TextAlign="Center" DataField="Selected" Width="60" Type="CheckBox" AllowCheckAll="True" ></px:PXGridColumn>
				<px:PXGridColumn DataField="DocType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="RefNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="DocDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CuryID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CashAccountID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="PaymentMethodID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SOOrderType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SOOrderNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomerID" Width="140" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask1" DataField="CustomerID" ></px:PXSegmentMask>
					<px:PXSelector runat="server" ID="CstPXSelector2" DataField="SOOrderNbr" AllowEdit="True" /></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>