<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyCostCenterPermissionsUserControl.ascx.cs" Inherits="Elan.MyCostCenterPermissions.MyCostCenterPermissionsUserControl" %>



<!--<asp:Panel ID="pnlLRFs" runat="server">
<asp:Label ID="lbLRFs" runat="server" >Initiate LRF</asp:Label>
<asp:BulletedList ID="blLRFs" runat="server" ></asp:BulletedList>
</asp:Panel>-->

<asp:Panel ID="pnlAgreements" runat="server">
<asp:Label ID="lbAgreements" runat="server">Cost Center Super User Access</asp:Label>
<asp:BulletedList ID="blAgreements" runat="server" ></asp:BulletedList>
</asp:Panel> 


