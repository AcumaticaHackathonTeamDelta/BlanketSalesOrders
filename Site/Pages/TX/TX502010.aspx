<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX502010.aspx.cs" Inherits="Page_TX502010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.ReportTaxDetail" PrimaryView="History_Header">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewBatch" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="History_Header" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="true" CommitChanges="true" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" />
			<px:PXSelector CommitChanges="True" ID="edTaxPeriodID" runat="server" DataField="TaxPeriodID" AutoRefresh="true" Size="s" />
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDateUI" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateInclusive" />
			<px:PXDropDown CommitChanges="True" ID="edLineNbr" runat="server" DataField="LineNbr" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" Caption="Details" SkinID="Inquire"
		FastFilterFields="RefNbr, TaxID, BAccount__AcctCD, BAccount__AcctName" SyncPosition="true">
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Batch" Tooltip="View Batch">
					<AutoCallBack Command="ViewBatch" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="History_Detail">
				<Columns>
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="Module" TextAlign="Left" Width="54px" />
					<px:PXGridColumn DataField="TranTypeInvoiceDiscriminated" TextAlign="Left" Width="90px" />
					<px:PXGridColumn DataField="RefNbr" TextAlign="Left" Width="90px" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="TranDate" TextAlign="Left" Width="90px" />
					<px:PXGridColumn DataField="TaxID" TextAlign="Left" Width="81px" />
					<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="ReportTaxableAmt" TextAlign="Right" Width="90px" />
					<px:PXGridColumn DataField="ReportTaxAmt" TextAlign="Right" Width="90px" />
					<px:PXGridColumn DataField="BAccount__AcctCD" TextAlign="Right" Width="90px" />
					<px:PXGridColumn DataField="BAccount__AcctName" TextAlign="Right" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
