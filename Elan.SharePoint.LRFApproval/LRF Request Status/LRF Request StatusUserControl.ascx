<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LRF Request StatusUserControl.ascx.cs"
    Inherits="Elan.SharePoint.LRFApproval.LRF_Request_Status.LRF_Request_StatusUserControl" %>


    <asp:Label runat="server" ID="lblMessage" ForeColor="Red" Visible="false"></asp:Label>


    <asp:Panel runat="server" ID="pnlStatus" Visible="true">
		<div class="hc-main">
		    <div class="hc-dd-top"></div>
		    <div class="hc-dd-mid">
		        <div class="hc-dd-inner">
		        
		        	<div class="LifeLrfCreation">
                        <asp:Image runat="server" ID="imgLRFCreation" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeCreateOff.png" />

		        	</div>
		        	<div class="LifeLrfSubmitted">
                        <asp:Image runat="server" ID="imgLRFSubmitted" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeSubmitOff.png" />
					</div>
		        	<div class="LifeLRFDeptApproval">
                        <asp:Image runat="server" ID="imgLRFDeptApproval" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeDptApprovalOff.png" />
					</div>
		        	<div class="LifeDeptApproved">
                        <asp:Image runat="server" ID="imgDeptApproved" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeDptApprovedOff.png" />
					</div>
		        	<div class="LifeLrfFinanceApproval">
                        <asp:Image runat="server" ID="imgLrfFinanceApproval" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeFinanceApprovalOff.png" />
					</div>
		        	<div class="LifeFinanceApproved">
                        <asp:Image runat="server" ID="imgFinanceApproved" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeFinanceApprovedOff.png" />
					</div>
		        	<div class="LifeAttorneyAssigned">
                        <asp:Image runat="server" ID="imgAttorneyAssigned" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeAssignedOff.png" />
					</div>
		        	<div class="LifeFullyExecuted">
                        <asp:Image runat="server" ID="imgFullyExecuted" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeExecutedOff.png" />
					</div>
		        </div>
		    </div>
		    <div class="hc-dd-bottom"></div>
		</div>
    </asp:Panel>


    <asp:Panel runat="server" ID="pnlCancelled" Visible="false">
		<div class="hc-main">
		    <div class="hc-dd-top"></div>
		    <div class="hc-dd-mid">
		        <div class="hc-dd-inner">
		        	<div class="LifeLrfCancelled">
                        <asp:Image runat="server" ID="imgLrfCancelled" CssClass="LifeIcon" ImageUrl="~/_layouts/images/Elan.SharePoint.LRFApproval/LifeLrfCancelledOn.png" />
					</div>
		        	<div class="LifeLrfRejected">
                       <!-- <asp:Label runat="server" ID="lblCancelledRejected" Text="LRF Cancelled/Rejected"></asp:Label>-->
		            </div>
		        </div>
		    </div>
		    <div class="hc-dd-bottom"></div>
		</div>
    </asp:Panel>

