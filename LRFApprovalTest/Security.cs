using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Common;

namespace LRFApprovalTest
{
    public class Security
    {

        public static void GetSecurityList_Entries_Valid(SPWeb web)
        {
            List<SPPrincipal> readers = new List<SPPrincipal>();

            List<SPPrincipal> contributors = new List<SPPrincipal>();

            Dictionary<string, UserPermission> perms = Elan.SharePoint.LRFApproval.Common.Security.GetSecurityList(web, readers, contributors);

        }

        public static void ClearItemSecurity_Inherited_NoPerm(SPWeb web)
        {

            SPListItem item = web.Lists["GenericList"].AddItem();
            item["Title"] = "ClearItemSecurity_Inherited_NoPerm";
            item.Update();
            Elan.SharePoint.LRFApproval.Common.Security.ClearItemSecurity(item);
        }

        public static void ClearItemSecurity_ItemSecurity_NoPerm(SPWeb web)
        {

            SPListItem item = web.Lists["GenericList"].Items[0];
            Elan.SharePoint.LRFApproval.Common.Security.ClearItemSecurity(item);
        }

        public static void ClearItemSecurity_Member_Removed(SPWeb web)
        {
            SPListItem item = web.Lists["GenericList"].Items[0];
            SPPrincipal member = web.Users.GetByEmail("webgrouptest3@elan.com");

            Elan.SharePoint.LRFApproval.Common.Security.ClearItemSecurity(item, member);
        }

        public static void AssignItemPermissions_Perms_Assigned(SPWeb web)
        {

            SPListItem item = null;
            Dictionary<string, UserPermission> perms = null;

            Elan.SharePoint.LRFApproval.Common.Security.AssignItemPermissions(item, perms);
        }

        public static void AssignItemPermissions_PrinRole_Assigned(SPWeb web)
        {

            SPListItem item = null;
            SPPrincipal p = null;
            SPRoleDefinition r = null; 

            Elan.SharePoint.LRFApproval.Common.Security.AssignItemPermissions(item, p, r);
        }

        public static void SetItemReadOnly_Contributor_Read(SPWeb web)
        {
            SPGroup member = web.SiteGroups["Legal Group 1"];
            SPListItem item = Elan.SharePoint.LRFApproval.Common.LRF.GetItemById(web, 960);
            Elan.SharePoint.LRFApproval.Common.Security.SetItemReadOnly(item, member);
        }

        public static void SetItemContribute_Exists_Contribute(SPWeb web)
        {
            SPGroup member = web.SiteGroups["Legal Group 1"];
            SPListItem item = Elan.SharePoint.LRFApproval.Common.LRF.GetItemById(web, 960);
            Elan.SharePoint.LRFApproval.Common.Security.SetItemContribute(item, member);
        }

        public static void SetItemContribute_GroupDoesNotExists_NoAction(SPWeb web)
        {

            SPGroup member = null;
            SPListItem item = Elan.SharePoint.LRFApproval.Common.LRF.GetItemById(web, 960);
            Elan.SharePoint.LRFApproval.Common.Security.SetItemContribute(item, member);
        }

        public static void SetItemContribute_ItemDoesNotExists_NoAction(SPWeb web)
        {

            SPGroup member = web.SiteGroups["Legal Group 1"];
            SPListItem item = null;
            Elan.SharePoint.LRFApproval.Common.Security.SetItemContribute(item, member);
        }

    }
}
