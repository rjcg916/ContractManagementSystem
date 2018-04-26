using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.Office.Server.UserProfiles;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;


namespace LRFApprovalTest
{

    class User
    {
        internal static void GetUserManagerProfile_existing_return(SPWeb web)
        {

            SPUser user = web.Users[@"ecorp\webgrouptest1"];

            UserProfile up = Elan.SharePoint.LRFApproval.Common.User.GetUserManagerProfile(web, user);

            Console.WriteLine("webgrouptest1 " + up.DisplayName);
            Console.ReadKey();

        }

        internal static void GetUserProfile_existing_return(SPWeb web)
        {

            SPUser user = web.Users[@"ecorp\webgrouptest1"];

            UserProfile up = Elan.SharePoint.LRFApproval.Common.User.GetUserProfile(web, user);

            Console.WriteLine("webgrouptest1 " + up.DisplayName);
            Console.ReadKey();

        }
 
        internal static void GetUserAttributes_existing_return(SPWeb web)
        {
            SPUser user = web.Users[@"ecorp\webgrouptest1"];
            int band;
            int AuthAmount;
            SPUser manager;
            string costCenter;
            Elan.SharePoint.LRFApproval.Common.User.GetUserAttributes(web, user, out band, out AuthAmount, out manager, out costCenter);

            Console.WriteLine("User: " + user.Name + " band: " + band + " AuthAmount: " + AuthAmount + " mgr: " + manager.Name + " costCenter:" + costCenter);
            Console.ReadKey();
        }


        internal static void GetUserFromField_existing_return(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items[1];
            string userField = "Created By";
            SPUser user = Elan.SharePoint.LRFApproval.Common.User.GetUserFromField(item, userField);

            Console.WriteLine("User: " + user.Name);
            Console.ReadKey();        
        }

        //internal static void GetGroupFromField_existing_return(SPWeb web)
        //{
        //    SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(836);
        //    string groupField = "Assigned To Legal";
        //    SPGroup group = Elan.SharePoint.LRFApproval.Common.User.GetGroupFromField(item, groupField);

        //    Console.WriteLine("Group: " + group.Name);
        //    Console.ReadKey();
        //}

        internal static void AddGroupToField_valid_added(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(836);
            SPGroup group = web.SiteGroups["Legal Group 5"];
            item["Assigned To Legal"] = group;
            item.Update();
        }

        internal static void AddUserToField_valid_added(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items[1];
            SPFieldUserValueCollection approversDeptField = new SPFieldUserValueCollection();

            SPUser user1 = web.Users[@"ecorp\webgrouptest4"];
            SPUser user2 = web.Users[@"ecorp\webgrouptest3"];

            Elan.SharePoint.LRFApproval.Common.User.AddUserToField(web, approversDeptField, user1);
            Elan.SharePoint.LRFApproval.Common.User.AddUserToField(web, approversDeptField, user2);

            item["DeptApprovers"] = approversDeptField;

            item.Update();
        }


    }
}
