using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using CMSCommon;
using ElanCostCenterGroups;


namespace ElanCostCenterGroupsTest
{

    class Utils
    {
        //UserExists
        internal static void UserExists_exists_splistitem(SPSite site)
        {
            //arrange
            SPList listUserProfile = site.RootWeb.Lists[CMSCommon.Constants.CMSUSERPROFILELIST];

            string username = @"webgrouptest1";

            //do
            SPListItem liUser = null;
            liUser = ElanCostCenterGroups.Utils.UserExists(listUserProfile, username);

            //act
            if (liUser == null)
                Console.WriteLine("UserExists_userexists_true: failed!");

        }   

        internal static void UserExists_nouser_null(SPSite site)
        {
            //arrange
            SPList listUserProfile = site.RootWeb.Lists[CMSCommon.Constants.CMSUSERPROFILELIST];

            string username = @"xxx";

            //do
            SPListItem liUser = null;
            liUser = ElanCostCenterGroups.Utils.UserExists(listUserProfile, username);

            //act
            if (liUser != null)
                Console.WriteLine("UserExists_nouser_false: failed!");

        }    
        
        //GetValidUser

        internal static void GetValidUser_validuser_find(SPSite site, UserProfileManager pm)
        { 
           
            string username = string.Empty;
            string costcenter = string.Empty;

            UserProfile up = pm.GetUserProfile(@"ecorp\webgrouptest1");
            SPUser user = ElanCostCenterGroups.Utils.GetValidUser(site, up, ref username, ref costcenter);

            if ((username != "WebgroupTest1") ||
                (costcenter != "51510") ||
                (user.Name != "WebgroupTest1")
                )
                Console.WriteLine("GetValidUser_validuser_find failed!");

        }

        internal static void GetValidUser_invalidcostcenter_nulluser(SPSite site, UserProfileManager pm)
        {

            string username = string.Empty;
            string costcenter = string.Empty;
            SPUser user = null;

            UserProfile up = pm.GetUserProfile(@"ecorp\v-bob.graham");
            user = ElanCostCenterGroups.Utils.GetValidUser(site, up, ref username, ref costcenter);

            if ((ElanCostCenterGroups.Constants.INVALID != username ) ||
                (ElanCostCenterGroups.Constants.INVALID != costcenter) ||
                (user != null)
              )
                Console.WriteLine("GetValidUser_invaliduser_null failed!");
        }

        //UpdateUserItem
        internal static void UpdateUserItem_newinfo_insert(SPSite site, UserProfileManager pm)
        {

            SPList listUserProfile = site.RootWeb.Lists[CMSCommon.Constants.CMSUSERPROFILELIST];

            string costcenter = "000000";
            string username = "v-bob.graham";
            UserProfile up = pm.GetUserProfile(@"ecorp\v-bob.graham");

            ElanCostCenterGroups.Utils.UpdateUserItem(listUserProfile, up, username, costcenter);

        }


        //EnsureGroupsForCostCenter
        internal static void EnsureCostCenterGroupsForUser_newcostcenter_insert(SPSite site)
        {
            string username = "v-bob.graham";
            string costcenter = "51427";
            SPUser user = site.RootWeb.Users.GetByEmail("Bob.Graham@elan.com");
          
            //create new groups, add user to profile group, remove user from old profile group
            ElanCostCenterGroups.Utils.EnsureCostCenterGroupsForUser(site, user, costcenter, username);
        }


        internal static void UpdateCostCenterGroups_run_output(SPSite site)
        {
            ElanCostCenterGroups.Utils.UpdateCostCenterGroups(site, null);
        }   
    
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            using (SPSite site = new SPSite(CMSCommon.Constants.CMSDEVURL))
            {
                SPServiceContext context = SPServiceContext.GetContext(site);

                UserProfileManager profileManager = new UserProfileManager(context);

                //Utils.EnsureCostCenterGroupsForUser_newcostcenter_insert(site);

                //Utils.UpdateUserItem_newinfo_insert(site, profileManager);

                //Utils.UpdateCostCenterGroups_run_output(site);

                //Utils.GetValidUser_validuser_find(site, profileManager);

                //Utils.GetValidUser_invalidcostcenter_nulluser(site, profileManager);
                
                //Utils.UserExists_exists_splistitem(site);

                //Utils.UserExists_nouser_null(site);

            }
        }
    }
}
