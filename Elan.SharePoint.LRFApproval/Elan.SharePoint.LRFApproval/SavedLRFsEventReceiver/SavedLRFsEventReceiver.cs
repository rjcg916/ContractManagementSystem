using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;
using System.Collections.Generic;

namespace Elan.SharePoint.LRFApproval.SavedLRFsEventReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class SavedLRFsEventReceiver : SPItemEventReceiver
    {
        SPListItem currentItem;
        public SPListItem CurrentItem
        {
            get { return currentItem; }
            set { currentItem = value; }
        }

        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
        }

        /// <summary>
        /// An item is updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);

            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == Settings.Default.ListTitleSavedLRFs)
                {
                    if (properties.BeforeProperties != null && properties.AfterProperties != null && properties.AfterProperties[Settings.Default.FieldLRRequestor] != null)
                    {
                        if (properties.BeforeProperties[Settings.Default.FieldLRRequestor] != properties.AfterProperties[Settings.Default.FieldLRRequestor])
                            ProcessItem(properties);
                    }
                }
            }
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);

            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == Settings.Default.ListTitleSavedLRFs)
                {
                    ProcessItem(properties);
                }
            }
        }

        #region Private Methods

        private void ProcessItem(SPItemEventProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPWeb web = properties.OpenWeb())
                {
                    try
                    {
                        this.EventFiringEnabled = false;
                        CurrentItem = properties.ListItem;
                        EnsureCustomFields(properties.List);
                        SPUser requestor = null;

                        if (CurrentItem[Settings.Default.FieldLRRequestor] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRRequestor].ToString()))
                        {
                            string requestorusername = CurrentItem[Settings.Default.FieldLRRequestor].ToString();
                            requestor = web.EnsureUser(requestorusername);
                        }

                        //if (requestor == null)
                        //{
                        //    CommonEventReceiver.WriteLogEntry(currentItem, "Invalid or Missing Required field: Requestor", "Missing Required field: Requestor.");
                        //    return;
                        //}

                        AssignItemSecurity(web, requestor);

                        CurrentItem.Update();
                    }
                    catch (Exception ex)
                    {
                        if (CurrentItem != null)
                            CommonEventReceiver.WriteLogEntry(CurrentItem, "Fatal Error:", ex.ToString() + " " + ex.StackTrace);

                        properties.ErrorMessage = "You cannot save this list item at this time; " + ex.ToString();
                        properties.Cancel = true;
                    }
                    finally { this.EventFiringEnabled = true; }
                }
            });
        }

        private void AssignItemSecurity(SPWeb web, SPUser requestor)
        {
            try
            {
                //Check for permission inheritance, and break if necessary  
                if (!CurrentItem.HasUniqueRoleAssignments)
                {
                    CurrentItem.BreakRoleInheritance(false); //pass true to copy role assignments from parent, false to start from scratch  
                }

                //remove all the inherited roles 
                for (int i = CurrentItem.RoleAssignments.Count - 1; i >= 0; --i)
                {
                    CurrentItem.RoleAssignments.Remove(i);
                }

                SPRoleDefinition RoleDefinitionContributor = web.RoleDefinitions.GetByType(SPRoleType.Contributor);
                SPRoleDefinition RoleDefinitionReadOnly = web.RoleDefinitions.GetByType(SPRoleType.Reader);
                SPRoleDefinition RoleDefinitionAdmin = web.RoleDefinitions.GetByType(SPRoleType.Administrator);

                //Creator 
                CommonEventReceiver.AssignUserPermission(web, ref currentItem, web.CurrentUser.LoginName, RoleDefinitionContributor);

                //Requestor
                if (requestor != null)
                    CommonEventReceiver.AssignUserPermission(web, ref currentItem, requestor.LoginName, RoleDefinitionContributor);

                //LRF Super users --> Admin
                CommonEventReceiver.AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupLRFSuperUsers, RoleDefinitionAdmin);

            }
            catch (Exception ex)
            {
                CommonEventReceiver.WriteLogEntry(currentItem, "Error: Assigning permissions", ex.ToString() + " " + ex.StackTrace);
                //throw new Exception("Error Assigning permissions", ex);
            }

            //item.Update();
        }

        private void EnsureCustomFields(SPList list)
        {
            bool updateList = false;
            SPView view = list.DefaultView;

            if (!list.Fields.ContainsField(Settings.Default.FieldItemIssues))
            {
                list.Fields.Add(Settings.Default.FieldItemIssues, SPFieldType.Text, false);
                view.ViewFields.Add(Settings.Default.FieldItemIssues);
                updateList = true;
            }

            if (updateList)
            {
                view.Update();
                list.Update();
                list.ParentWeb.Update();
            }
        }

        #endregion
    }
}
