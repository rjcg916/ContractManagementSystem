using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Common;

namespace LRFApprovalTest
{
    class LRF
    {


        public static void ResetLegalSecurity_New_Switched(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(836);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            SPGroup previousLegalTeam = web.SiteGroups["Legal Group 3"];
            SPGroup newLegalTeam = web.SiteGroups["Legal Group 2"];
            lrf.ResetLegalSecurity(previousLegalTeam, newLegalTeam);
        }

        public static void SetSecurity_Draft_Set(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1019);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            lrf.SetSecurity();
            
        }

        public static void SetSecurity_Submit_Set(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1014);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            lrf.SetSecurity();
        }

        public static void SetSecurity_Approved_Set(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(995);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            lrf.SetSecurity();
        }


        public static void LRF_NoCostInfo_Null(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(960);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            CostCenterCharge[] ccc = lrf.costCenters;

        }

        public static void LRF_OneValue_CC(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(986);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            CostCenterCharge[] ccc = lrf.costCenters;

        }

        public static void LRF_MultiCC_MaxCC(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1005);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            CostCenterCharge[] ccc = lrf.costCenters;

        }

        public static void GetMaxCostCenter_NoCostInfo_Null(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(960);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

   //         Decimal maxCostCenterValue;
  //          string maxCostCenter = lrf.GetMaxCostCenter(out maxCostCenterValue);


        }

        public static void GetMaxCostCenter_OneValue_CC(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(986);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

  //          Decimal maxCostCenterValue;
 //           string maxCostCenter = lrf.GetMaxCostCenter(out maxCostCenterValue);

        }

        public static void GetMaxCostCenter_MultiCC_MaxCC(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1005);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

    //       Decimal maxCostCenterValue;
   //         string maxCostCenter = lrf.GetMaxCostCenter(out maxCostCenterValue);
        }
        
        public static void LRF_costcenter_assign(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(960);

            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string msg;

            msg = String.Format("Valid {0} Status {1} Requestor {2} RequestorCC: {3} AssignedCC: {4}", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor, item["Requestor_x0020_Cost_x0020_Center"], item["Assigned_x0020_Cost_x0020_Center"]);

            Console.WriteLine(msg);

            lrf.SetApprovalFields();
      //      if (lrf.needsUpdate)
                item.Update();

            msg = String.Format("Valid {0} Status {1} Requestor {2} RequestorCC: {3} AssignedCC: {4}", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor, item["Requestor_x0020_Cost_x0020_Center"], item["Assigned_x0020_Cost_x0020_Center"]);

            Console.WriteLine(msg);

            
            Console.ReadKey();
        }

        public static void LRF_costcenterdetails_fetch(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(995);

            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
 
            string msg;
            msg = String.Format("Valid {0} Status {1} Requestor {2} ", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor);

            Console.WriteLine(msg);

//            decimal maxCostCenterValue;
 //           string maxCostCenter = lrf.GetMaxCostCenter(out maxCostCenterValue);

//            Console.WriteLine("Cost Center: " + maxCostCenter + " value: " + maxCostCenterValue.ToString() );

            Console.WriteLine();

            Console.ReadKey();
        }

        public static void LRF_draft_exist(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(865); 
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string msg = String.Format("Valid {0} Status {1} Amt {2} Creator {3} Requestor {4}", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        public static void LRF_submit_exist(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(864); 
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string msg = String.Format("Valid {0} Status {1} Amt {2} Creator {3} Requestor {4}", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        public static void LRF_canceled_exist(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(897);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string msg = String.Format("Valid {0} Status {1} Amt {2} Creator {3} Requestor {4}", lrf.valid, lrf.status, lrf.requestAmount, lrf.creator, lrf.requestor);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        public static void LRF_bylrfnumber_exist(SPWeb web)
        {

            int lrfnumber = 777;
            SPListItem item = Elan.SharePoint.LRFApproval.Common.LRF.GetItemById(web, lrfnumber);

//          Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string msg = String.Format("Item {0}", item.Title);

            Console.WriteLine(msg);
            Console.ReadKey();

        }
        public static void CreateEmailComponents_Assigned_Parts(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(836);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string recipients = null;
            string subject = null;
            string body = null;
            Elan.SharePoint.LRFApproval.Common.LRF.CreateEmailComponents(item, "{0} with {1} is assigned to {2}", out recipients, out subject, out body);
        }
  
        public static void PartyName_Party_Valid(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1084);

            string partyName = Elan.SharePoint.LRFApproval.Common.LRF.GetPartyName(item);
        }

        public static void CreateEmailComponents_Executed_Parts(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(836);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);

            string recipients = null;
            string subject = null;
            string body = null;
            Elan.SharePoint.LRFApproval.Common.LRF.CreateEmailComponents(item, "Agreement {0} with {1} has been fully executed by {2}", out recipients, out subject, out body);
        }

        public static void LRF_NoCostCenters_Done(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1084);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            lrf.SetCostCentersField();
            item.Update();

        }
        public static void LRF_SetCostCenters_Done(SPWeb web)
        {
            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1082);
            Elan.SharePoint.LRFApproval.Common.LRF lrf = new Elan.SharePoint.LRFApproval.Common.LRF(item);
            lrf.SetCostCentersField();
            item.Update();

        }
  }


}
