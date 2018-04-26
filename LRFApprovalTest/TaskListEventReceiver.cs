using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace LRFApprovalTest
{
    class TaskListEventReceiver
    {

        static void Update_LRF_Permissions_Approver_Remove(SPWeb web)
        {


            // previous valud for %Completion
            string oldValue = string.Empty;

            //get task id
            SPListItem itemTask = web.Lists["Tasks"].Items[0];


            if ((itemTask["% Complete"] != oldValue)
                &&
                (itemTask["% Complete"] == "100%"))
            {

                // get task owner
//                SPUser user = itemTask["Assigned To"];
                  SPUser user = null;

                //find LRF associated with this task
                //                  int lrfID = itemTask["LRF Number"];
                  int lrfID = int.MinValue;

                  SPListItem itemLRF = web.Lists["Legal Request Form"].Items[lrfID];

                 //find out if task owner is approver
            //      string approversList = itemLRF["Approvers"];

            //      bool found = approversList.First(user);
    
                  // if task owner is approver, remove permissions
            //      if (found)
                      Elan.SharePoint.LRFApproval.Common.Security.ClearItemSecurity(itemLRF, user);

            }

                // check for list, if not LRF/task list, quit
        // check that completion status is changing and that task is completing, if not quit
        // get user name and LRF id
        // find user name on LRF approver list
        // if user has permission, remove permission on associated LRF
        
        }
    }
}
