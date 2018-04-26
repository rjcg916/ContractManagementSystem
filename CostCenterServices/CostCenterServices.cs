using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.Office.Server.UserProfiles;
using System.Diagnostics;
using CMSCommon;

namespace CostCenterServices
{
    
    public class ElanUser
    {
        string DisplayNameValue = "";
        public string DisplayName
        {
            get { return DisplayNameValue; }
            set { DisplayNameValue = value; }
        }

        int IdValue = 0;
        public int Id
        {
            get { return IdValue; }
            set { IdValue = value; }
        }

        string LoginNameValue = "";
        public string LoginName
        {
            get { return LoginNameValue; }
            set { LoginNameValue = value; }
        }


    }

    
    public class ElanUserProfile
    {
        string CostCenterValue = "";
        public string CostCenter
        {
            get { return CostCenterValue; }
            set { CostCenterValue = value; }
        }


        string OfficeValue = "";
        public string Office
        {
            get { return OfficeValue; }
            set { OfficeValue = value; }

        }

        string LocationValue = "";
        public string Location
        {
            get { return LocationValue; }
            set { LocationValue = value; }

        }

        string PhoneNumberValue = "";
        public string PhoneNumber
        {
            get { return PhoneNumberValue; }
            set { PhoneNumberValue = value; }
        }

        string DeptNumberValue = "";
        public string DeptNumber
        {
            get { return DeptNumberValue; }
            set { DeptNumberValue = value; }
        }

        string DeptNameValue = "";
        public string DeptName
        {
            get { return DeptNameValue; }
            set { DeptNameValue = value; }
        }

        string EmailAddressValue = "";
        public string EmailAddress
        {
            get { return EmailAddressValue; }
            set { EmailAddressValue = value; }
        }

        string SapUsernameValue = "";
        public string SapUsername
        {
            get { return SapUsernameValue; }
            set { SapUsernameValue = value; }
        }

    }
    

    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1), WebService(Namespace = "http://tempuri.org")]
    class CostCenterServices : System.Web.Services.WebService
    {



        [WebMethod(EnableSession = true, Description = "return profile attributes for a user")]
        public ElanUserProfile GetUserProfile(string accountName)
        {

            Trace.WriteLine("GetUserProfile: accountName: " + accountName);

            ElanUserProfile eup = new ElanUserProfile();

            SPSite site = SPContext.Current.Web.Site;
            SPServiceContext context = SPServiceContext.GetContext(site);
            UserProfileManager profileManager = new UserProfileManager(context);


            try
            {
                UserProfile up = profileManager.GetUserProfile(accountName);

                // Trace.WriteLine("GetUserProfile: DisplayName" + up.DisplayName.ToString());

                try
                {
                    eup.CostCenter = up[PropertyConstants.Department].Value.ToString().Substring(0, Constants.COSTCENTERLENGTH);
                }
                catch { };


                try
                {
                    eup.DeptName = up[PropertyConstants.Department].Value.ToString().Substring(Constants.COSTCENTERLENGTH + 1);
                }
                catch { };


                try
                {
                    eup.DeptNumber = up[PropertyConstants.Department].Value.ToString().Substring(0, Constants.COSTCENTERLENGTH);
                }
                catch { };

                try
                {
                    eup.Location = up[PropertyConstants.Location].Value.ToString();
                }
                catch { };

                try
                {
                    eup.Office = up[PropertyConstants.Office].Value.ToString();
                }
                catch { };

                try
                {
                    eup.PhoneNumber = up[PropertyConstants.WorkPhone].Value.ToString();
                }
                catch { };

                try
                {
                    eup.EmailAddress = up[PropertyConstants.WorkEmail].Value.ToString();
                }
                catch { };

                try
                {
                    eup.SapUsername = up[Constants.SAPUSERNAMEFIELD].Value.ToString();
                }
                catch { };
            }
            catch (Exception ex)
            {
                Trace.WriteLine("catch GetUserProfile: " + ex.Message + " " + ex.StackTrace);
            }

            return eup;
        }


        private List<ElanUser> CoreGetAllowedRequestors(SPWeb web, string accountName)
        {

 
            SPSite site = web.Site;

            List<ElanUser> sc = new List<ElanUser>();

            Dictionary<int, ElanUser> CostCenterRequestors = new Dictionary<int, ElanUser>(); //allows management of duplicates

            SPGroupCollection theGroups = site.RootWeb.SiteGroups;

            SPUser theUser = null;

            Trace.WriteLine("CoreGetAllowedRequestors: accountName: " + accountName);

            try
            {

                SPServiceContext context = SPServiceContext.GetContext(site);
                UserProfileManager profileManager = new UserProfileManager(context);
                UserProfile up = profileManager.GetUserProfile(accountName);
                string email = up[PropertyConstants.WorkEmail].Value.ToString();

//                Trace.WriteLine("CoreGetAllowedRequestors: email: " + email.ToString());
                theUser = web.SiteUsers.GetByEmail(email);
//                Trace.WriteLine("CoreGetAllowedRequestors: theUser.LoginName: " + theUser.LoginName);

            }
            catch (Exception ex)
            {
                Trace.WriteLine("catch CoreGetAllowedRequestors: accountName: " +  accountName + " " + ex.Message + " " + ex.StackTrace);

                return sc;
            }

            theGroups = site.RootWeb.SiteGroups;
//            Trace.WriteLine("CoreGetAllowedRequestors: theUser.Groups.Count: " + theUser.Groups.Count);

            if ((theUser == null) || (theUser.Groups.Count == 0))
                return sc;

            // build list of cost centers for user
            StringDictionary CostCenters = new StringDictionary();
            foreach (SPGroup ugroup in theUser.Groups)
            {

                if (ugroup.Name.StartsWith(Constants.COSTCENTERPROFILEPREFIX))
                {
                    string cc = ugroup.Name.Substring(Constants.COSTCENTERPROFILEPREFIX.Length);
                    CostCenters[cc] = cc;
                }

                if (ugroup.Name.StartsWith(Constants.COSTCENTERUSERPREFIX))
                {
                    string cc = ugroup.Name.Substring(Constants.COSTCENTERUSERPREFIX.Length);
                    CostCenters[cc] = cc;
                }

                if (ugroup.Name.StartsWith(Constants.COSTCENTERSUPERUSERPREFIX))
                {
                    string cc = ugroup.Name.Substring(Constants.COSTCENTERSUPERUSERPREFIX.Length);
                    CostCenters[cc] = cc;
                }

            }

//            Trace.WriteLine("CoreGetAllowedRequestors: CostCenters.Count: " + CostCenters.Count);

            if (CostCenters.Count == 0)
                return sc;



//            Trace.WriteLine("CoreGetAllowedRequestors: theGroups.Count: " + theGroups.Count);
//            foreach (SPGroup entry in theGroups)
//            {
//                Trace.WriteLine("CoreGetAllowedRequestors: SPGroup[i].Name: " + entry.Name);
//            }

            // for each allowed cost center, find all other users in all groups
            foreach (DictionaryEntry entry in CostCenters)
            {

//                Trace.WriteLine("CoreGetAllowedRequestors: CostCenters[i]: " + entry.Key.ToString());

//                Trace.WriteLine("CoreGetAllowedRequestors: Settings.Default.COSTCENTERPROFILEPREFIX + entry.Key: " + Settings.Default.COSTCENTERPROFILEPREFIX + entry.Key);

                foreach (SPUser user in theGroups[Constants.COSTCENTERPROFILEPREFIX + entry.Key].Users)
                {
//                    Trace.WriteLine("CoreGetAllowedRequestors: Profile: " + user.Name);

                    ElanUser eu = new ElanUser();
                    eu.Id = user.ID;
                    eu.LoginName = user.LoginName;
                    eu.DisplayName = user.Name;
                    CostCenterRequestors[user.ID] = eu;
                }


                foreach (SPUser user in theGroups[Constants.COSTCENTERUSERPREFIX + entry.Key].Users)
                {
//                    Trace.WriteLine("CoreGetAllowedRequestors: User: " + user.Name);

                    ElanUser eu = new ElanUser();
                    eu.Id = user.ID;
                    eu.LoginName = user.LoginName;
                    eu.DisplayName = user.Name;
                    CostCenterRequestors[user.ID] = eu;
                }

                foreach (SPUser user in theGroups[Constants.COSTCENTERSUPERUSERPREFIX + entry.Key].Users)
                {
//                    Trace.WriteLine("CoreGetAllowedRequestors: SuperUser: " + user.Name);

                    ElanUser eu = new ElanUser();
                    eu.Id = user.ID;
                    eu.LoginName = user.LoginName;
                    eu.DisplayName = user.Name;
                    CostCenterRequestors[user.ID] = eu;
                }

            }

//            Trace.WriteLine("CoreGetAllowedRequestors: CostCenterRequestors.Count: " + CostCenterRequestors.Count);


            //for return value, place the results in a list
            foreach (var pair in CostCenterRequestors)
            {
                ElanUser item = new ElanUser();
                item.Id = pair.Value.Id;
                item.DisplayName = pair.Value.DisplayName;
                item.LoginName = pair.Value.LoginName;
                sc.Add(item);
            }

            return sc;   
        
        }

        [WebMethod(EnableSession = true, Description = "return a list of users that are allowed to submit requests for the user with accountName")]
        public List<ElanUser> GetAllowedRequestors(string accountName)
        {
            List<ElanUser> sc = null;

            SPWeb web = SPContext.Current.Web;

            // need elevated privs to access groups and memberships
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite elevatedSite = new SPSite(web.Site.ID))
                {
                    SPWeb elevatedWeb = elevatedSite.OpenWeb(web.ID);

                    sc = CoreGetAllowedRequestors(elevatedWeb, accountName);
                }
            });   

            return sc;
        }

  
        //[WebMethod (EnableSession=true, Description="return a list of users that are allowed to submit requests for the user with accountName")]
        //public List<ElanUser> orgGetAllowedRequestors(string accountName)
        //{

        //    SPWeb web = SPContext.Current.Web;
        //    SPSite site = web.Site;

        //    List<ElanUser> sc = new List<ElanUser>();

        //    Dictionary<int, ElanUser> CostCenterRequestors = new Dictionary<int, ElanUser>(); //allows management of duplicates


        //    SPGroupCollection theGroups = site.RootWeb.SiteGroups;           
            
        //    SPUser theUser = null;

        //    Trace.WriteLine("GetAllowedRequestors: GetUserProfile");
                   
        //    try
        //    {
                 
        //        SPServiceContext context = SPServiceContext.GetContext(site);
        //        UserProfileManager profileManager = new UserProfileManager(context);
        //        UserProfile up = profileManager.GetUserProfile(accountName);
        //        string email = up[PropertyConstants.WorkEmail].Value.ToString();

        //        Trace.WriteLine("GetAllowedRequestors: email: " + email.ToString());
        //        theUser = web.SiteUsers.GetByEmail(email);
        //        Trace.WriteLine("GetAllowedRequestors: theUser.LoginName: " + theUser.LoginName);
                           
        //    }
        //    catch
        //    {
        //        return sc;
        //    }

        //    Trace.WriteLine("GetAllowedRequestors: theUser.Groups.Count: " + theUser.Groups.Count);

        //    if ((theUser == null) || (theUser.Groups.Count == 0))
        //        return sc;

        //    // build list of cost centers for user
        //    StringDictionary CostCenters = new StringDictionary();
        //    foreach (SPGroup ugroup in theUser.Groups)
        //    {

        //        if (ugroup.Name.StartsWith(Settings.Default.COSTCENTERPROFILEPREFIX))
        //        {
        //            string cc = ugroup.Name.Substring(Settings.Default.COSTCENTERPROFILEPREFIX.Length);
        //            CostCenters[cc] = cc;
        //        }

        //        if (ugroup.Name.StartsWith(Settings.Default.COSTCENTERUSERPREFIX))
        //        {
        //            string cc = ugroup.Name.Substring(Settings.Default.COSTCENTERUSERPREFIX.Length);
        //            CostCenters[cc] = cc;
        //        }

        //        if (ugroup.Name.StartsWith(Settings.Default.COSTCENTERSUPERUSERPREFIX))
        //        {
        //            string cc = ugroup.Name.Substring(Settings.Default.COSTCENTERSUPERUSERPREFIX.Length);
        //            CostCenters[cc] = cc;
        //        }

        //    }

        //    Trace.WriteLine("GetAllowedRequestors: CostCenters.Count: " + CostCenters.Count);

        //    if (CostCenters.Count == 0)
        //        return sc;


        //    Trace.WriteLine("GetAllowedRequestors: theGroups.Count: " + theGroups.Count);

        //    foreach (SPGroup entry in theGroups)
        //    {
        //        Trace.WriteLine("GetAllowedRequestors: SPGroup[i].Name: " + entry.Name);
        //    }

        //    // for each allowed cost center, find all other users in all groups
        //    foreach (DictionaryEntry entry in CostCenters)
        //    {

        //        Trace.WriteLine("GetAllowedRequestors: CostCenters[i]: " + entry.Key.ToString());

        //        Trace.WriteLine("GetAllowedRequestors: Settings.Default.COSTCENTERPROFILEPREFIX + entry.Key: " + Settings.Default.COSTCENTERPROFILEPREFIX + entry.Key);

        //        foreach (SPUser user in theGroups[Settings.Default.COSTCENTERPROFILEPREFIX + entry.Key].Users)
        //        {
        //            Trace.WriteLine("GetAllowedRequestors: Profile: " + user.Name);

        //            ElanUser eu = new ElanUser();
        //            eu.Id = user.ID;
        //            eu.LoginName = user.LoginName;
        //            eu.DisplayName = user.Name;
        //            CostCenterRequestors[user.ID] = eu;
        //        }


        //        foreach (SPUser user in theGroups[Settings.Default.COSTCENTERUSERPREFIX + entry.Key].Users)
        //        {
        //            Trace.WriteLine("GetAllowedRequestors: User: " + user.Name);

        //            ElanUser eu = new ElanUser();
        //            eu.Id = user.ID;
        //            eu.LoginName = user.LoginName;
        //            eu.DisplayName = user.Name;
        //            CostCenterRequestors[user.ID] = eu;
        //        }

        //        foreach (SPUser user in theGroups[Settings.Default.COSTCENTERSUPERUSERPREFIX + entry.Key].Users)
        //        {
        //            Trace.WriteLine("GetAllowedRequestors: SuperUser: " + user.Name);

        //            ElanUser eu = new ElanUser();
        //            eu.Id = user.ID;
        //            eu.LoginName = user.LoginName;
        //            eu.DisplayName = user.Name;
        //            CostCenterRequestors[user.ID] = eu;
        //        }

        //    }

        //    Trace.WriteLine("GetAllowedRequestors: CostCenterRequestors.Count: " + CostCenterRequestors.Count);


        //    //for return value, place the results in a list
        //    foreach (var pair in CostCenterRequestors)
        //    {
        //        ElanUser item = new ElanUser();
        //        item.Id = pair.Value.Id;
        //        item.DisplayName = pair.Value.DisplayName;
        //        item.LoginName = pair.Value.LoginName;
        //        sc.Add(item);
        //    }

        //    return sc;
        //}

    }
}
