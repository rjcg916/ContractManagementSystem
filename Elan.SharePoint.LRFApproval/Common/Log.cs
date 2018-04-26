using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Common;

namespace Elan.SharePoint.LRFApproval.Common
{
    public class Log
    {
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

  
    }
}
