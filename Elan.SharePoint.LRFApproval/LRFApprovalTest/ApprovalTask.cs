using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace LRFApprovalTest
{
    public class ApprovalTask
    {

        public static void ApprovalTask_Exists_Values(SPWeb web)
        {
            SPListItem item = web.Lists["Tasks"].Items[0];
            Elan.SharePoint.LRFApproval.Common.ApprovalTask task = new Elan.SharePoint.LRFApproval.Common.ApprovalTask(item);

            string msg = string.Format("Assigned To {0} %complete {1} lrfnumber {2}", task.assignedTo.Name, task.pctComplete.ToString(), task.lrfNumber);
            Console.WriteLine(msg);
            Console.ReadKey();
        }

    }
}
