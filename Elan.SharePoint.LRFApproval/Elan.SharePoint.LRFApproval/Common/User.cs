using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Elan.SharePoint.LRFApproval.Common;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class User
    {
        private static readonly string ProfileDepartmentFieldName = "Department";
        private static readonly int CostCenterDigits = 5;

        public static UserProfile GetUserProfile(SPWeb web, SPUser user)
        {
            UserProfile currentUserProfile = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var site = new SPSite(web.Site.ID))
                using (var web1 = site.OpenWeb(web.ID))
                {
                    try
                    {
                        SPServiceContext siteContext = SPServiceContext.GetContext(site);
                        UserProfileManager userProfileManager = new UserProfileManager(siteContext);
                        currentUserProfile = userProfileManager.GetUserProfile(user.LoginName);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteOnlyLogEntry(web1, "GetUserProfile: Error: " + user.LoginName + " " , ex.ToString());
                    }
                }
            });
            return currentUserProfile;
        }
        
        public static UserProfile GetUserManagerProfile(SPWeb web, SPUser user)
        {
            UserProfile currentUserProfile = null;
            UserProfile managerProfile = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var site = new SPSite(web.Site.ID))
                using (var web1 = site.OpenWeb(web.ID))
                {
                    try
                    {

                        SPServiceContext siteContext = SPServiceContext.GetContext(site);
                        UserProfileManager userProfileManager = new UserProfileManager(siteContext);
                        currentUserProfile = userProfileManager.GetUserProfile(user.LoginName);

                    }
                    catch (Exception ex)
                    {

                        Log.WriteOnlyLogEntry(web1, "GetUserManagerProfile: GetUserProfile: Error: ", ex.ToString());
                    }

                    try
                    {
                        managerProfile = currentUserProfile.GetManager();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteOnlyLogEntry(web1, "GetUserManagerProfile: GetManager: Error: " + currentUserProfile.DisplayName + " ", ex.ToString());
                    }
                }
            });

            return managerProfile;
        }

        public static SPUser GetUserFromField(SPListItem item, string userField)
        {

            if (item[userField] != null && !string.IsNullOrEmpty(item[userField].ToString()))
            {
                string uname = item[userField].ToString();

                SPFieldUser ufield = item.Fields.GetField(userField) as SPFieldUser;
                SPFieldUserValue ufieldValue = ufield.GetFieldValue(uname) as SPFieldUserValue;
                
                if (ufieldValue != null)
                {
                    SPUser user = ufieldValue.User;

                    if ((user != null) && !(string.IsNullOrEmpty(user.Email)))
                    {
                        return user;
                    }
                }
            }

            return null;
        }


        public static List<SPUser> GetUsersFromField(SPListItem item, string usersField)
        {
            List<SPUser> users = null;

            if ( (item[usersField] != null) && !string.IsNullOrEmpty(item[usersField].ToString()))
            {

                users = new List<SPUser>();

                SPFieldUserValueCollection values = new SPFieldUserValueCollection(item.Web, item[usersField].ToString());
                foreach (SPFieldUserValue userValue in values)
                {
                    users.Add(userValue.User);
                     
                }
                return users;
            }
            return null;
        }

        public static void AddUserToField(SPWeb web, SPFieldUserValueCollection users, SPUser user)
        {
            if ((user == null) || (web == null))
                return;

            SPFieldUserValue value;
            value = new SPFieldUserValue(web, user.ID, user.LoginName);

            if (value != null)
            {
                if (!users.Contains(value))
                    users.Add(value);
            }
        }

        public static bool GetUserAttributes(SPWeb web, SPUser user, out int band, out int AuthAmount, out SPUser manager, out string costCenter)
        {
            manager = null;
            UserProfile userProfile = Common.User.GetUserProfile(web, user);

            // if can't find profile, quit
            if (userProfile == null)
            {
                costCenter = string.Empty;
                manager = null;
                AuthAmount = 0;
                band = 0;
                return false;
            }

            // find band
            band = 0; // if band not found, it will be 0
            if (userProfile[Settings.Default.UserprofileFieldBand] != null && userProfile[Settings.Default.UserprofileFieldBand].Value != null)
            {
                string strBand = userProfile[Settings.Default.UserprofileFieldBand].ToString();
                band = Util.MakeInt(strBand);
            }

            //find authorized amount, default to 0
            AuthAmount = 0;
            if (userProfile[Settings.Default.UserProfileFieldAuthAmount] != null && userProfile[Settings.Default.UserProfileFieldAuthAmount].Value != null)
            {
                if (!string.IsNullOrEmpty(userProfile[Settings.Default.UserProfileFieldAuthAmount].ToString()))
                {
                    string strAuthAmount = userProfile[Settings.Default.UserProfileFieldAuthAmount].ToString();
                    AuthAmount = Util.MakeInt(strAuthAmount);
                }
            }

            costCenter = string.Empty;
            if (userProfile[ProfileDepartmentFieldName] != null && userProfile[ProfileDepartmentFieldName].Value != null)
            {
                costCenter = userProfile[ProfileDepartmentFieldName].ToString();
                if (costCenter.Length >= CostCenterDigits )
                    costCenter = costCenter.Substring(0, CostCenterDigits);
            }

            //find manager, return null if not found
            UserProfile managerProfile = Common.User.GetUserManagerProfile(web, user);
            if (managerProfile == null)
            {
                manager = null;
            }
            else
            {
                manager = null;
                try
                {
                    string managerName = managerProfile[PropertyConstants.AccountName].ToString(); 
                    manager = web.EnsureUser(managerName);
                }
                catch (Exception ex) {
                    Log.WriteOnlyLogEntry(web, "GetUserAttributes: Error: MangerProfile Account Not Found: User: " + user.Name, ex.ToString());
                }
            }

            return true;
        }


    }
}
