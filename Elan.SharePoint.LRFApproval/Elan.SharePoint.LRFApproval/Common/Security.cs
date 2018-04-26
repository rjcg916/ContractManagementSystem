using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class UserPermission
    {
        public UserPermission(SPPrincipal p, SPRoleDefinition r)
        {
            Principal = p;
            Role = r;
        }
        public SPPrincipal Principal { get; set; }
        public SPRoleDefinition Role { get; set; }
    }

    public class Security
    {

        public static Dictionary<string, UserPermission> GetSecurityList(
        SPWeb web,
        List<SPPrincipal> readers,
        List<SPPrincipal> contributors)
        {

            SPRoleDefinition RoleDefinitionContributor = web.RoleDefinitions.GetByType(SPRoleType.Contributor);
            SPRoleDefinition RoleDefinitionReadOnly = web.RoleDefinitions.GetByType(SPRoleType.Reader);

            Dictionary<string, UserPermission> userSecurityList = new Dictionary<string, UserPermission>();

            //add read rights first to allow later addition/override of contribute rights

            //add readers 
            if (readers != null)
                foreach (SPPrincipal p in readers)
                    userSecurityList[p.LoginName] = new UserPermission(p, RoleDefinitionReadOnly);

            //add contributing groups
            if (contributors != null)
                foreach (SPPrincipal p in contributors)
                    userSecurityList[p.LoginName] = new UserPermission(p, RoleDefinitionContributor);

            return userSecurityList;
        }

        public static void ClearItemSecurity(SPListItem item)
        {

            //Check for permission inheritance, and break if necessary  
            if (!item.HasUniqueRoleAssignments)
            {
                item.BreakRoleInheritance(false); //pass true to copy role assignments from parent, false to start from scratch  
            }

            //remove all existing role assignments
            for (int i = item.RoleAssignments.Count - 1; i >= 0; --i)
            {
                item.RoleAssignments.Remove(i);
            }
        }

        public static void ClearItemSecurity(SPListItem item, SPPrincipal member)
        {
            if (member != null)
                if (item.HasUniqueRoleAssignments)
                    try 
                    {
                        if (item.RoleAssignments.GetAssignmentByPrincipal(member).Member != null)
                            item.RoleAssignments.Remove(member);
                    }
                    catch { } //ignore case where user has no item permission
        }

        public static void AssignItemPermissions(SPListItem item, SPPrincipal p, SPRoleDefinition r)
        {
            if ((p != null) && (r != null))
            {
                SPRoleAssignment RoleAssignment = new SPRoleAssignment(p);
                RoleAssignment.RoleDefinitionBindings.Add(r);
                item.RoleAssignments.Add(RoleAssignment);
            }
        }

        public static void AssignItemPermissions(SPListItem item, Dictionary<string, UserPermission> perms)
        {
            if ((perms != null) && (perms.Values != null))
                foreach (UserPermission userperm in perms.Values)
                {
                    if ((userperm.Role != null) && (userperm.Principal != null))
                    {
                        AssignItemPermissions(item, userperm.Principal, userperm.Role);
                    }
                }

        }

        public static void SetItemReadOnly(SPListItem item, SPPrincipal member)
        {
            Security.ClearItemSecurity(item, member);
            SPRoleDefinition RoleDefinition = item.Web.RoleDefinitions.GetByType(SPRoleType.Reader);

            Security.AssignItemPermissions(item, member, RoleDefinition);

            //       item.Update();
        }

        public static void SetItemContribute(SPListItem item, SPPrincipal member)
        {
            if ( (item == null) || (member == null))
                return;
            
            Security.ClearItemSecurity(item, member);

            SPRoleDefinition RoleDefinition = item.Web.RoleDefinitions.GetByType(SPRoleType.Contributor);
            Security.AssignItemPermissions(item, member, RoleDefinition);

            //       item.Update();
        }

    
    }
}
