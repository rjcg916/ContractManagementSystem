using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Properties;
using Microsoft.Office.Server.UserProfiles;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class Request
    {
        public static void SetLRFStatusFullyExecuted(SPWeb web, SPListItem currentItem)
        {
            //If LRF is associated with contract, copy the purchasing data to the new form instance.
            if (currentItem[Settings.Default.FieldAgreementLRFID] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFID].ToString()))
            {
                ////No LRF associatied with current Agreement
                string lrfID = currentItem[Settings.Default.FieldAgreementLRFID].ToString();
                if (lrfID.Contains("#"))
                    lrfID = lrfID.Substring(lrfID.IndexOf("#") + 1);

                if (!string.IsNullOrEmpty(lrfID))
                {
                    int LRFID = Convert.ToInt32(lrfID);
                    SPList lrfList = web.Lists.TryGetList(Settings.Default.ListTitleLRF);
                    SPListItem currentLrf = lrfList.GetItemById(LRFID);

                    if (currentLrf != null)
                    {
                        SPFieldLookupValue fullyExecutedStatus = GetRequestStatusLookupField(web, "Fully Executed Contract");

                        if (fullyExecutedStatus != null)
                            currentLrf[Settings.Default.FieldRequestStatus] = fullyExecutedStatus;

                        currentLrf.Update();

                    }
                }
            }
        }

        public static string GetRequestStatusValue(SPWeb web, string strId)
        {
            SPListItem item = null;
            try
            {
                int id = Int16.Parse(strId);

                SPList list = web.Site.RootWeb.Lists.TryGetList(Settings.Default.ListTitleRequestStatus);

                item = list.GetItemById(id);

                return item.Title;
            }
            catch
            {
                return string.Empty;
            }

        }

        public static  SPFieldLookupValue GetRequestStatusLookupField(SPWeb web, string title)
        {
            SPList list = web.Site.RootWeb.Lists.TryGetList(Settings.Default.ListTitleRequestStatus);

            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>", title);
            SPListItemCollection itemCollection = list.GetItems(query);

            if (itemCollection != null && itemCollection.Count > 0)
            {
                SPListItem field = itemCollection[0];
                SPFieldLookupValue newValue = new SPFieldLookupValue(field.ID, title);

                return newValue;
            }
            return null;
        }
    
    }
}
