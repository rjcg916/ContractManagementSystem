using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;

namespace Elan.SharePoint.LRFApproval.TaskListEventReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class TaskListEventReceiver : SPItemEventReceiver
    {

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {

            SPListItem item = null;
            SPWeb web = null;

            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == "Tasks")
                {

                    item = properties.ListItem;
                    web = item.Web;

                    ApprovalTask task = new ApprovalTask(item);

                    if ((task != null) && (task.completed))
                    {
                        // task just completed

                        // need elevated perm here, as this will remove contribute rights

                        SPSecurity.RunWithElevatedPrivileges(delegate
                        {
                            using (SPSite sitePriv = new SPSite(web.Site.ID))
                            using (SPWeb webPriv = sitePriv.OpenWeb(web.ID))
                            {
                                try
                                {
                                    //get associated LRF
                                    SPListItem lrfItemPriv = Elan.SharePoint.LRFApproval.Common.LRF.GetItemById(webPriv, task.lrfNumber);

                                    // after delay 5 seconds (to avoid  workflow contention), 
                                    // remove edit access for owner of completed task
                                    System.Threading.Thread.Sleep(5000);
                                    Elan.SharePoint.LRFApproval.Common.Security.SetItemReadOnly(lrfItemPriv, task.assignedTo);

                                }
                                catch (Exception ex)
                                {
                                    Log.WriteOnlyLogEntry(webPriv, "Task ItemUpdated: Error: ", ex.ToString());
                                }


                            }
                        });
                    }
                }
            }


        }

    }
}
