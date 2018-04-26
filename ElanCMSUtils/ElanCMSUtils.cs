using System;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Elan.SharePoint.LRFApproval;
using CMSCommon;
using Elan.CMSPages;

namespace ElanCMSUtils
{

    class ElanCMSUtils
    {

        //static void UpdateLegalGroups(SPWeb web)
        //{


        //    // for all LRFs
        //    // if legal group field found
        //    //remove Legal Team Permissions
        //    //replace with Legal Group
            
        //    SPListItemCollection items = web.Lists["Legal Request Forms"].Items;

        //    SPGroup previousLegalTeam = web.SiteGroups["Legal Team"]; 
            
        //    foreach (SPListItem item in items)
        //    {
        //      //  Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

        //        if (item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalTeamAssigned] != null &&
        //            (!String.IsNullOrEmpty(item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalTeamAssigned].ToString())))
        //        {
        //            SPGroup newLegalTeam = web.SiteGroups[item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalTeamAssigned].ToString()];

        //            if (newLegalTeam != null)
        //            {
        //                Elan.SharePoint.LRFApproval.Common.Security.ClearItemSecurity(item, previousLegalTeam);
        //                Elan.SharePoint.LRFApproval.Common.Security.SetItemContribute(item, newLegalTeam);
        //            }
        //         }

        //    }


        //}


        static void UpdateLRFCostCenters(SPListItem item)
        {

            try
            {
                //get the lrf request
                Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

                // update approval fields not previously set (requestor and lrf cost center)
                lrf.LegacyLRFCostCenterFields(true);

            }
            catch { }

        }

        static void UpdateLRFCostCenters(SPWeb web)
        { 

            // for all LRFs (anything other than draft)
            // compute
            // Requestor Cost Center
            // LRF Cost Center
            // Cost Center List
            // populate LRF fields with these values
            // if field empty, populate            

            SPListItemCollection items = web.Lists["Legal Request Forms"].Items; 

            foreach (SPListItem item in items)
            {
//                if (String.IsNullOrEmpty(item[Elan.SharePoint.LRFApproval.Common.LRF.FieldRequestorCostCenter].ToString())) 
                    UpdateLRFCostCenters(item);
            }
        
        }

        static void FixLegalGroup(SPWeb web)
        {
            foreach (SPNavigationNode node in web.Navigation.TopNavigationBar)
            {
                if (node.Title == "Legal Review Center")
                {
                    Console.WriteLine(node.Title + " " + node.Properties["Audience"].ToString());
                    node.Properties["Audience"] = ";;;;Legal Team";
                    node.Update();
                    Console.WriteLine(node.Title + " " + node.Properties["Audience"].ToString());
                }

                if (node.Title == "Reports")
                {
                    Console.WriteLine(node.Title + " " + node.Properties["Audience"].ToString());
                    node.Properties["Audience"] = ";;;;Legal Team, Finance Team";
                    node.Update();
                    Console.WriteLine(node.Title + " " + node.Properties["Audience"].ToString());
                }

            }
        }

        static void Main(string[] args)
        {
            Console.Write("RequestUrl: ");
            string requestUrl = Console.ReadLine();

            Console.Write("Function: ");
            string functionName =  Console.ReadLine();
                        
            using (SPSite site = new SPSite(requestUrl))
            {
                SPWebCollection sites = site.AllWebs;

                SPWeb web = sites[0];


                switch (functionName.ToLower())
                {
                    case "fixlegalgroup":
                        FixLegalGroup(web);
                        break;
                    case "updatecostcenters":
                        UpdateLRFCostCenters(web);                        
                        break;

                }

                Console.Write("Press ENTER to continue");
                Console.ReadLine();

            }
        }
    }
}
