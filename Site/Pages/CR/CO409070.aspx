﻿<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409070.aspx.cs" Inherits="Pages_CO409070"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Emails" TypeName="PX.Objects.SM.EmailEnq" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="Emails_ViewDetails" Visible="False" PopupCommandTarget="gridInbox" PopupCommand="Refresh"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridInbox" runat="server" DataSourceID="ds" ActionsPosition="Top" SyncPosition="True" OnRowDataBound="grid_RowDataBound"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="PrimaryInquire" NoteIndicator="false"
		Width="100%" MatrixMode="True" RestrictFields="False" FastFilterFields="Subject,MailFrom,EMailAccount__Description">
		<Levels>
			<px:PXGridLevel DataMember="Emails">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="50px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox"/>

                    <px:PXGridColumn DataField="CRActivity__ClassIcon" Width="50px" TextAlign="Center" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="CRActivity__Type" Width="50px" AllowShowHide="True" Visible="false" SyncVisible="false"/>

                    <px:PXGridColumn DataField="IsIncome" Width="50px" TextAlign="Center" Type="CheckBox" AllowShowHide="True" Visible="false" SyncVisible="false"/>

					<px:PXGridColumn DataField="Subject" Width="450px" LinkCommand="Emails_ViewDetails" />
					<px:PXGridColumn DataField="MailFrom" Width="250px" />

					<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" />
					<px:PXGridColumn DataField="MPStatus" Width="70px" />
                    <px:PXGridColumn DataField="MailTo" Width="250px" />
                    <px:PXGridColumn DataField="MailCc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="MailBcc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
					<px:PXGridColumn DataField="CRActivity__Source" Width="150px" LinkCommand="viewEntity" AllowSort="false" AllowFilter="false"/>
					<px:PXGridColumn DataField="CRActivity__RefNoteID" Width="150px" Visible="false" SyncVisible="false" AllowSort="false" AllowFilter="false"/>
					<px:PXGridColumn DataField="CRActivity__WorkgroupID" Width="90px" DisplayMode="Text" />
					<px:PXGridColumn DataField="CRActivity__OwnerID" Width="90px" DisplayMode="Text" />
					<px:PXGridColumn DataField="MessageID" Width="90px" Visible="false"  SyncVisible="false"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="90px"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
		    <Actions>
		        <AddNew Enabled="False"/>
                <Delete GroupIndex="1"/>
		    </Actions>
			<CustomItems>
				<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
					<AutoCallBack Command="Emails_ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="False" AllowUpdate="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
