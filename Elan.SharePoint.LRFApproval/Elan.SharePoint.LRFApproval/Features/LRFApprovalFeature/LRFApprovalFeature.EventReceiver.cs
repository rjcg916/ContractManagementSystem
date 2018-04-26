using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Features.LRFApprovalFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("ae9d3f05-5868-4614-99dd-2ec9076c1aea")]
    public class LRFApprovalFeatureEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite site = properties.Feature.Parent as SPSite;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.Lists.TryGetList(Properties.Settings.Default.ListTitleLRFIssuesLog);
                    if (list == null)
                    {
                        Guid id = web.Lists.Add(Settings.Default.ListTitleLRFIssuesLog, "List to hold LRF Process Issues", SPListTemplateType.GenericList);
                        list = web.Lists[id];

                        list.Fields.Add(Settings.Default.FieldLRFIssueID, SPFieldType.Integer, false);
                        list.Fields.Add(Settings.Default.FieldLRFIssueUrl, SPFieldType.URL, false);
                        list.Fields.Add(Settings.Default.FieldLRFIssueErrorDetails, SPFieldType.Note, false);
                        SPView defaultView = list.DefaultView;
                        defaultView.ViewFields.Add(Settings.Default.FieldLRFIssueID);
                        defaultView.ViewFields.Add(Settings.Default.FieldLRFIssueUrl);
                        defaultView.ViewFields.Add(Settings.Default.FieldLRFIssueErrorDetails);

                        defaultView.Update();
                        list.Update();
                        list.ParentWeb.Update();
                    }
                }

            });
        }

        //private void EnsureCustomFields(SPList list)
        //{
        //    bool updateList = false;
        //    SPView view = list.DefaultView;

        //    if (!list.Fields.ContainsField(Settings.Default.FieldLRFFormApprovers))
        //    {

        //        list.Fields.Add(Settings.Default.FieldLRFFormApprovers.Replace("_x0020_", " "), SPFieldType.User, false);
        //        SPFieldUser f = (SPFieldUser)list.Fields[Settings.Default.FieldLRFFormApprovers];

        //        f.AllowMultipleValues = true;
        //        f.Update();

        //        view.ViewFields.Add(Settings.Default.FieldLRFFormApprovers);
        //        updateList = true;
        //    }


        //    if (!list.Fields.ContainsField(Settings.Default.FieldLRFDeptApprovers))
        //    {
        //        list.Fields.Add(Settings.Default.FieldLRFDeptApprovers, SPFieldType.User, false);
        //        SPFieldUser f = (SPFieldUser)list.Fields[Settings.Default.FieldLRFDeptApprovers];

        //        f.AllowMultipleValues = true;
        //        f.Update();

        //        view.ViewFields.Add(Settings.Default.FieldLRFDeptApprovers);
        //        updateList = true;
        //    }

        //    if (!list.Fields.ContainsField(Settings.Default.FieldLRFFinancialApprovers))
        //    {

        //        list.Fields.Add(Settings.Default.FieldLRFFinancialApprovers, SPFieldType.User, false);
        //        SPFieldUser f = (SPFieldUser)list.Fields[Settings.Default.FieldLRFFinancialApprovers];

        //        f.AllowMultipleValues = true;
        //        f.Update();

        //        view.ViewFields.Add(Settings.Default.FieldLRFFinancialApprovers);
        //        updateList = true;
        //    }

        //    if (!list.Fields.ContainsField(Settings.Default.FieldLastDeptApprover))
        //    {

        //        list.Fields.Add(Settings.Default.FieldLastDeptApprover.Replace("_x0020_", " "), SPFieldType.User, false);
        //        SPFieldUser f = (SPFieldUser)list.Fields[Settings.Default.FieldLastDeptApprover.Replace("_x0020_", " ")];

        //        f.AllowMultipleValues = false;
        //        f.Update();

        //        view.ViewFields.Add(f);
        //        updateList = true;
        //    }

        //    if (!list.Fields.ContainsField(Settings.Default.FieldLastFinancialApprover))
        //    {
        //        list.Fields.Add(Settings.Default.FieldLastFinancialApprover.Replace("_x0020_", " "), SPFieldType.User, false);
        //        SPFieldUser f = (SPFieldUser)list.Fields[Settings.Default.FieldLastFinancialApprover.Replace("_x0020_", " ")];

        //        f.AllowMultipleValues = false;
        //        f.Update();

        //        view.ViewFields.Add(f);
        //        updateList = true;
        //    }

        //    if (!list.Fields.ContainsField(Settings.Default.FieldItemIssues))
        //    {
        //        list.Fields.Add(Settings.Default.FieldItemIssues, SPFieldType.Text, false);
        //        view.ViewFields.Add(Settings.Default.FieldItemIssues);
        //        updateList = true;
        //    }

        //    if (updateList)
        //    {
        //        try
        //        {
        //            view.Update();
        //            list.Update();
        //            list.ParentWeb.Update();
        //        }
        //        catch (Exception ex)
        //        {
        //            CommonEventReceiver.WriteLogEntry(currentItem, "Error: Ensure Custom Fields: updateList ", ex.ToString() + " " + ex.StackTrace);
        //        }
        //    }
        //}

        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
