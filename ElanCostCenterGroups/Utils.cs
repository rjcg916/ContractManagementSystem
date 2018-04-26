using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Administration;
using CMSCommon;
using Elan.SharePoint.LRFApproval.Common;

namespace ElanCostCenterGroups
{

    public class Utils
    {

        public static SPListItem UserExists(SPList list, string username)
        {
            SPListItem item = null;

            // check for item 

            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name='Title'/><Value Type='Text'>" + username + "</Value></Eq></Where>";

            SPListItemCollection collListItems = list.GetItems(oQuery);

            if (collListItems != null)
                if (collListItems.Count > 0)
                    item = list.GetItemById(collListItems[0].ID);

            return item;

        }
      
        public static SPUser GetValidUser(SPSite site, UserProfile up, ref string username, ref string costcenter)
        {

            username = Constants.INVALID;
            costcenter = Constants.INVALID;
            SPUser user = null;

            if (up == null)
            {
                Trace.WriteLine("GetValidUser: called with missing user profile");
                return user;
            }

            string loginname = Constants.INVALID; 
            
            try
            {

                // Profile group membership will be maintained by this job,  
                // so fetch profile and only include profiles with managers
                string manager = up[PropertyConstants.Manager].Value.ToString();
                username = up[PropertyConstants.UserName].Value.ToString();
                loginname = up[PropertyConstants.AccountName].Value.ToString();
                user = site.RootWeb.EnsureUser(loginname);

            }
            catch { }


            if ((username == Constants.INVALID) || (loginname == Constants.INVALID) || (user == null))
            {
                user = null;
                username = Constants.INVALID;
                return user;
            }


            string costCenterSource = Constants.INVALID;

            try
            {
                // cost center should be in department field, quit if not found                    
                if (up[PropertyConstants.Department].Value == null)
                {
                    user = null;
                    username = Constants.INVALID;
                    return user;
                }
                costCenterSource = up[PropertyConstants.Department].Value.ToString();
            }
            catch { }

            if (costCenterSource == Constants.INVALID)
            {
                user = null;
                username = Constants.INVALID;
                return user;
            }

            if (String.IsNullOrEmpty(costCenterSource))
            {
                user = null;
                username = Constants.INVALID;
                costcenter = Constants.INVALID;
                return user;
            }


            costCenterSource = costCenterSource.Trim();


            if (costCenterSource.Length < CMSCommon.Constants.COSTCENTERLENGTH)
            {
                user = null;
                username = Constants.INVALID;
                costcenter = Constants.INVALID;
                return user;
            }

            //cost center found in leading characters of field
            costcenter = Constants.INVALID;
            costcenter = costCenterSource.Substring(0, CMSCommon.Constants.COSTCENTERLENGTH);

            if (costcenter == Constants.INVALID)
            {
                user = null;
                username = Constants.INVALID;
                return user;
            }


            // check that cost center is valid (numeric range)
            int number = 0;
            try
            {
                number = Convert.ToInt32(costcenter);
            }
            catch
            {}

            if ((number < CMSCommon.Constants.MINCOSTCENTER) || (number > CMSCommon.Constants.MAXCOSTCENTER))
            {
                user = null;
                username = Constants.INVALID;
                costcenter = Constants.INVALID;
                return user;
            }


            return user;
        }

        public static void UpdateUserItem(SPList listUserProfile, UserProfile up, string username, string costcenter)
        {

            // check for user in CMS list
            SPItem item = UserExists(listUserProfile, username);

            // if not in list, add new item
            if (item == null)
            {
                item = listUserProfile.AddItem();
                item["Title"] = username;
            }

            // update new or current user entry

            try { item[PropertyConstants.UserName] = username; }
            catch { }

            try
            {
                item[PropertyConstants.AccountName] = up[PropertyConstants.AccountName].Value.ToString();
            }
            catch { }


            try
            {
                item[PropertyConstants.Location] = up[PropertyConstants.Location].Value.ToString();
            }
            catch { }


            try
            {
                item[PropertyConstants.WorkPhone] = up[PropertyConstants.WorkPhone].Value.ToString();
            }
            catch { }

            try
            {
                item[PropertyConstants.WorkEmail] = up[PropertyConstants.WorkEmail].Value.ToString();
            }
            catch { }

            try
            {
                item[PropertyConstants.Office] = up[PropertyConstants.Office].Value.ToString();
            }
            catch { }

            try
            {
                item[PropertyConstants.Department] = up[PropertyConstants.Department].Value.ToString();
            }
            catch { }

            try
            {
                item[CMSCommon.Constants.SAPUSERNAMEFIELD] = up[CMSCommon.Constants.SAPUSERNAMEFIELD].Value.ToString();
            }
            catch { }

            try
            {
                item[CMSCommon.Constants.CMSPROFILECOSTCENTER] = costcenter;
            }
            catch { }

            try
            {
                item.Update();
            }
            catch (Exception ex) {
                Trace.WriteLine("UpdateUserItem: Error for user: " + username + " " + ex.ToString());
            }

        }


        public static void EnsureCostCenterGroupsForUser(SPSite site, SPUser user, string costcenter, string username)
        {

            // make sure all groups exists for current user's cost center
            try
            {
                SPGroup userGroup = null;
                string costcenterUserGroup = CMSCommon.Constants.COSTCENTERUSERPREFIX + costcenter;
                if (!SecurityUtils.SiteGroupExists(site, costcenterUserGroup))
                {
                    userGroup = SecurityUtils.CreateSiteGroup(site, CMSCommon.Constants.CMSFINANCEGROUP, costcenterUserGroup, CMSCommon.Constants.COSTCENTERUSERDESC);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EnsureCostCenterGroupsForUser: Issue with CostCenterUserGroup: " + costcenter + " " + ex.ToString());
            }

            try
            {
                SPGroup superUserGroup = null;
                string costcenterSuperUserGroup = CMSCommon.Constants.COSTCENTERSUPERUSERPREFIX + costcenter;
                if (!SecurityUtils.SiteGroupExists(site, costcenterSuperUserGroup))
                {
                    superUserGroup = SecurityUtils.CreateSiteGroup(site, CMSCommon.Constants.CMSFINANCEGROUP, costcenterSuperUserGroup, CMSCommon.Constants.COSTCENTERSUPERUSERDESC);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EnsureCostCenterGroupsForUser: Issue with CostCenterSuperUserGroup: " + costcenter + " " + ex.ToString());
            }


            try
            {
                string costcenterprofileGroup = CMSCommon.Constants.COSTCENTERPROFILEPREFIX + costcenter;
                SPGroup profileGroup = null;
                if (!SecurityUtils.SiteGroupExists(site, costcenterprofileGroup))
                {
                    profileGroup = SecurityUtils.CreateSiteGroup(site, CMSCommon.Constants.CMSOWNERGROUP, costcenterprofileGroup, CMSCommon.Constants.COSTCENTERPROFILEDESC);
                }
                else { 
                    profileGroup = site.RootWeb.SiteGroups[costcenterprofileGroup];
                }

                // if not already a member, add user to default profile group
                if (!SecurityUtils.GroupExplicitlyContainsUser(site, costcenterprofileGroup, username))
                {
                    profileGroup.AddUser(user);
                }

                // remove user from all but default profile group

                SecurityUtils.RemoveUserFromSiteGroups(site, costcenterprofileGroup, CMSCommon.Constants.COSTCENTERPROFILEPREFIX, user);
            }

            catch (Exception ex)
            {
                Trace.WriteLine("EnsureCostCenterGroupsForUser: Issue with CostCenterProfileGroup: " + costcenter + " " + ex.ToString());
            }

        }


        public delegate void UpdateProgress(int progress); 
       
        public static void UpdateCostCenterGroups(SPSite site, UpdateProgress progressUpdate)
        {

            string costcenter;
            string username;

            SPServiceContext context = null;
            try
            {
                context = SPServiceContext.GetContext(site);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("UpdateCostCenterGroups: Error creating service context " + ex.ToString());
                throw ex;
            }


            UserProfileManager profileManager = null;
            try
            {
                profileManager = new UserProfileManager(context);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("UpdateCostCenterGroups: Error creating UserProfileManager " + ex.ToString());
                throw ex;
            }


            //user profile list
            SPList listUserProfile = site.RootWeb.Lists[CMSCommon.Constants.CMSUSERPROFILELIST];

            //counters to keep track of progress
            long profCount =  profileManager.Count;
            int curProfile = 0;

            foreach (UserProfile up in profileManager)
            {

                if (progressUpdate != null)
                {
                    curProfile++;
                    progressUpdate((int)(curProfile * 100 / profCount));
                }

                SPUser user = null;
                costcenter = Constants.INVALID;
                username = Constants.INVALID; 

                // step 1 of 3: fetch a user and update properties in list
                try
                {
    
                    //fetch a valid user along with username and costcenter

                    user = GetValidUser(site, up, ref username, ref costcenter);

                    if ((user == null) || (costcenter == Constants.INVALID) || (username == Constants.INVALID))
                        continue;

                    UpdateUserItem(listUserProfile, up, username, costcenter);

                }
                catch (Exception ex) {
                    Trace.WriteLine("UpdateCostCenterGroups: Error Fetching and/or Updating User: " + up.DisplayName + " "  + ex.ToString());
                    continue;
                }

                try
                {
                    // step 2 of 3: update groups and membership for this user
                    EnsureCostCenterGroupsForUser(site, user, costcenter, username);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("EnsureCostCenterGroupsForUser: Error for user/costcenter:" + user.ToString() + " " + costcenter + " " + ex.ToString());
                    continue;
                }


            }
        }
    }


}
