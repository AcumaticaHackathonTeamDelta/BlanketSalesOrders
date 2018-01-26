<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201020.aspx.cs" Inherits="Page_AU201020"
	Title="Automation Step Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUScreenActionMaint"
		PrimaryView="Buttons" >
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>		
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXSplitContainer runat="server" ID="splitActions" SplitterPosition="250" Style="border-top: solid 1px #BBBBBB;">
			<Template1>
			    <px:PXGrid ID="gridActions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
				Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Primary" SyncPosition="true" AllowPaging="false" Caption="Actions" CaptionVisible="true">
			        <AutoCallBack Enabled="true" Command="Refresh" Target="gridActionProps" />						        
					<Levels>
						<px:PXGridLevel DataMember="Buttons" >							        
							 <Columns>									        
								<px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />    
                                <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							 </Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" MinHeight="150" />
                    <ActionBar ActionsVisible="False">
                        <CustomItems>                         
								<px:PXToolBarButton CommandSourceID="ds" CommandName="Save" Text="" ImageSet="main" ImageKey="Save" />							    
                        </CustomItems>
                            <Actions>
                                <AddNew ToolBarVisible="Top" GroupIndex="0" Order="1" />
                                <Save ToolBarVisible="Top" GroupIndex="0" Order="2"/>                                
                                <Delete GroupIndex="0" Order="3"/>
                                <AdjustColumns  ToolBarVisible = "false" />
                                <ExportExcel  ToolBarVisible ="false" />
                                <Refresh ToolBarVisible ="False" />
                            </Actions>
                    </ActionBar>  
				</px:PXGrid>               
			</Template1>
            <Template2>
               <px:PXGrid ID="gridActionProps" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
				Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
				MatrixMode="true" AllowPaging="false" Caption="Action Properties" CaptionVisible="true">					        
					<Levels>
						<px:PXGridLevel DataMember="ButtonProps" >
							<Mode InitNewRow="True" />
							<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										Width="80px" />
									<px:PXGridColumn DataField="PropertyName" Width="200px" />									        
									<px:PXGridColumn DataField="OriginalValue" Width="200px" />
									<px:PXGridColumn DataField="OverrideValue" Width="200px" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" MinHeight="150" />
                    <ActionBar ActionsVisible="false"/>                              
				</px:PXGrid>
            </Template2>
			<AutoSize Enabled="true" Container="Window" />	                    					                          
		</px:PXSplitContainer>	     		
</asp:Content>
