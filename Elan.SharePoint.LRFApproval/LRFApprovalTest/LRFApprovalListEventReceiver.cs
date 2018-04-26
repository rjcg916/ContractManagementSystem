using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval;

namespace LRFApprovalTest
{

    public class LRFApprovalListEventReceiver
    {


        public static void LegalAssigneeChanged_NoChange_False(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1082);
            item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalOwner] = item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalOwner];
            item.Update();

        }

        public static void LegalAssigneeChanged_Change_True(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1082);
            item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalOwner] = web.SiteUsers[@"ecorp\webgrouptest4"].ID;
            item.Update();

        }

        public static void LegalTeamChanged_NoChange_False(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1082);

            item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalTeamAssigned] = item[Elan.SharePoint.LRFApproval.Common.LRF.FieldLegalTeamAssigned];
            item.Update();

        }

        public static void LegalTeamChanged_Change_True(SPWeb web)
        {

            SPListItem item = web.Lists["Legal Request Forms"].Items.GetItemById(1082);
            SPGroup group = web.SiteGroups["Legal Group 6"];

            item["Assigned To Legal"] = group;
            item.Update();

        }
    }
}
