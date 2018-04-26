using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class ApprovalTask
    {
        public SPUser assignedTo = null;
        public double pctComplete = 0;
        public int lrfNumber = 0;
        public bool completed = false;

        private SPListItem _item;

        public ApprovalTask(SPListItem item)
        {
            _item = item;

            if (_item == null) return;
 
            assignedTo = User.GetUserFromField(_item, "Assigned To");

            pctComplete = (double)_item["% Complete"];
            completed = (pctComplete == 1.0); 

            try
            {
                lrfNumber = Int16.Parse(_item["LRFNumber"].ToString());
            }
            catch { }
            //if can't convert, default
        }
    }
}
