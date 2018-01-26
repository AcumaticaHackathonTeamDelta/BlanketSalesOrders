<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS500400.aspx.cs" Inherits="Page_FS500400" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" 
        TypeName="PX.Objects.FS.StaffContractScheduleProcess">
		<CallbackCommands>
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="gridStaffSchedules" 
                Name="StaffSchedules_ViewDetails" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="RollBackRun" Visible="False">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="160px" DataMember="Filter" TabIndex="1300" DefaultControlID="edBAccountID">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="SM" 
                LabelsWidth="120px" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Filtering Option" StartGroup="True">
            </px:PXLayoutRule>
            <px:PXSegmentMask runat="server" DataField="BAccountID" DataSourceID="ds" 
                ID="edBAccountID" DisplayMode="Text" AutoRefresh="True">
                <AutoCallBack Command="Save" Target="form">
                </AutoCallBack>
            </px:PXSegmentMask>
            <px:PXSelector ID="edBranchID" runat="server" CommitChanges="True" 
                DataField="BranchID">
            </px:PXSelector>
            <px:PXSelector ID="edBranchLocationID" runat="server" CommitChanges="True" 
                DataField="BranchLocationID">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="XS" 
                StartColumn="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Generation options" 
                StartGroup="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit runat="server" DataField="ToDate" ID="edToDate" 
                LabelWidth="180px" CommitChanges="True">
            </px:PXDateTimeEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" 
        DataMember="StaffSchedules">
		<Items>
            <px:PXTabItem Text="Schedules">
                <Template>
                    <px:PXGrid ID="gridStaffSchedules" runat="server" AllowPaging="True" 
                        DataSourceID="ds" Style="z-index: 100" 
		                Width="100%" Height="150px" SkinID="Inquire" TabIndex="500" 
                        AutoAdjustColumns="True" SyncPosition="True" KeepPosition="True">
		                <Levels>
			                <px:PXGridLevel DataMember="StaffSchedules">
			                    <RowTemplate>
                                    <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" 
                                        Text="Selected">
                                    </px:PXCheckBox>
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" 
                                        AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edEmployeeID" runat="server" AllowEdit="True" 
                                        DataField="EmployeeID">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edBAccount__AcctName" runat="server" 
                                        DataField="BAccount__AcctName">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edScheduleType" runat="server" DataField="ScheduleType">
                                    </px:PXDropDown>
                                    <px:PXDateTimeEdit ID="edStartDate_Date" runat="server" 
                                        DataField="StartDate_Date">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edEndDate_Date" runat="server" DataField="EndDate_Date">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edStartTime_Time" runat="server" 
                                        DataField="StartTime_Time">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edEndTime_Time" runat="server" 
                                        DataField="EndTime_Time">
                                    </px:PXDateTimeEdit>
                                    <px:PXTextEdit ID="edRecurrenceDescription" runat="server" 
                                        DataField="RecurrenceDescription">
                                    </px:PXTextEdit>
                                    <px:PXDateTimeEdit ID="edLastGeneratedElementDate" runat="server" 
                                        DataField="LastGeneratedElementDate">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" 
                                        Width="80px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RefNbr" Width="150px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StaffScheduleDescription" Width="150px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EmployeeID" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BAccount__AcctName" Width="200px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduleType" Width="120px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StartDate_Date">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EndDate_Date">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="StartTime_Time">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EndTime_Time">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="RecurrenceDescription" Width="170px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastGeneratedElementDate" Width="110px">
                                    </px:PXGridColumn>
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
                        <ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
                            <PagerSettings Mode="NextPrevFirstLast" ></PagerSettings>
                            <CustomItems>
                                <px:PXToolBarButton Text="View Details" Key="cmdItemDetails" Visible="false">
                                    <Images Normal="main@DataEntry" ></Images>
                                    <AutoCallBack Command="StaffSchedules_ViewDetails" Target="ds" ></AutoCallBack> 
                                    <ActionBar GroupIndex="0" ></ActionBar>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
	                    <AutoSize Enabled="True" />
	                    <Mode AllowAddNew="False" AllowDelete="False"/>
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Run History">
                <Template>
                    <px:PXGrid ID="GridContractGenerationHistory" runat="server" DataSourceID="ds" SkinID="Inquire"
                        Width="100%" Height="100%" TabIndex="-15736">
                        <Levels>
                            <px:PXGridLevel DataMember="ContractHistoryRecords">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edGenerationID" runat="server" DataField="GenerationID">
                                    </px:PXNumberEdit>
                                    <px:PXDateTimeEdit ID="edLastGeneratedAppointmentDate2" runat="server" 
                                        DataField="LastGeneratedAppointmentDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edLastProcessedDate" runat="server" 
                                        DataField="LastProcessedDate">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" 
                                        DataField="LastModifiedDateTime">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="GenerationID" Width="120px"
                                        TextAlign="Left">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastProcessedDate" Width="160px" 
                                        TextAlign="Left">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastModifiedDateTime" Width="160px" 
                                        TextAlign="Left">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar ActionsText="False">
                        <CustomItems>
                            <px:PXToolBarSeperator></px:PXToolBarSeperator>
                            <px:PXToolBarButton Tooltip="Rollback Last Run" ImageSet="main" ImageKey="Task">
                                <AutoCallBack Command="RollBackRun" Target="ds">
                                </AutoCallBack>
                            </px:PXToolBarButton>
                        </CustomItems>
		                </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>

</asp:Content>
