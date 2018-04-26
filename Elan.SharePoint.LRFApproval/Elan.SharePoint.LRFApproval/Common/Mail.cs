using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Common;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class Mail
    {

        //public static bool SendLRFAssignedEmail(SPUser legalOwner, SPListItem lrf)
        //{

        //    string recipients = string.Empty;
        //    string subject = string.Empty;
        //    string body = string.Empty;

        //    //build the message
        //    try
        //    {
        //        LRF.CreateAssignedEmailComponents(lrf, legalOwner, ref recipients, ref subject, ref body);
        //    }
        //    catch (Exception ex)
        //    {
        //        Common.Log.WriteOnlyLogEntry(lrf, "CreateLRFAssignedEmail ", ex.ToString());
        //        return false;
        //    }

        //    //send the message
        //    try
        //    {
        //        SPWeb web = lrf.ParentList.ParentWeb;
        //        return SPUtility.SendEmail(web, false, false, recipients, subject, body);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteOnlyLogEntry(lrf, "SendLRFAssignedEmail ", ex.ToString());
        //        return false;
        //    }

        //}


    }
}
