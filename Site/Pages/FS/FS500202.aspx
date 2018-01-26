<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS500202.aspx.cs" Inherits="Page_FS500202" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Batches" 
                TypeName="PX.Objects.FS.ReviewInvoiceBatches">
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phL" Runat="Server">
            <px:PXGrid ID="gridBatches" runat="server" Width="100%" Style="z-index: 100" AllowPaging="True" 
                AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" 
                AutoSize="True" TabIndex="900">
                <Levels>
                    <px:PXGridLevel DataMember="Batches" DataKeyNames="BatchID">
                        <RowTemplate>
                            <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected">
                            </px:PXCheckBox>
                            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr">
                            </px:PXSelector>
                            <px:PXDropDown ID="edPostTo" runat="server" DataField="PostTo">
                            </px:PXDropDown>
                            <px:PXNumberEdit ID="edQtyDoc" runat="server" DataField="QtyDoc" DefaultLocale="">
                            </px:PXNumberEdit>
                            <px:PXDateTimeEdit ID="edUpToDate" runat="server" DataField="UpToDate" DefaultLocale="">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edBillingCycleID" runat="server" DataField="BillingCycleID">
                            </px:PXSelector>
                            <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DefaultLocale="">
                            </px:PXMaskEdit>
                            <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate" DefaultLocale="">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DefaultLocale="">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime" DefaultLocale="">
                            </px:PXDateTimeEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="70px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BatchNbr" Width="80px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostTo" Width="160px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="QtyDoc" TextAlign="Right" Width="110px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="UpToDate" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillingCycleID" Width="120px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FinPeriodID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="InvoiceDate" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CreatedDateTime" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="LastModifiedDateTime" Width="90px">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowAddNew="False" AllowDelete="False" />
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
            </px:PXGrid>
        </asp:Content>
