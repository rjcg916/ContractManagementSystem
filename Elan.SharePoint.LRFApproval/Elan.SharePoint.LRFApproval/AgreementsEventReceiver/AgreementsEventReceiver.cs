using System;
using System.Security.Permissions;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Globalization;
using System.Xml.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Collections;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Microsoft.Office.Server.UserProfiles;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;


namespace Elan.SharePoint.LRFApproval.AgreementsEventReceiver
{


    /// <summary>
    /// List Item Events
    /// </summary>
    public class AgreementsEventReceiver : SPItemEventReceiver
    {

        SPListItem currentItem;
        public SPListItem CurrentItem
        {
            get { return currentItem; }
            set { currentItem = value; }
        }
        /// <summary>
        /// An item was Added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
        }


        /// <summary>
        /// An item is being checked in.
        /// </summary>
        public override void ItemCheckedIn(SPItemEventProperties properties)
        {

            base.ItemCheckedIn(properties);

            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == Settings.Default.ListTitleAgreements)
                {
                    this.EventFiringEnabled = false;

                    CurrentItem = properties.ListItem;
                    bool isTransferToSRM = false;
                    bool isAgreementFormGenerated = false;
                    int totalValue = 0;
                    string actualValue = "0";
                    bool lrfExists = false;

                    try
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {

                            using (SPSite site = new SPSite(properties.Web.Site.ID, properties.OriginatingUserToken))
                            {

                                using (SPWeb web = site.OpenWeb(properties.Web.ID))
                                {
                                    try
                                    {
                                        web.AllowUnsafeUpdates = true;

                                        Request.SetLRFStatusFullyExecuted(web, currentItem);

                                        // see if purchasing form previously generated
                                        if (currentItem[Settings.Default.FieldAgreementPurchasingFormGenerated] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementPurchasingFormGenerated].ToString()))
                                        {
                                            if (currentItem[Settings.Default.FieldAgreementPurchasingFormGenerated].ToString() == "1")
                                                isAgreementFormGenerated = true;
                                        }

                                        // find Contract Total $ Not to Exceed 
                                        if (currentItem[Settings.Default.FieldAgreementActualValue] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementActualValue].ToString()))
                                        {
                                            actualValue = currentItem[Settings.Default.FieldAgreementActualValue].ToString();
                                            totalValue = Util.MakeInt(actualValue);
                                        }
                                        
                                        ///find TransferToSRM value
                                        if (currentItem[Settings.Default.FieldAgreementTransferToSRM] != null && 
                                            !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementTransferToSRM].ToString()))
                                        {
                                            string transferToSRM = currentItem[Settings.Default.FieldAgreementTransferToSRM].ToString();
                                            isTransferToSRM = Convert.ToBoolean(transferToSRM);
                                        }

                                        //see if LRF exists
                                        if (currentItem[Settings.Default.FieldAgreementLRFID] != null &&
                                            !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFID].ToString()))
                                        {
                                            SPListItem item = null;
                                            item = LRF.GetItemById(web, currentItem[Settings.Default.FieldAgreementLRFID].ToString());
                                            if (item != null)
                                               lrfExists = true;
                                        }

                                        //determine if purchasing form should be generated
                                        //Rules: lrf exists, not already generated, value >0, TransferToSRM requested
                                        if (lrfExists && isTransferToSRM && totalValue > 0 && !isAgreementFormGenerated)
                                        {

                                            //	populate  Purchasing Form object values
                                            PurchasingForm newPurchasingForm = PopulatePurchasingForm(web, actualValue, properties);

                                            newPurchasingForm.FileName = "ElanPRF-" + currentItem.ID.ToString() + "-" + newPurchasingForm.ContractNumber;

                                            //  create a new instance of a Purchasing Form
                                            CreatePurchasingForm(web, newPurchasingForm);


                                            currentItem.Web.AllowUnsafeUpdates = true;
                                            if (currentItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                                                currentItem.File.CheckOut();

                                            currentItem[Settings.Default.FieldAgreementPurchasingFormGenerated] = "1";
                                            currentItem.UpdateOverwriteVersion();

                                            if (currentItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                                                currentItem.File.CheckIn("", SPCheckinType.OverwriteCheckIn);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteLogEntry(CurrentItem, "Fatal Exception Agreement/Purchasing Error: " + ex.Message, ex.StackTrace + " - - - " + ex.ToString());
                                        throw ex;
                                    }
                                    finally
                                    {
                                        this.EventFiringEnabled = true;
                                        web.AllowUnsafeUpdates = false;
                                    }

                                }
                            }
                        });

                        ProcessItemPermissions(properties, false);

                    }
                    catch (Exception ex)
                    {
                        if (currentItem != null)
                            WriteLogEntry(currentItem, "Fatal Error:", ex.StackTrace + " " + ex.ToString());

                        properties.ErrorMessage = "You cannot save this list item at this time; " + ex.ToString();
                        properties.Cancel = true;

                    }

                }
            }
        }

        #region Private Methods

        private void ProcessItemPermissions(SPItemEventProperties properties, bool isNew)
        {
            this.EventFiringEnabled = false;

            //          bool wasNotCheckedOut = false;
            if (currentItem.File.CheckOutType != SPFile.SPCheckOutType.None)
            {
                currentItem.File.CheckIn(String.Empty);
                //               wasNotCheckedOut = true;
            }
            using (SPWeb web = properties.OpenWeb())
            {
                int itemID = properties.ListItem.ID;
                web.AllowUnsafeUpdates = true;
                try
                {
                    SPUser requestor = null;
//                    string requestorusername = string.Empty;

                    SPUser creator = null;
//                    string creatorusername = string.Empty;

                    string costCenterNumber = string.Empty;

                    //CurrentItem = web.GetListItem(properties.ListItem.Url);

                    CurrentItem = properties.ListItem;

                    if (currentItem[Properties.Settings.Default.FieldItemIssues] != null && !string.IsNullOrEmpty(currentItem[Properties.Settings.Default.FieldItemIssues].ToString()))
                        currentItem[Properties.Settings.Default.FieldItemIssues] = "";

                    if (currentItem[Settings.Default.FieldAgreementLRFRequestor] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFRequestor].ToString()))
                    {
                        string requestorusername = CurrentItem[Settings.Default.FieldAgreementLRFRequestor].ToString();

                        SPFieldUser field = currentItem.Fields.GetField(Settings.Default.FieldAgreementLRFRequestor) as SPFieldUser;
                        SPFieldUserValue fieldValue = field.GetFieldValue(requestorusername) as SPFieldUserValue;
                        if (fieldValue != null)
                        {
                            requestor = fieldValue.User;
//                            requestorusername = requestor.LoginName;
                        }

                        if (requestor != null)
                        {
                            int band;
                            int AuthAmount;
                            SPUser manager;
                            User.GetUserAttributes(web, requestor, out band, out AuthAmount, out manager, out costCenterNumber);
                            if (string.IsNullOrEmpty(costCenterNumber))
                                WriteLogEntry(currentItem, "Error: Requestor does not have a defined cost center", "Requestor does not have a defined cost center: " + requestorusername);
                        }
                    }


                    if (currentItem[Settings.Default.FieldAgreementLRFSubmitter] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFSubmitter].ToString()))
                    {
                        //fetch LRF Submitter to use for AssignItemSecurity
                        string creatorusername = CurrentItem[Settings.Default.FieldAgreementLRFSubmitter].ToString();

                        SPFieldUser cfield = currentItem.Fields.GetField(Settings.Default.FieldAgreementLRFSubmitter) as SPFieldUser;
                        SPFieldUserValue cfieldValue = cfield.GetFieldValue(creatorusername) as SPFieldUserValue;
                        if (cfieldValue != null)
                        {
                            creator = cfieldValue.User;
 //                           creatorusername = creator.LoginName;
                        }

                    }

                    //rjg: added creator parameter
                    AssignItemSecurity(creator, requestor, costCenterNumber, properties, isNew);

                    CurrentItem = properties.ListItem;
                }
                catch (Exception ex)
                {
                    if (currentItem != null)
                        WriteLogEntry(currentItem, "Process Item Permissons: Error: ", ex.StackTrace + " " + ex.ToString());
                }
                finally
                {
                    this.EventFiringEnabled = true;
                    web.AllowUnsafeUpdates = false;
                }
            }

        }

        private void AssignItemSecurity(SPUser requestor, string costCenterNumber, SPItemEventProperties properties, bool isNew)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var site = new SPSite(properties.SiteId))
                using (var web = site.OpenWeb(properties.RelativeWebUrl))
                {
                    System.Threading.Thread.Sleep(1000);
                    var list = web.Lists[properties.ListId];
                    CurrentItem = list.Items[properties.ListItem.UniqueId];

                    try
                    {
                        //Check for permission inheritance, and break if necessary  
                        if (!CurrentItem.HasUniqueRoleAssignments)
                            CurrentItem.BreakRoleInheritance(false); //pass true to copy role assignments from parent, false to start from scratch  

                        SPRoleDefinition RoleDefinitionContributor = web.RoleDefinitions.GetByType(SPRoleType.Contributor);
                        SPRoleDefinition RoleDefinitionReadOnly = web.RoleDefinitions.GetByType(SPRoleType.Reader);
                        SPRoleDefinition RoleDefinitionAdmin = web.RoleDefinitions.GetByType(SPRoleType.Administrator);
                        SPUser author = currentItem.File.Author;
                        if (isNew)
                        {
                            if (!string.IsNullOrEmpty(author.LoginName))
                                AssignUserPermission(web, ref currentItem, author.LoginName, RoleDefinitionContributor);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(author.LoginName))
                                AssignUserPermission(web, ref currentItem, author.LoginName, RoleDefinitionReadOnly);
                        }

                        //Creator 
                        if (author.LoginName != properties.Web.CurrentUser.LoginName)
                            AssignUserPermission(web, ref currentItem, properties.Web.CurrentUser.LoginName, RoleDefinitionReadOnly);

                        //Requestor
                        if (requestor != null)
                            AssignUserPermission(web, ref currentItem, requestor.LoginName, RoleDefinitionReadOnly);

                        //Finance Group --> Read
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupFinanceTeam, RoleDefinitionReadOnly);
                        //Legal Team group --> Contribute
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupLegalTeam, RoleDefinitionContributor);
                        //LRF Super users --> Admin
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupLRFSuperUsers, RoleDefinitionAdmin);
                        //Purchasing Team Group --> Read
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupPurchasingTeam, RoleDefinitionReadOnly);

                        //Cost center Super users --> Read
                        if (!string.IsNullOrEmpty(costCenterNumber))
                        {
                            string groupCostCenterSuperUser = Properties.Settings.Default.GroupCostCenterSuperUserPrefix + costCenterNumber;
                            AssignGroupPermission(web, ref currentItem, groupCostCenterSuperUser, RoleDefinitionReadOnly);
                        }
                        else
                        {
                            string requestorUserName = string.Empty;
                            if (requestor != null)
                                requestorUserName = requestor.LoginName;

                            WriteLogEntry(currentItem, "Warning: Could not Assign Permissions to 'Cost Center Super users' group", "Could not retreive Cost center number for current user: " + requestorUserName);
                        }

                        if (currentItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                            currentItem.File.CheckOut();

                        currentItem.Update();

                        if (currentItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                            currentItem.File.CheckIn("", SPCheckinType.OverwriteCheckIn);
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry(currentItem, "AgreementEventReciver: AssignItemSecurity ", ex.ToString() + " " + ex.StackTrace);
                    }
                    finally
                    {
                        this.EventFiringEnabled = true;
                    }
                }
            });


        }


        private void AssignItemSecurity(SPUser creator, SPUser requestor, string costCenterNumber, SPItemEventProperties properties, bool isNew)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var site = new SPSite(properties.SiteId))
                using (var web = site.OpenWeb(properties.RelativeWebUrl))
                {
                    System.Threading.Thread.Sleep(1000);
                    var list = web.Lists[properties.ListId];
                    CurrentItem = list.Items[properties.ListItem.UniqueId];

                    try
                    {
                        //Check for permission inheritance, and break if necessary  
                        if (!CurrentItem.HasUniqueRoleAssignments)
                            CurrentItem.BreakRoleInheritance(false); //pass true to copy role assignments from parent, false to start from scratch  

                        SPRoleDefinition RoleDefinitionContributor = web.RoleDefinitions.GetByType(SPRoleType.Contributor);
                        SPRoleDefinition RoleDefinitionReadOnly = web.RoleDefinitions.GetByType(SPRoleType.Reader);
                        SPRoleDefinition RoleDefinitionAdmin = web.RoleDefinitions.GetByType(SPRoleType.Administrator);
                        SPUser author = currentItem.File.Author;
                        if (isNew)
                        {
                            if (!string.IsNullOrEmpty(author.LoginName))
                                AssignUserPermission(web, ref currentItem, author.LoginName, RoleDefinitionContributor);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(author.LoginName))
                                AssignUserPermission(web, ref currentItem, author.LoginName, RoleDefinitionReadOnly);
                        }

                        //Creator 
                        if (creator != null)
                            AssignUserPermission(web, ref currentItem, creator.LoginName, RoleDefinitionReadOnly);

                        //Requestor
                        if (requestor != null)
                            AssignUserPermission(web, ref currentItem, requestor.LoginName, RoleDefinitionReadOnly);

                        //Finance Group --> Read
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupFinanceTeam, RoleDefinitionReadOnly);
                        //Legal Team group --> Contribute
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupLegalTeam, RoleDefinitionContributor);
                        //LRF Super users --> Admin
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupLRFSuperUsers, RoleDefinitionAdmin);
                        //Purchasing Team Group --> Read
                        AssignGroupPermission(web, ref currentItem, Properties.Settings.Default.GroupPurchasingTeam, RoleDefinitionReadOnly);

                        //Cost center Super users --> Read
                        if (!string.IsNullOrEmpty(costCenterNumber))
                        {
                            string groupCostCenterSuperUser = Properties.Settings.Default.GroupCostCenterSuperUserPrefix + costCenterNumber;
                            AssignGroupPermission(web, ref currentItem, groupCostCenterSuperUser, RoleDefinitionReadOnly);
                        }
                        else
                        {
                            string requestorUserName = string.Empty;
                            if (requestor != null)
                                requestorUserName = requestor.LoginName;

                            WriteLogEntry(currentItem, "Warning: Could not Assign Permissions to 'Cost Center Super users' group", "Could not retreive Cost center number for current user: " + requestorUserName);
                        }

                        if (currentItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                            currentItem.File.CheckOut();

                        currentItem.Update();

                        if (currentItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                            currentItem.File.CheckIn("", SPCheckinType.OverwriteCheckIn);
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry(currentItem, "AgreementEventReciver: AssignItemPermissions: Error:", ex.ToString() + " " + ex.StackTrace);
                    }
                    finally
                    {
                        this.EventFiringEnabled = true;
                    }
                }
            });


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

            if (!list.Fields.ContainsField(Settings.Default.FieldAgreementPurchasingFormGenerated))
            {
                list.Fields.Add(Settings.Default.FieldAgreementPurchasingFormGenerated, SPFieldType.Boolean, true);
                SPFieldBoolean f = (SPFieldBoolean)list.Fields[Settings.Default.FieldAgreementPurchasingFormGenerated];
                f.DefaultValue = "0";
                f.Update();
                f.ReadOnlyField = true;
                //view.ViewFields.Add(f);
                updateList = true;

            }

            if (updateList)
            {
                view.Update();
                list.Update();
                list.ParentWeb.Update();
            }
        }

        private void WriteLogEntry(SPListItem currentItem, string title, string error)
        {
            SPListItem logItem = currentItem.Web.Site.RootWeb.Lists[Properties.Settings.Default.ListTitleLRFIssuesLog].Items.Add();
            logItem[Properties.Settings.Default.FieldLRFIssueID] = currentItem.ID;
            string itemUrl = currentItem.Web.Url + "/" + currentItem.Url;
            logItem[Properties.Settings.Default.FieldLRFIssueUrl] = itemUrl;

            if (title.Length > 254)
                logItem["Title"] = title.Substring(0, 254);
            else
                logItem["Title"] = title;

            logItem[Properties.Settings.Default.FieldLRFIssueErrorDetails] = error;

            logItem.Update();
        }


        public void AssignUserPermission(SPWeb web, ref SPListItem currentItem, string userName, SPRoleDefinition roleDefinition)
        {
            try
            {
                SPUser user = web.EnsureUser(userName);
                SPRoleAssignment RoleAssignment = new SPRoleAssignment((SPPrincipal)user);
                RoleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                currentItem.RoleAssignments.Add(RoleAssignment);
            }
            catch (Exception ex)
            {

                WriteLogEntry(currentItem, "Problem Assigning User Permission: user role: " + userName + roleDefinition.Name, ex.ToString());

            }
        }


        public void AssignGroupPermission(SPWeb web, ref SPListItem currentItem, string groupName, SPRoleDefinition roleDefinition)
        {

            try
            {
                SPGroup spGroup = web.SiteGroups[groupName];
                SPRoleAssignment roleAssignment = new SPRoleAssignment(spGroup);
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                //return roleAssignment;
                currentItem.RoleAssignments.Add(roleAssignment);

            }
            catch (Exception ex)
            {
                WriteLogEntry(currentItem, "Security Group does Not Exist: " + groupName + ". ", "Exception: " + ex.ToString());
            }
        }



        /// <summary>
        ///Programmatically create a new instance of a Purchasing Form
        /// </summary>
        /// <param name="web"></param>
        /// <param name="totalAmount"></param>
        /// <returns></returns>


 

        private PurchasingForm PopulatePurchasingForm(SPWeb web, string totalAmount, SPItemEventProperties properties)
        {
            string requestor = string.Empty;
            int LRFID;
            string lrfID = string.Empty;
            SPListItem currentLrf = null;
            SPList lrfList = web.Lists.TryGetList(Settings.Default.ListTitleLRF);

            PurchasingForm newPurchasingForm = new PurchasingForm();
            newPurchasingForm.EffectiveDateSpecified = false;
            newPurchasingForm.ExpirationDateSpecified = false;
            newPurchasingForm.isLargerThan0Specified = false;
            newPurchasingForm.TotalAmountSpecified = false;
            newPurchasingForm.Currency = Settings.Default.DefaultCurrency;
            newPurchasingForm.IsSubmitted = false;
            newPurchasingForm.IsSubmittedSpecified = true;
            //Transfer to SRM will always be set to true from agreements list , Otherwise no purchasing form will be created
            newPurchasingForm.TransferToSRM = true;
            newPurchasingForm.TransferToSRMSpecified = true;

            //Total amount from agreement library Item
            if (string.IsNullOrEmpty(totalAmount))
            {
                newPurchasingForm.TotalAmount = 0;
                newPurchasingForm.TotalAmountSpecified = false;
                newPurchasingForm.isLargerThan0 = null;
                newPurchasingForm.isLargerThan0Specified = false;
            }
            else
            {
                Double ta;
                try
                {
                    totalAmount.Replace(",", "");
                    ta = Convert.ToDouble(totalAmount);
                }
                catch (Exception)
                {
                    ta = 0.0;
                }
                //int ta = CommonEventReceiver.MakeInt(totalAmount);
                newPurchasingForm.TotalAmount = ta;
                newPurchasingForm.TotalAmountSpecified = true;

                newPurchasingForm.isLargerThan0 = (ta > 0) ? true : false;
                newPurchasingForm.isLargerThan0Specified = true;
            }

            try
            {

                //ContentType Name
                if (currentItem.ContentType != null && !string.IsNullOrEmpty(currentItem.ContentType.Name))
                    newPurchasingForm.ContentTypeName = currentItem.ContentType.Name;

                //Agreemet Url
                if (currentItem.File != null && currentItem.File.Exists)
                    newPurchasingForm.AgrrementLink = web.Url + "/" + currentItem.File.Url;

                //Contract Number
                if (currentItem[Settings.Default.FieldAgreementContractNumber] != null)
                {
                    newPurchasingForm.ContractNumber = currentItem[Settings.Default.FieldAgreementContractNumber].ToString();
                }

                //Expiration Date
                if (currentItem[Settings.Default.FieldAgreementExpirationDateRevised] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementExpirationDateRevised].ToString()))
                {
                    string expirationDate = currentItem[Settings.Default.FieldAgreementExpirationDateRevised].ToString();
                    newPurchasingForm.ExpirationDate = DateTime.Parse(expirationDate);
                    newPurchasingForm.ExpirationDateSpecified = true;
                }

                //Effective Date
                if (currentItem[Settings.Default.FieldAgreementEffectiveDate] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementEffectiveDate].ToString()))
                {
                    string effectiveDate = currentItem[Settings.Default.FieldAgreementEffectiveDate].ToString();
                    newPurchasingForm.EffectiveDate = DateTime.Parse(effectiveDate);
                    newPurchasingForm.EffectiveDateSpecified = true;
                }



                //LRF Creator
                if (currentItem[Settings.Default.FieldAgreementLRFSubmitter] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFSubmitter].ToString()))
                {
                    string creatorusername = CurrentItem[Settings.Default.FieldAgreementLRFSubmitter].ToString();

                    SPFieldUser cfield = currentItem.Fields.GetField(Settings.Default.FieldAgreementLRFSubmitter) as SPFieldUser;
                    SPFieldUserValue cfieldValue = cfield.GetFieldValue(creatorusername) as SPFieldUserValue;

                    if (cfieldValue != null)
                    {
                        SPUser lrfCreator = cfieldValue.User;

                        if (!string.IsNullOrEmpty(lrfCreator.LoginName))
                            newPurchasingForm.CurrentUserHidden = lrfCreator.LoginName;

                        if (newPurchasingForm.CurrentUserHidden.IndexOf("#") > -1)
                            newPurchasingForm.CurrentUserHidden = newPurchasingForm.CurrentUserHidden.Substring(newPurchasingForm.CurrentUserHidden.IndexOf("#") + 1);

                        if (newPurchasingForm.CurrentUserHidden.IndexOf("|") > -1)
                            newPurchasingForm.CurrentUserHidden = newPurchasingForm.CurrentUserHidden.Substring(newPurchasingForm.CurrentUserHidden.IndexOf("|") + 1);

                        if (!string.IsNullOrEmpty(lrfCreator.Email))
                            newPurchasingForm.CurrentUserEmailHidden = lrfCreator.Email;

                        //                      System.Diagnostics.Trace.WriteLine("AgreementEventReciver: creator user/email" + newPurchasingForm.CurrentUserHidden + " " + newPurchasingForm.CurrentUserEmailHidden);
                    }

                }
                else
                {
                    //Make current User LRF submitter
                    if (!string.IsNullOrEmpty(properties.Web.CurrentUser.LoginName))
                        newPurchasingForm.CurrentUserHidden = properties.Web.CurrentUser.LoginName;

                    if (newPurchasingForm.CurrentUserHidden.IndexOf("#") > -1)
                        newPurchasingForm.CurrentUserHidden = newPurchasingForm.CurrentUserHidden.Substring(newPurchasingForm.CurrentUserHidden.IndexOf("#") + 1);

                    if (newPurchasingForm.CurrentUserHidden.IndexOf("|") > -1)
                        newPurchasingForm.CurrentUserHidden = newPurchasingForm.CurrentUserHidden.Substring(newPurchasingForm.CurrentUserHidden.IndexOf("|") + 1);


                    if (!string.IsNullOrEmpty(properties.Web.CurrentUser.Email))
                        newPurchasingForm.CurrentUserEmailHidden = properties.Web.CurrentUser.Email;

                }


                //Requestor
                if (currentItem[Settings.Default.FieldAgreementLRFRequestor] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFRequestor].ToString()))
                {
                    string requestorusername = CurrentItem[Settings.Default.FieldAgreementLRFRequestor].ToString();

                    SPFieldUser field = currentItem.Fields.GetField(Settings.Default.FieldAgreementLRFRequestor) as SPFieldUser; 
                    SPFieldUserValue fieldValue = field.GetFieldValue(requestorusername) as SPFieldUserValue;

                    SPUser userRequestor;
                    if (fieldValue != null)
                    {
                        userRequestor = fieldValue.User;
                        requestorusername = userRequestor.LoginName;
                        Person[] requestors = new Person[1];
                        Person newRequestor = new Person();
                        newRequestor.AccountId = userRequestor.LoginName;
                        newRequestor.DisplayName = userRequestor.Name;
                        
                        if (newRequestor.AccountId.IndexOf("#") > -1)
                            newRequestor.AccountId = newRequestor.AccountId.Substring(newRequestor.AccountId.IndexOf("#") + 1);

                        if (newRequestor.AccountId.IndexOf("|") > -1)
                            newRequestor.AccountId = newRequestor.AccountId.Substring(newRequestor.AccountId.IndexOf("|") + 1);

                        if (newRequestor.DisplayName.IndexOf("#") > -1)
                            newRequestor.DisplayName = newRequestor.DisplayName.Substring(newRequestor.DisplayName.IndexOf("#") + 1);

                        if (newRequestor.DisplayName.IndexOf("|") > -1)
                            newRequestor.DisplayName = newRequestor.DisplayName.Substring(newRequestor.DisplayName.IndexOf("|") + 1);


                        requestors[0] = newRequestor;
                        newPurchasingForm.Requestor = requestors;
                        newPurchasingForm.RequestorAccountNameHidden = newRequestor.AccountId;
                        newPurchasingForm.RequestorEmailHidden = userRequestor.Email;
                    }
                    //requestor = web.EnsureUser(requestorusername);
                }
                else
                {

                    if ((CurrentItem[Settings.Default.FieldAgreementBusinessContact] != null)
                         && (!String.IsNullOrEmpty(CurrentItem[Settings.Default.FieldAgreementBusinessContact].ToString())))
                    {
                        //Business contact is requestor
                        string requestorusername = CurrentItem[Settings.Default.FieldAgreementBusinessContact].ToString();

                        SPFieldUser field = currentItem.Fields.GetField(Settings.Default.FieldAgreementBusinessContact) as SPFieldUser;
                        SPFieldUserValue fieldValue = field.GetFieldValue(requestorusername) as SPFieldUserValue;

                        SPUser userRequestor;
                        if (fieldValue != null)
                        {
                            userRequestor = fieldValue.User;
                            requestorusername = userRequestor.LoginName;
                            Person[] requestors = new Person[1];
                            Person newRequestor = new Person();
                            newRequestor.AccountId = userRequestor.LoginName;
                            newRequestor.DisplayName = userRequestor.Name;

                            if (newRequestor.AccountId.IndexOf("#") > -1)
                                newRequestor.AccountId = newRequestor.AccountId.Substring(newRequestor.AccountId.IndexOf("#") + 1);

                            if (newRequestor.AccountId.IndexOf("|") > -1)
                                newRequestor.AccountId = newRequestor.AccountId.Substring(newRequestor.AccountId.IndexOf("|") + 1);

                            if (newRequestor.DisplayName.IndexOf("#") > -1)
                                newRequestor.DisplayName = newRequestor.DisplayName.Substring(newRequestor.DisplayName.IndexOf("#") + 1);

                            if (newRequestor.DisplayName.IndexOf("|") > -1)
                                newRequestor.DisplayName = newRequestor.DisplayName.Substring(newRequestor.DisplayName.IndexOf("|") + 1);


                            requestors[0] = newRequestor;
                            newPurchasingForm.Requestor = requestors;
                            newPurchasingForm.RequestorAccountNameHidden = newRequestor.AccountId;
                            newPurchasingForm.RequestorEmailHidden = userRequestor.Email;
                        }
                    }
                }

                //If LRF is associated with contract, copy the purchasing data to the new form instance.
                if (currentItem[Settings.Default.FieldAgreementLRFID] != null && !string.IsNullOrEmpty(currentItem[Settings.Default.FieldAgreementLRFID].ToString()))
                {
                    ////No LRF associatied with current Agreement
                    lrfID = currentItem[Settings.Default.FieldAgreementLRFID].ToString();
                    if (lrfID.Contains("#"))
                        lrfID = lrfID.Substring(lrfID.IndexOf("#") + 1);

                    if (!string.IsNullOrEmpty(lrfID))
                    {
                        LRFID = Convert.ToInt32(lrfID);
                        currentLrf = lrfList.GetItemById(LRFID);

                        if (currentLrf != null)
                        {
                            if (currentLrf[Settings.Default.FieldLRFLinkFilename] != null && !string.IsNullOrEmpty(currentLrf[Settings.Default.FieldLRFLinkFilename].ToString()))
                                newPurchasingForm.LRFNumber = currentLrf[Settings.Default.FieldLRFLinkFilename].ToString().Replace(".xml", "");

                            //Set the LRF URL if it exists
                            if (currentLrf.File != null && currentLrf.File.Exists)
                            {
                                newPurchasingForm.LRFLink = currentItem.Web.Url + "/_layouts/FormServer.aspx?XmlLocation=" + currentLrf.Web.Url + "/" + currentLrf.Url + "&DefaultItemOpen=1";
                                ReadLRFFormVariables(currentLrf.File, ref newPurchasingForm);
                            }
                        }
                    }
                }
                return newPurchasingForm;
            }
            catch (Exception ex)
            {

                WriteLogEntry(CurrentItem, "Fatal Exception Reading LRF Form  - Agreement/Purchasing Error: ", ex.StackTrace + " - - - " + ex.ToString());
                throw ex;
            }
        }

        private void ReadLRFFormVariables(SPFile lrfFormFile, ref PurchasingForm newPurchasingForm)
        {
            try
            {
                MemoryStream myInStream = new MemoryStream(lrfFormFile.OpenBinary());
                XmlDocument doc = new XmlDocument();
                doc.Load(myInStream);

                XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(doc.NameTable);
                nameSpaceManager.AddNamespace("my", "http://schemas.microsoft.com/office/infopath/2003/myXSD/2011-10-05T00:10:10");
                nameSpaceManager.AddNamespace("pc", "http://schemas.microsoft.com/office/infopath/2007/PartnerControls");

                XmlElement root = doc.DocumentElement;

                //Read Form
                if (root.SelectSingleNode("/my:myFields/my:group15/my:group21/my:CostAssignmentDetail", nameSpaceManager).InnerXml != null)
                {
                    string cADetailReview = root.SelectSingleNode("/my:myFields/my:group15/my:group21/my:CostAssignmentDetail", nameSpaceManager).InnerXml;
                    if (!string.IsNullOrEmpty(cADetailReview))
                    {
                        newPurchasingForm.ReviewCostAssignment = cADetailReview;
                    }
                }

                newPurchasingForm.LRFTitle = root.SelectSingleNode("/my:myFields/my:group11/my:LRFTitle", nameSpaceManager).InnerXml;

                //Cost center purchasing info
                #region Set Cost Center purchasing info
                XmlNodeList selectedNodes = root.SelectNodes("/my:myFields/my:group15/my:group21/my:group22/my:group23", nameSpaceManager);
                CostCenterCharge[] costCenters = new CostCenterCharge[selectedNodes.Count];
                int costCenterIndex = 0;
                foreach (XmlNode selectedNode in selectedNodes)
                {
                    string costCenterHeaderNumber = string.Empty;
                    string amount = string.Empty;
                    string productCategory = string.Empty;
                    string descrip = string.Empty;
                    CostCenterCharge costCenterCharge = new CostCenterCharge();

                    costCenterCharge.CostCenter = selectedNode.SelectSingleNode("my:ItemCostCenter", nameSpaceManager).InnerXml;

                    XmlNodeList CCChargeDetails = selectedNode.SelectNodes("my:group24/my:group25", nameSpaceManager);
                    if (CCChargeDetails.Count > 0)
                    {
                        int detailIndex = 0;
                        CostCenterDetails[] ccDetails = new CostCenterDetails[CCChargeDetails.Count];
                        foreach (XmlNode node in CCChargeDetails)
                        {
                            CostCenterDetails ccdetail = new CostCenterDetails();
                            ccdetail.Amount = node.SelectSingleNode("my:ItemAmount", nameSpaceManager).InnerXml;
                            ccdetail.Description = node.SelectSingleNode("my:ItemDescription", nameSpaceManager).InnerXml;
                            ccdetail.ProductCategory = node.SelectSingleNode("my:ItemProductCategory", nameSpaceManager).InnerXml;
                            ccDetails[detailIndex] = ccdetail;
                            detailIndex = detailIndex + 1;
                        }
                        costCenterCharge.CostCenterEntry = ccDetails;
                    }
                    costCenters[costCenterIndex] = costCenterCharge;
                    costCenterIndex = costCenterIndex + 1;
                }

                newPurchasingForm.group11 = costCenters;

                #endregion
            }
            catch (Exception ex)
            {
                WriteLogEntry(CurrentItem, "Fatal Exception Agreement/Purchasing Error: Reading LRF Form", ex.ToString() + " - - - " + ex.StackTrace);
                throw ex;
            }
        }

        private void CreatePurchasingForm(SPWeb web, PurchasingForm newPurchasingForm)
        {
            string siteUrl = web.Site.Url;
            MemoryStream myOutStream = new MemoryStream();
            string rTemplateLocation = siteUrl + @Settings.Default.FormTemplateUrlPurchasingInfo;

            using (myOutStream)
            {
                ////Create XmlSerializer using myfields type
                XmlSerializer serializer = new XmlSerializer(typeof(PurchasingForm));
                XmlTextWriter writer = new XmlTextWriter(myOutStream, Encoding.UTF8);

                // Serialize infopath data
                serializer.Serialize(writer, newPurchasingForm);
                // Upload form to form site.
                using (SPSite site = new SPSite(siteUrl))
                {
                    // Create web instance.
                    using (SPWeb spweb = site.OpenWeb(web.ID))
                    {
                        spweb.AllowUnsafeUpdates = true;

                        SPFolder purchasingFormsLibraryRootFolder = spweb.Folders[Settings.Default.ListTitlePurchasingRequests];
                        XmlDocument form = CreateFormFromTemplate(web, newPurchasingForm);
                        SPList list = spweb.Lists[Settings.Default.ListTitlePurchasingRequests];

                        var formBytes = Encoding.UTF8.GetBytes(form.OuterXml);

                        SPFile documentFile = list.RootFolder.Files.Add(newPurchasingForm.FileName + ".xml", formBytes, true);
                        documentFile.Update();

                        documentFile.Item.Update();

                        spweb.AllowUnsafeUpdates = false;
                    }
                }
            }
        }

        private void SetNodeValue(ref XmlElement purchasingFormRoot, XmlNamespaceManager nameSpaceManager, PurchasingForm newPurchasingForm, string address, object value)
        {
            if (value == null || string.IsNullOrEmpty(Convert.ToString(value)))
                return;

            try
            {
                if (value != null && purchasingFormRoot.SelectSingleNode(address, nameSpaceManager) != null)
                {

                    XmlNode node = purchasingFormRoot.SelectSingleNode(address, nameSpaceManager);
                    node.InnerXml = value.ToString().Trim();

                    node.Attributes.RemoveNamedItem("nil", "http://www.w3.org/2001/XMLSchema-instance");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private XmlDocument PopulatePurchasingForm(XmlDocument form, XmlNamespaceManager nameSpaceManager, PurchasingForm newPurchasingForm)
        {
            try
            {
                XmlElement purchasingFormRoot = form.DocumentElement;

                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:TotalAmount", newPurchasingForm.TotalAmount.ToString());
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:isLargerThan0", (newPurchasingForm.TotalAmount > 0 ? "true" : "false"));
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:TransferToSRM", newPurchasingForm.TransferToSRM.ToString().ToLower());
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:ReviewCostAssignment", newPurchasingForm.ReviewCostAssignment);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:LRFNumber", newPurchasingForm.LRFNumber);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:LRFTitle", newPurchasingForm.LRFTitle);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:FileNameHidden", newPurchasingForm.FileName);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:ContractNumber", newPurchasingForm.ContractNumber);
                if (newPurchasingForm.EffectiveDate != null)
                    SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:EffectiveDate", DateTime.Parse(newPurchasingForm.EffectiveDate.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                if (newPurchasingForm.ExpirationDate != null)
                    SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:ExpirationDate", DateTime.Parse(newPurchasingForm.ExpirationDate.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                // System.Diagnostics.Trace.WriteLine("AgreementEventReciever: current user" + newPurchasingForm.CurrentUserHidden);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:CurrentUserHidden", newPurchasingForm.CurrentUserHidden);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:CurrentUserEmailHidden", newPurchasingForm.CurrentUserEmailHidden);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:RequestorEmailHidden", newPurchasingForm.RequestorEmailHidden);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:RequestorAccountNameHidden", newPurchasingForm.RequestorAccountNameHidden);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:Currency", newPurchasingForm.Currency);
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:IsSubmitted", "false");
                SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:RequestType", newPurchasingForm.ContentTypeName);


                if (newPurchasingForm.Requestor != null && newPurchasingForm.Requestor.Length > 0)
                {
                    if (newPurchasingForm.Requestor[0].DisplayName != null)
                        SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:Requestor/pc:Person/pc:DisplayName", newPurchasingForm.Requestor[0].DisplayName);
                    if (newPurchasingForm.Requestor[0].AccountId != null)
                        SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:Requestor/pc:Person/pc:AccountId", newPurchasingForm.Requestor[0].AccountId);
                    if (newPurchasingForm.Requestor[0].AccountType != null)
                        SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:Requestor/pc:Person/pc:AccountType", newPurchasingForm.Requestor[0].AccountType);
                }

                if (newPurchasingForm.AgrrementLink != null && !string.IsNullOrEmpty(newPurchasingForm.AgrrementLink))
                {
                    newPurchasingForm.AgrrementLink = "<![CDATA[" + newPurchasingForm.AgrrementLink.Replace(" ", "%20") + "]]>";
                    SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:AgreementLink", newPurchasingForm.AgrrementLink);
                }

                if (newPurchasingForm.LRFLink != null && !string.IsNullOrEmpty(newPurchasingForm.LRFLink))
                {
                    newPurchasingForm.LRFLink = "<![CDATA[" + newPurchasingForm.LRFLink.Replace(" ", "%20") + "]]>";
                    SetNodeValue(ref purchasingFormRoot, nameSpaceManager, newPurchasingForm, "/my:myFields/my:LRFLink", newPurchasingForm.LRFLink);

                    XmlNode cccNode = null;
                    try
                    {
                        cccNode = purchasingFormRoot.SelectSingleNode("/my:myFields/my:group11", nameSpaceManager);
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry(currentItem, "Warning: Error Extracting cost center purchasing info from LRF, invalid xPath: /my:myFields/my:group11", ex.ToString());
                    }

                    if (cccNode != null && !string.IsNullOrEmpty(cccNode.InnerXml))
                    {
                        StringBuilder xml = new StringBuilder();
                        if (newPurchasingForm.group11.Length > 0)
                        {
                            foreach (CostCenterCharge ccc in newPurchasingForm.group11)
                            {
                                xml.Append("<my:CostCenterCharge>");
                                xml.Append("<my:CostCenterEntry>");
                                foreach (CostCenterDetails ccd in ccc.CostCenterEntry)
                                {

                                    xml.Append("<my:CostCenterDetails>");
                                    xml.Append("<my:Amount>" + ccd.Amount.Trim() + @"</my:Amount>");
                                    xml.Append("<my:Description>" + ccd.Description.Trim() + @"</my:Description>");
                                    xml.Append("<my:ProductCategory>" + ccd.ProductCategory + @"</my:ProductCategory>");
                                    xml.Append(@"</my:CostCenterDetails>");
                                }
                                xml.Append(@"</my:CostCenterEntry>");
                                xml.Append("<my:CostCenter>" + ccc.CostCenter + @"</my:CostCenter>");
                                xml.Append(@"</my:CostCenterCharge>");
                            }
                            cccNode.InnerXml = xml.ToString();
                        }
                    }
                }

                return form;
            }
            catch (Exception ex)
            {
                WriteLogEntry(currentItem, "Error Populating purchasing Form", ex.ToString() + " --- " + ex.StackTrace);
                throw ex;
            }
        }

        private XmlDocument CreateFormFromTemplate(SPWeb web, PurchasingForm newPurchasingForm)
        {
            string contentTypeName = Settings.Default.CTPurchasingInfo;
            // get CT
            var ct = from SPContentType wct in web.AvailableContentTypes
                     where wct.Name.Equals(contentTypeName, StringComparison.OrdinalIgnoreCase)
                     select wct;
            if (ct.Count() < 1)
                throw new Exception(string.Format("Content Type {0} not found in Web {1}", contentTypeName, web.Site.Url));
            string docTempl = ct.First().DocumentTemplateUrl;

            SPFile xsnFormTemplate = GetFormTemplateFile(web, docTempl);


            string TemplateXml;
            XmlDocument formManifest;
            var xsnForm = new InfopathFormGrocker(true);
            using (var xsnFormStream = new MemoryStream(xsnFormTemplate.OpenBinary(SPOpenBinaryOptions.SkipVirusScan)))
            {
                // get the template Xml file
                xsnForm.ExtractComponent(xsnFormStream, "template.xml");
                TemplateXml = xsnForm.ComponentContent.DocumentElement.OuterXml;
            }

            using (var xsnFormStream = new MemoryStream(xsnFormTemplate.OpenBinary(SPOpenBinaryOptions.SkipVirusScan)))
            {
                // get the manifest (.XSF) file
                xsnForm.ExtractComponent(xsnFormStream, "manifest.xsf");
                formManifest = xsnForm.ComponentContent;//.DocumentElement;
            }

            // get form metadata values
            var ns = CreateNamespaceManager(formManifest);
            string Name = formManifest.DocumentElement.GetAttribute("name", "");
            string SolutionVersion = formManifest.DocumentElement.GetAttribute("solutionVersion", "");
            string ProductVersion = formManifest.DocumentElement.GetAttribute("productVersion", "");

            // get the form template (.XSN) file url from the SPFile itself
            string HRef = xsnFormTemplate.Item["ows_EncodedAbsUrl"].ToString();

            string FormName;
            // get the (english) form (display)name
            var node = formManifest.SelectSingleNode("//xsf2:solutionPropertiesExtension[@branch='share']/xsf2:share", ns);
            if (node != null && (node is XmlElement))
                FormName = (node as XmlElement).GetAttribute("formName");

            XmlDocument form = CreateFormInstanceDocument(TemplateXml, Name, SolutionVersion, ProductVersion, HRef);

            form = PopulatePurchasingForm(form, ns, newPurchasingForm);
            return form;
        }

        private XmlDocument CreateFormInstanceDocument(string TemplateXml, string Name, string SolutionVersion, string ProductVersion, string HRef)
        {
            var doc = new XmlDocument();
            doc.Load(new XmlTextReader(new StringReader(TemplateXml)));

            // first remove the PI nodes if they're already present (which they shouldn't!)
            var piNodes = doc.SelectNodes("/processing-instruction()");
            if (piNodes != null)
            {
                foreach (XmlNode piNode in piNodes)
                {
                    if (piNode.LocalName == "mso-infoPathSolution" ||
                            piNode.LocalName == "mso-application" ||
                            piNode.LocalName == "MicrosoftWindowsSharePointServices")
                        doc.RemoveChild(piNode);
                }
            }

            // create PI values
            var mso_infoPathSolution = string.Format("name=\"{0}\" solutionVersion=\"{1}\" productVersion=\"{2}\" PIVersion=\"{3}\" href=\"{4}\"",
                                                            Name,
                                                            SolutionVersion,
                                                            ProductVersion,
                                                            "1.0.0.0",
                                                            HRef);
            var mso_application = string.Format("progid=\"{0}\" versionProgid=\"{1}\"",
                                                            "InfoPath.Document",
                                                            "InfoPath.Document.3");

            // add PIs to doc
            var pi = doc.CreateProcessingInstruction("mso-infoPathSolution", mso_infoPathSolution);
            doc.InsertBefore(pi, doc.DocumentElement);

            pi = doc.CreateProcessingInstruction("mso-application", mso_application);
            doc.InsertBefore(pi, doc.DocumentElement);

            return doc;
        }

        private XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (document.DocumentElement == null) throw new ArgumentNullException("document", "The root document element is null!");

            var ns = new XmlNamespaceManager(document.NameTable);
            foreach (XmlAttribute xatt in document.DocumentElement.Attributes)
            {
                var prefixPair = xatt.Name.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (prefixPair.Length < 1) continue;
                if (!prefixPair[0].Equals("xmlns", StringComparison.OrdinalIgnoreCase)) continue;

                var prefix = prefixPair.Length == 2
                                    ? prefixPair[1]
                                    : string.Empty;
                var uri = xatt.Value;
                ns.AddNamespace(prefix, uri);
            }
            return ns;
        }

        private static SPFile GetFormTemplateFile(SPWeb web, string xsnFormTemplateUrl)
        {
            try
            {
                var xsnFile = web.GetFile(xsnFormTemplateUrl);
                return xsnFile;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Unable to load form template {0} in Web {1}", xsnFormTemplateUrl, web.Site.Url));
            }
        }

        private string UpdateTemplate(string templateUrl, string templateXml)
        {
            XDocument xDocRoot = XDocument.Parse(templateXml);
            IEnumerable<XNode> nodes = xDocRoot.Nodes();
            foreach (XNode node in nodes)
            {
                if (node.ToString().Contains("mso-infoPathSolution"))
                //Lame, but couldn't find any other way.              
                {
                    if (!((XProcessingInstruction)node).Data.Contains("href="))
                    {
                        ((XProcessingInstruction)node).Data = ((XProcessingInstruction)node).Data + " href=\"" + templateUrl + "\"";
                    }
                    break;
                }
            }
            return xDocRoot.Declaration + xDocRoot.ToString();
        }

        private byte[] _docBytes = null;

        public byte[] DocumentBytes
        {
            get
            { return _docBytes; }
            set
            { _docBytes = value; }
        }

        #endregion
    }
}

