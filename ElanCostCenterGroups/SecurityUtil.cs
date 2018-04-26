using System;
using Microsoft.SharePoint;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Elan
{


    public static class SecurityUtils
    {



        public static bool GroupImplicitlyContainsUser(SPSite site, string groupName, string userName)
        {
            SPGroup siteGroup = null;
            try
            {
                siteGroup = site.RootWeb.SiteGroups[groupName];
            }
            catch 
            {
            }
            return siteGroup != null && siteGroup.ContainsCurrentUser;
        }


        public static bool GroupExplicitlyContainsUser(SPSite site, string groupName, string userName)
        {
            SPGroup siteGroup = null;
            try
            {
                bool currentCatchAccessDeniedExceptionSetting = site.CatchAccessDeniedException;
                site.CatchAccessDeniedException = false;
                siteGroup = site.RootWeb.SiteGroups[groupName];

                if (siteGroup != null)
                {
                    foreach (SPUser spUser in siteGroup.Users)
                    {
                        if (spUser.LoginName.Equals(userName))
                        {
                            return true;
                        }
                    }
                }

                site.CatchAccessDeniedException = currentCatchAccessDeniedExceptionSetting;
            }
            catch 
            {
            }

            return false;
        }


        public static void RemoveUserFromSiteGroups(SPSite site, string keepInGroupName, string removeFromGroupNamePrefix, SPUser user)
        {
            foreach (SPGroup @group in site.RootWeb.SiteGroups)
            {
                if ( !@group.Name.Equals(keepInGroupName) && @group.Name.StartsWith(removeFromGroupNamePrefix))
                {
                    @group.RemoveUser(user);
                }
            }
        }  
        
        public static bool SiteGroupExists(SPSite site, string groupName)
        {
            foreach (SPGroup @group in site.RootWeb.SiteGroups)
            {
                if (@group.Name.Equals(groupName))
                {
                    return true;
                }
            }
            return false;
        }


        public static SPGroup CreateSiteGroup(SPSite site, string ownerGroupName, string groupName, string description)
        {


            Trace.WriteLine("Group: " + groupName + " Owner: " + ownerGroupName);

            if (!SiteGroupExists(site, groupName))
            {
                //SPGroup ownerGroup = site.RootWeb.SiteGroups[ownerGroupName];

                try
                {
                    site.RootWeb.SiteGroups.Add(groupName, site.Owner, null, description);
                }
                catch (Exception ex) {
                    Trace.WriteLine("Create Site Group: " + ex.Message + ex.InnerException + ex.Source);
                }
            }

            SPGroup group = site.RootWeb.SiteGroups[groupName];

            return group;
        }

        public static void RemoveSiteGroups(SPSite site, string removeGroupPrefix)
        {

            Trace.WriteLine("CostCenterGroup: Removing Site Groups");

            List<string> groups = new List<string>();


            try
            {
                foreach (SPGroup @group in site.RootWeb.SiteGroups)
                {
                    if (@group.Name.StartsWith(removeGroupPrefix))
                    {
                        groups.Add(@group.Name);
                    }
                }
            }
            catch (Exception ex) 
            {
                Trace.WriteLine("Remove Site Groups Checking: " + ex.Message + ex.Source + ex.InnerException);
            }

            try {
            foreach (string groupname in groups)
            {
                site.RootWeb.SiteGroups.Remove(groupname);
                site.RootWeb.Update();
            }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Remove Site Groups deleting: " + ex.Message + ex.Source + ex.InnerException);
            }

        }        

    }
}
