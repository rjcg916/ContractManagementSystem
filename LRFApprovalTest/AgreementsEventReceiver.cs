using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval;
using Elan.SharePoint.LRFApproval.Properties;

namespace LRFApprovalTest
{
    public class AgreementsEventReceiver
    {

        public static void  AgreementsLegacy_Requestor_False(SPWeb web)
        {

            SPListItem currentItem = web.Lists["Agreements"].Items.GetItemById(284);
   
            SPUser requestor = null;
            string requestorusername = string.Empty;

            SPUser creator = null;
            string creatorusername = string.Empty;

            string costCenterNumber = string.Empty;

            string FieldAgreementsLRFRequestor = "LRF Requestor";


            if (currentItem[FieldAgreementsLRFRequestor] != null && !string.IsNullOrEmpty(currentItem[FieldAgreementsLRFRequestor].ToString()))
            {
                requestorusername = currentItem[FieldAgreementsLRFRequestor].ToString();

                SPFieldUser field = currentItem.Fields[FieldAgreementsLRFRequestor] as SPFieldUser;
                SPFieldUserValue fieldValue = field.GetFieldValue(currentItem[FieldAgreementsLRFRequestor].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    requestor = fieldValue.User;
                    requestorusername = requestor.LoginName;
                }

                if (requestor != null)
                {
                    int band;
                    int AuthAmount;
                    SPUser manager;
                    Elan.SharePoint.LRFApproval.Common.User.GetUserAttributes(web, requestor, out band, out AuthAmount, out manager, out costCenterNumber);
//                    if (string.IsNullOrEmpty(costCenterNumber))
//                        WriteLogEntry(currentItem, "Error: Requestor does not have a defined cost center", "Requestor does not have a defined cost center: " + requestorusername);
                }

                //string FieldAgreementsLRFSubmitter = "LRF Submitter";
                string FieldAgreementsLRFSubmitter = "LRF_x0020_Submitter";
                //fetch LRF Submitter to use for AssignItemSecurity
                creatorusername = currentItem[FieldAgreementsLRFSubmitter].ToString();

                SPFieldUser cfield = currentItem.Fields.GetField(FieldAgreementsLRFSubmitter) as SPFieldUser;
                SPFieldUserValue cfieldValue = cfield.GetFieldValue(creatorusername) as SPFieldUserValue;
                if (cfieldValue != null)
                {
                    creator = cfieldValue.User;
                    creatorusername = creator.LoginName;
                }

            }

        }
    
    }
}
