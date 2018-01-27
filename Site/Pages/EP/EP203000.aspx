<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP203000.aspx.cs"
    Inherits="Page_EP203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Employee" TypeName="PX.Objects.EP.EmployeeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewContact" Visible="False" />

            <px:PXDSCallbackCommand Name="GenerateTimeCards" Visible="False" CommitChanges="True"/>

            <px:PXDSCallbackCommand Name="ResetPassword" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ResetPasswordOK" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="ActivateLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="EnableLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="DisableLogin" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UnlockLogin" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="viewMap" Visible="False" CommitChanges="True" />            
            <px:PXDSCallbackCommand Name="CreateNewLicense" Visible="False" />
            <px:PXDSCallbackCommand Name="EmployeeSchedule" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="GridEmployeeLicenses" Name="EmployeeLicenses_ViewDetails" Visible="False" RepaintControls="All" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Employee" Caption="Employee Info"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edAcctCD"
        TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" DataSourceID="ds" FilterByAllFields="True" DisplayMode="Value"/>
            <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="518px" DataSourceID="ds" DataMember="CurrentEmployee" BorderStyle="None"
        AccessKey="T">
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
                    <px:PXFormView ID="ContactInfo" runat="server" Caption="Contact Info" DataMember="Contact" RenderStyle="Fieldset" DataSourceID="ds" TabIndex="200">
                        <Activity HighlightColor="" SelectedColor="" />
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" Enabled="False">
                                <LinkCommand Command="ViewContact" Target="ds" />
                            </px:PXTextEdit>
                            <px:PXDropDown ID="edTitle" runat="server" DataField="Title" />
                            <px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
                            <px:PXTextEdit ID="edMidName" runat="server" DataField="MidName" />
                            <px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
                            <px:PXLayoutRule runat="server" Merge="True"/>
                            <px:PXLabel ID="LPhone1" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone1" runat="server" DataField="Phone1"  LabelID="LPhone1"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone2" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone2Type" runat="server" DataField="Phone2Type" SelectedIndex="1"  SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone2" runat="server" DataField="Phone2" LabelID="LPhone2" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LPhone3" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edPhone3Type" runat="server" DataField="Phone3Type" SelectedIndex="5" SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edPhone3" runat="server" DataField="Phone3" LabelID="LPhone3"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLabel ID="LFax" runat="server" Size="SM" />
                            <px:PXDropDown Size="XS" ID="edFaxType" runat="server" DataField="FaxType" SelectedIndex="4"  SuppressLabel="True"/>
                            <px:PXMaskEdit Width="164px" ID="edFax" runat="server" DataField="Fax" LabelID="LFax"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True" />
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="AddressInfo" runat="server" Caption="Address info" DataMember="Address" DataSourceID="ds" RenderStyle="FieldSet" TabIndex="300">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowAddNew="True" DataSourceID="ds" CommitChanges="true" AutoRefresh="True" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AllowAddNew="True" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                    </px:PXFormView>

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Service Management" />
                    <px:PXCheckBox runat="server" ID="edSDEnabled" DataField="SDEnabled" CommitChanges="True" AlignLeft="True" />
                    <px:PXCheckBox runat="server" ID="edSendAppNotification" DataField="SendAppNotification" AlignLeft="True" />
                    <px:PXCheckBox runat="server" ID="edIsDriver" DataField="IsDriver" AlignLeft="True" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXButton runat="server" ID="edEmployeeScheduleButton" Width="120px" AlignLeft="True">
                        <AutoCallBack Target="ds" Command="EmployeeSchedule" />
                    </px:PXButton>

                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Employee Settings" />
                    <px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" />
                    <px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" />
                    <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" AllowEdit="True" />
                    <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" />
                    <px:PXDropDown ID="edHoursValidation" runat="server" AllowNull="False" DataField="HoursValidation" />
                    <px:PXSegmentMask ID="edSupervisorID" runat="server" DataField="SupervisorID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXSelector ID="edUserID" runat="server" DataField="UserID" AllowEdit="True" Enabled="False" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryID" runat="server" DataField="CuryID" AllowAddNew="True" />
                    <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" AllowAddNew="True" />
                    <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXSegmentMask ID="edLabourItemID" runat="server" DataField="LabourItemID" />
                    <px:PXCheckBox SuppressLabel="True" ID="edRouteEmails" runat="server" DataField="RouteEmails" />
                    <px:PXCheckBox SuppressLabel="True" ID="edTimeCardRequired" runat="server" DataField="TimeCardRequired" />
                    <px:PXFormView ID="PersonalInfo" runat="server" Caption="Personal info" DataMember="Contact" DataSourceID="ds" RenderStyle="FieldSet">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXDateTimeEdit ID="edDateOfBirth" runat="server" DataField="DateOfBirth" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employment History">
                <Template>
                    <px:PXGrid runat="server" ID="gridPositions" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeePositions">
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" Width="60px" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="PositionID" Width="180px" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="StartDate" Width="90px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="StartReason" Width="120px" />
                                    <px:PXGridColumn DataField="EndDate" Width="90px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsTerminated" Width="120px" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TermReason" Width="180px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsRehirable" Width="120px" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton CommandName="GenerateTimeCards" CommandSourceID="ds" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="GL Accounts" />
                    <px:PXFormView ID="frmPmtDefLocation" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVAPAccountID" runat="server" DataField="VAPAccountID" DataSourceID="ds" />
                            <px:PXSegmentMask ID="edVAPSubID" runat="server" DataField="VAPSubID" DataSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" />
                    <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" />
                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" />
                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Financial Settings" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" DataSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Payment Settings" />
                    <px:PXFormView ID="PXFormView3" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector CommitChanges="True" ID="edVPaymentMethodID" runat="server" DataField="VPaymentMethodID" AllowEdit="True"
                                DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVCashAccountID" runat="server" DataField="VCashAccountID" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Caption="Payment Instructions" Width="400px" Height="160px" MatrixMode="True" SkinID="Attributes">
                                <Levels>
                                    <px:PXGridLevel DataMember="PaymentDetails" DataKeyNames="BAccountID,LocationID,PaymentMethodID,DetailID">
                                        <Columns>
                                            <px:PXGridColumn DataField="PaymentMethodDetail__descr" Width="150px" />
                                            <px:PXGridColumn DataField="DetailValue" Width="200px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Layout HighlightMode="Cell" ColumnsMenu="False" HeaderVisible="False" />
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowSort="False" />
                                <AutoSize Enabled="False" />
                            </px:PXGrid>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
                                        TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
                                    <px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities" LoadOnDemand="True">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
                        AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
                        PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
                        BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
                                        ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" Width="60px" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" Width="80px" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="80px" />
                                    <px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
                                        SyncVisible="False" SyncVisibility="False" Width="108px">
                                        <NavigateParams>
                                            <px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
                                        </NavigateParams>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="WorkgroupID" Width="90px" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
                                MaxLength="50" Width="100%" Height="100%" SkinID="Label" >
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailings" LoadOnDemand="True">
                <Template>
                    <px:PXGrid runat="server" ID="gridNC" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" AdjustPageSize="Auto">
                        <Mode AllowAddNew="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="NWatchers" DataKeyNames="NotificationID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                    <px:PXDropDown ID="edFormat" runat="server" DataField="Format" SelectedIndex="3" />
                                    <px:PXTextEdit ID="edEntityDescription" runat="server" DataField="EntityDescription" Enabled="False" />
                                    <px:PXTextEdit ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="NotificationSetup__Module" />
                                    <px:PXGridColumn DataField="NotificationSetup__SourceCD" />
                                    <px:PXGridColumn DataField="NotificationSetup__NotificationCD" Width="120px" />
                                    <px:PXGridColumn DataField="ClassID" Width="100px" />
                                    <px:PXGridColumn DataField="EntityDescription" Width="200px" />
                                    <px:PXGridColumn DataField="ReportID" Width="100px" />
                                    <px:PXGridColumn DataField="TemplateID" Width="120px" />
                                    <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="80px" />
                                    <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Labor Item Overrides">
                <Template>
                    <px:PXGrid ID="LaborClassesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
                        DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="LaborMatrix">
                                <Columns>
                                    <px:PXGridColumn DataField="EarningType" CommitChanges="True" Width="110px" />
                                    <px:PXGridColumn DataField="EPEarningType__Description" Width="200px" />
                                    <px:PXGridColumn DataField="LabourItemID" CommitChanges="True" Width="150px" />
                                    <px:PXGridColumn DataField="InventoryItem__BasePrice" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employee Cost">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridEmployeeRates" runat="server" DataSourceID="ds" MatrixMode="True" SyncPosition="True" Height="400px" Width="100%"
                                SkinID="DetailsInTab">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeRates" DataKeyNames="RateID">
                                        <Columns>
                                            <px:PXGridColumn DataField="EffectiveDate" Width="100px" />
                                            <px:PXGridColumn DataField="RateType" AutoCallBack="True" Width="135px" />
                                            <px:PXGridColumn DataField="RegularHours" AutoCallBack="True" TextAlign="Right" Width="145px" />
                                            <px:PXGridColumn DataField="AnnualSalary" AutoCallBack="True" TextAlign="Right" Width="140px" />
                                            <px:PXGridColumn DataField="HourlyRate" AutoCallBack="True" TextAlign="Right" Width="80px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoCallBack Target="gridEmployeeRatesByProject" Command="Refresh" />
                                <Mode InitNewRow="True" />
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridEmployeeRatesByProject" runat="server" DataSourceID="ds" Height="400px" Width="100%"
                                SkinID="DetailsInTab" Caption="Overrides">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeRatesByProject" DataKeyNames="RateID,Line">
                                        <Columns>
                                            <px:PXGridColumn DataField="ProjectID" AutoCallBack="True" Width="100px" />
                                            <px:PXGridColumn DataField="TaskID" AutoCallBack="True" Width="100px" />
                                            <px:PXGridColumn DataField="HourlyRate" AutoCallBack="True" TextAlign="Right" Width="80px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Company Tree Info">
                <Template>
                    <px:PXGrid ID="companyTreeGrid" runat="server" DataSourceID="ds" Height="400px" 
                        Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="CompanyTree" DataKeyNames="WorkGroupID,UserID">                                
                                <Columns>
                                    <px:PXGridColumn DataField="WorkGroupID" Label="Workgroup ID" Width="100px" />
                                    <px:PXGridColumn DataField="IsOwner" Label="Owner" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Active" Label="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Assignment and Approval Maps">
                <Template>
                    <px:PXGrid ID="gridAssignmentandApprovalMaps" runat="server" DataSourceID="ds" Height="400px" 
                        Width="100%" SkinID="Inquire" MatrixMode="True" SyncPosition="true" 
                        FilesIndicator="false" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="AssigmentAndApprovalMaps">                                
                                <Columns>
                                    <px:PXGridColumn DataField="EPAssignmentMap__Name" Width="100px" LinkCommand="ViewMap"/>
                                    <px:PXGridColumn DataField="StepName" Width="100px" />
                                    <px:PXGridColumn DataField="Name" Width="100px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem> 

            <px:PXTabItem LoadOnDemand="True" Text="User Info">
                <Template>
                    <px:PXFormView ID="frmLogin" runat="server" DataMember="User" SkinID="Transparent" MarkRequired="Dynamic">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXDropDown ID="edState" runat="server" DataField="State" Enabled="False" />
                            <px:PXSelector ID="edLoginType" runat="server" DataField="LoginTypeID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                            <px:PXMaskEdit ID="edUsername" runat="server" DataField="Username" CommitChanges="True" />
                            <px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" />
                            <px:PXCheckBox ID="edGenerate" runat="server" DataField="GeneratePassword" CommitChanges="True" />
                            <px:PXButton ID="btnResetPassword" runat="server" Text="Reset Password" CommandName="ResetPassword" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="SM" StartColumn="True" SuppressLabel="True" />
                            <px:PXButton ID="btnActivateLogin" runat="server" CommandName="ActivateLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnEnableLogin" runat="server" CommandName="EnableLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnDisableLogin" runat="server" CommandName="DisableLogin" CommandSourceID="ds" Width="150" Height="20" />
                            <px:PXButton ID="btnUnlockLogin" runat="server" CommandName="UnlockLogin" CommandSourceID="ds" Width="150" Height="20" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" Caption=" ">
                        <ActionBar>
                            <Actions>
                                <Save Enabled="False" />
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Roles">
                                <Columns>
                                    <px:PXGridColumn AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Rolename_Roles_descr" Width="300px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="250" MinWidth="300" />
                    </px:PXGrid>
                    <px:PXSmartPanel ID="pnlResetPassword" runat="server" Caption="Change password"
                        LoadOnDemand="True" Width="400px" Key="User" CommandName="ResetPasswordOK" 
                        CommandSourceID="ds" AcceptButtonID="btnOk" CancelButtonID="btnCancel" 
                        AutoCallBack-Command="Refresh" AutoCallBack-Target="frmResetParams" 
                        AutoCallBack-Enabled="true" AutoReload="True">
                        <px:PXFormView ID="frmResetParams" runat="server" DataSourceID="ds" Width="100%" DataMember="User"
                            Caption="Reset Password" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
                        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK" />
                            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Delegates">
                <Template>
                    <px:PXGrid ID="companyTreeGrid" runat="server" DataSourceID="ds" Height="400px" Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Wingman" DataKeyNames="RecordID">
                                <Columns>
                                    <px:PXGridColumn runat="server" DataField="WingmanId" CommitChanges="True" Width="100px" />
                                    <px:PXGridColumn runat="server" DataField="WingmanId_EPEmployee_acctName" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Skills" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeSkills" SkinID="DetailsInTab" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeSkills">
                                <Columns>
                                    <px:PXGridColumn DataField="SkillID" Width="120px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FSSkill__Descr" Width="400px" />
                                    <px:PXGridColumn DataField="FSSkill__IsDriverSkill" Width="70px" Type="CheckBox" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edSkillID" DataField="SkillID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edFSSkill__Descr" DataField="FSSkill__Descr" />
                                    <px:PXCheckBox runat="server" ID="edFSSkill__IsDriverSkill" DataField="FSSkill__IsDriverSkill" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                    </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Service Areas" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeGeoZones" SkinID="DetailsInTab" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeGeoZones">
                                <Columns>
                                    <px:PXGridColumn DataField="GeoZoneID" Width="120px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="FSGeoZone__Descr" Width="400px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edGeoZoneID" DataField="GeoZoneID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edFSGeoZone__Descr" DataField="FSGeoZone__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Licenses" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridEmployeeLicenses" SkinID="DetailsInTab" DataSourceID="ds" Style='height:400px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="EmployeeLicenses" DataKeyNames="RefNbr,LicenseTypeID">
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" Width="70px" />
                                    <px:PXGridColumn DataField="LicenseTypeID" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Descr" Width="200px" />
                                    <px:PXGridColumn DataField="IssueDate" Width="90px" />
                                    <px:PXGridColumn DataField="ExpirationDate" Width="90px" />
                                    <px:PXGridColumn DataField="IssueByVendorID" Width="130px" />
                                    <px:PXGridColumn DataField="IssuingAgencyName" Width="200px" />
                                    <px:PXGridColumn DataField="CertificateRequired" Width="80px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="InitialAmount" Width="100px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InitialTerm" Width="70px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InitialTermType" Width="85px" />
                                    <px:PXGridColumn DataField="RenewalAmount" Width="100px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RenewalTerm" Width="70px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RenewalTermType" Width="85px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXTextEdit runat="server" ID="edDescr" DataField="Descr" />
                                    <px:PXDateTimeEdit runat="server" ID="edExpirationDate" DataField="ExpirationDate" />
                                    <px:PXDateTimeEdit runat="server" ID="edIssueDate" DataField="IssueDate" />
                                    <px:PXSelector runat="server" ID="edLicenseTypeID" DataField="LicenseTypeID" AllowEdit="True" CommitChanges="True" />
                                    <px:PXSelector runat="server" ID="edIssueByVendorID" DataField="IssueByVendorID" />
                                    <px:PXTextEdit runat="server" ID="edIssuingAgencyName" DataField="IssuingAgencyName" />
                                    <px:PXCheckBox runat="server" ID="edCertificateRequired" DataField="CertificateRequired" />
                                    <px:PXNumberEdit runat="server" ID="edInitialAmount" DataField="InitialAmount" />
                                    <px:PXTextEdit runat="server" ID="edInitialTerm" DataField="InitialTerm" />
                                    <px:PXDropDown runat="server" ID="edInitialTermType" DataField="InitialTermType" />
                                    <px:PXNumberEdit runat="server" ID="edRenewalAmount" DataField="RenewalAmount" />
                                    <px:PXTextEdit runat="server" ID="edRenewalTerm" DataField="RenewalTerm" />
                                    <px:PXDropDown runat="server" ID="edRenewalTermType" DataField="RenewalTermType" />
                                    <px:PXTextEdit runat="server" ID="edRefNbr" DataField="RefNbr" />
                                </RowTemplate>
                                <Mode InitNewRow="True" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="viewDetail">
                            <Actions>
                                <AddNew ToolBarVisible="Top" />
                                <Delete ToolBarVisible="Top" />
                            </Actions>
                            <CustomItems />
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="538" Container="Window" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelGenerateTimeCards" runat="server" Caption="Generate Time Cards"
        CaptionVisible="True" Key="GenTimeCardFilter" LoadOnDemand="true" AutoCallBack-Command="gentcform" AutoCallBack-Enabled="True" CallBackMode-CommitChanges="True" AutoReload="True">
        <px:PXFormView ID="gentcform" runat="server" DataSourceID="ds" DataMember="GenTimeCardFilter" SkinID="Transparent" DefaultControlID="edGenerateUntil">
            <Template>
                <px:PXLayoutRule runat="server" ID="rule1" StartColumn="true" LabelsWidth="XS" ControlSize="M" />
                <px:PXDateTimeEdit ID="edLastDateGenerated" runat="server" DataField="LastDateGenerated" TimeMode="false" DisplayFormat="d" />
                <px:PXDateTimeEdit ID="edGenerateUntil" runat="server" DataField="GenerateUntil" TimeMode="false" DisplayFormat="d" EditFormat="d" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Generate" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlChangeID" runat="server"  Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
            <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                DataMember="ChangeIDDialog">
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
                </Template>
            </px:PXFormView>
            <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
                <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" >
                    <AutoCallBack Target="formChangeID" Command="Save" />
                </px:PXButton>
				<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />						
            </px:PXPanel>
    </px:PXSmartPanel>    
</asp:Content>