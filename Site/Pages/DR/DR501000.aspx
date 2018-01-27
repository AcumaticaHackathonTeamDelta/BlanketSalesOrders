<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR501000.aspx.cs"
    Inherits="Page_DR501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.DRRecognition" PrimaryView="Filter"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="RecDate" ID="edRecDate" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="DeferredCode" ID="edDeferredCode" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Deferred Transactions"
        AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" TabIndex="300">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" ValidateRequestMode="Inherit" />
                    <px:PXSegmentMask ID="edScheduleNumber" runat="server" DataField="ScheduleNbr" Enabled="False" ValidateRequestMode="Inherit" />
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" ValidateRequestMode="Inherit" />
                    <px:PXDateTimeEdit ID="edRecDate" runat="server" DataField="RecDate" Enabled="False" ValidateRequestMode="Inherit" />
                    <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" Enabled="False" ValidateRequestMode="Inherit" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False" />
                    <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" ValidateRequestMode="Inherit" />
                    <px:PXTextEdit ID="edDefCode" runat="server" AllowNull="False" DataField="DefCode" Enabled="False" ValidateRequestMode="Inherit" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="true" AllowNull="False" DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ScheduleNbr" Width="72px" TextAlign="Right" LinkCommand="ViewSchedule" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DefCode" Width="110px" AllowNull="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocType" Width="110px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ComponentCD" TextAlign="Left" Width="100px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LineNbr" Width="54px" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RecDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Amount" Width="81px" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" Width="63px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="BranchID" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="ViewSchedule"/>
            
    </px:PXGrid>
</asp:Content>