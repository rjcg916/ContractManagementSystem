using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Globalization;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.MetadataModel.Collections;
using Microsoft.SharePoint.BusinessData.Runtime;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Common
{
    public static class CommonEventReceiver
    {

        static SPUser GetUserFromField(SPListItem item, string userField)
        {

            if (item[userField] != null && !string.IsNullOrEmpty(item[userField].ToString()))
            {
                string uname = item[userField].ToString();

                SPFieldUser ufield = item.Fields[userField] as SPFieldUser;
                SPFieldUserValue ufieldValue = ufield.GetFieldValue(uname) as SPFieldUserValue;

                if (ufieldValue != null)
                {
                    SPUser user = ufieldValue.User;

                    if ((user != null) && !(string.IsNullOrEmpty(user.Email)))
                    {
                        return user;
                    }
                }
            }

            return null;
        }


        public static void CreateLRFAssignedEmail(SPListItem lrf, SPUser legalOwner,
                                                  ref string recipients, ref string subject, ref string body)
        {
            recipients = string.Empty;
            subject = String.Empty;
            body = String.Empty;

            //set recipients list
            SPUser submitter = GetUserFromField(lrf, "Created By");
            if ((submitter != null) && (!String.IsNullOrEmpty(submitter.Email)))
                recipients += submitter.Email + ";";

            SPUser requestor = GetUserFromField(lrf, "Requestor");
            if ((requestor != null) && (!String.IsNullOrEmpty(requestor.Email)))
                recipients += requestor.Email + ";";

            //SPUser legalOwner = LRFParticipant(lrf, "Legal Team Contact");
            if ((legalOwner != null) && (!String.IsNullOrEmpty(legalOwner.Email)))
                recipients += legalOwner.Email + ";";

            // find party name for subject line: party name OR website OR first/last
            string party = string.Empty;
            if ((lrf["Other Party Name Text"] != null) && (!String.IsNullOrEmpty(lrf["Other Party Name Text"].ToString())))
                party = lrf["Other Party Name Text"].ToString();
            else if ((lrf["OtherPartyWebsite"] != null) && (!String.IsNullOrEmpty(lrf["OtherPartyWebsite"].ToString())))
            {
                party = lrf["OtherPartyWebsite"].ToString();
            }
            else if ((lrf["OtherPartyLastName"] != null) && (!String.IsNullOrEmpty(lrf["OtherPartyLastName"].ToString())))
            {
                if ((lrf["OtherPartyFirstName"] != null) && (!String.IsNullOrEmpty(lrf["OtherPartyFirstName"].ToString())))
                {
                    party = lrf["OtherPartyFirstName"].ToString() + " " + lrf["OtherPartyLastName"].ToString();
                }
                else
                    party = lrf["OtherPartyLastName"].ToString();
            }
            else
                party = "Unspecified Party";

            string legalOwnerName = string.Empty;
            if (!String.IsNullOrEmpty(legalOwner.Name))
                legalOwnerName = legalOwner.Name;
            else
                legalOwnerName = "Unknown";

            subject = String.Format("{0} with {1} is assigned to {2}", lrf.Name, party, legalOwnerName);

            //create body
            SPWeb web = lrf.ParentList.ParentWeb;
            string link = web.Url + "/_layouts/FormServer.aspx?XmlLocation=/" + lrf.Url + "&Source=" + web.Url + "/Legal%2520Request%2520Forms%2FForms%2FAllItems%2Easpx&DefaultItemOpen=1";

            body = subject + "<br/><br/>";
            if ((lrf["EstimatedAmount"] != null) && (!String.IsNullOrEmpty(lrf["EstimatedAmount"].ToString())))
                body += "Estimated Value of Approval is " + lrf["EstimatedAmount"].ToString() + "<br/><br/>";

            body += String.Format("Click this <a href=\"{0}\">link</a> to review the request. ", link);
           
        }

        public static bool SendLRFAssignedEmail(SPUser legalOwner, SPListItem lrf)
        {

            string recipients = string.Empty;
            string subject = string.Empty;
            string body = string.Empty;

            //build the message
            try
            {
                CreateLRFAssignedEmail(lrf, legalOwner, ref recipients, ref subject, ref body);
            }
            catch (Exception ex) {
                WriteOnlyLogEntry(lrf, "CreateLRFAssignedEmail ", ex.ToString());
                return false;
            }

            //send the message
            try
            {
                SPWeb web = lrf.ParentList.ParentWeb;
                return SPUtility.SendEmail(web, false, false, recipients, subject, body);
            }
            catch (Exception ex)
            {
                WriteOnlyLogEntry(lrf, "SendLRFAssignedEmail ", ex.ToString());
                return false;
            }

        }

        public static void WriteOnlyLogEntry(SPListItem currentItem, string title, string error)
        {

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                SPListItem logItem = currentItem.Web.Site.RootWeb.Lists[Properties.Settings.Default.ListTitleLRFIssuesLog].Items.Add();
                logItem[Properties.Settings.Default.FieldLRFIssueID] = currentItem.ID;
                string itemUrl = currentItem.Web.Url + "/_layouts/FormServer.aspx?XmlLocation=" + currentItem.Web.Url + "/" + currentItem.Url + "&DefaultItemOpen=1";
                logItem[Properties.Settings.Default.FieldLRFIssueUrl] = itemUrl;

                if (title.Length > 254)
                    logItem["Title"] = title.Substring(0, 254);
                else
                    logItem["Title"] = title;

                logItem[Properties.Settings.Default.FieldLRFIssueErrorDetails] = error;

                logItem.Update();

            });
        }

        public static void WriteOnlyLogEntry(SPWeb web, string title, string error)
        {
            //            SPWeb web = SPContext.Current.Web;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                SPListItem logItem = web.Site.RootWeb.Lists[Properties.Settings.Default.ListTitleLRFIssuesLog].Items.Add();
                logItem[Properties.Settings.Default.FieldLRFIssueID] = 0;
                //string itemUrl = currentItem.Web.Url + "/_layouts/FormServer.aspx?XmlLocation=" + currentItem.Web.Url + "/" + currentItem.Url + "&DefaultItemOpen=1";
                logItem[Properties.Settings.Default.FieldLRFIssueUrl] = string.Empty;

                if (title.Length > 254)
                    logItem["Title"] = title.Substring(0, 254);
                else
                    logItem["Title"] = title;

                logItem[Properties.Settings.Default.FieldLRFIssueErrorDetails] = error;

                logItem.Update();

            });
        }

        public static void WriteLogEntry(SPListItem currentItem, string title, string error)
        {

            SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    SPListItem logItem = currentItem.Web.Site.RootWeb.Lists[Properties.Settings.Default.ListTitleLRFIssuesLog].Items.Add();
                    logItem[Properties.Settings.Default.FieldLRFIssueID] = currentItem.ID;
                    string itemUrl = currentItem.Web.Url + "/_layouts/FormServer.aspx?XmlLocation=" + currentItem.Web.Url + "/" + currentItem.Url + "&DefaultItemOpen=1";
                    logItem[Properties.Settings.Default.FieldLRFIssueUrl] = itemUrl;

                    if (title.Length > 254)
                        logItem["Title"] = title.Substring(0, 254);
                    else
                        logItem["Title"] = title;

                    logItem[Properties.Settings.Default.FieldLRFIssueErrorDetails] = error;

                    logItem.Update();

                    string issues = string.Empty;
                    if (currentItem != null && currentItem[Properties.Settings.Default.FieldItemIssues] != null)
                    {
                        issues = currentItem[Properties.Settings.Default.FieldItemIssues].ToString();
                        issues = issues + " ; " + title;
                        if (issues.Length > 251)
                            issues = issues.Substring(0, 251) + "...";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(issues))
                            issues = "Issues: " + title;
                    }
                    currentItem[Properties.Settings.Default.FieldItemIssues] = issues;
                    currentItem.Update();
                });
        }

        public static int MakeInt(string input)
        {
            int outInt = 0;
            string strOut = input;
            if (input.Length > 0)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (!char.IsNumber(c))
                        strOut = strOut.Replace(c.ToString(), "");
                }
            }

            if (!string.IsNullOrEmpty(strOut))
                outInt = Convert.ToInt32(strOut.Trim());

            return outInt;
        }


    }
}
