using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;


namespace Elan.SharePoint.LRFApproval.LRFApprovalListEventReceiver
{

    /// <summary>
    /// List Item Events
    /// </summary>
    public class LRFApprovalListEventReceiver : SPItemEventReceiver
    {

        //        const string requestAssignedLabel = "Attorney/Paralegal Assigned";


        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {

            SPWeb web = null;
            SPSite site = null;
            SPListItem item = null;


            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == Settings.Default.ListTitleLRF)
                {


                    item = properties.ListItem;
                    web = properties.Web;
                    site = properties.Web.Site;

                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        using (SPSite sitePriv = new SPSite(web.Site.ID))
                        using (SPWeb webPriv = sitePriv.OpenWeb(web.ID))
                        {

                            SPListItem itemPriv = webPriv.Lists[Properties.Settings.Default.ListTitleLRF].GetItemByUniqueId(item.UniqueId);
                            try
                            {

                                base.ItemAdded(properties);

                                String callingUserId = properties.CurrentUserId.ToString();

                                // create LRF object from SPListItem
                                LRF lrf = new LRF(itemPriv);

                                this.EventFiringEnabled = false;

                                if (lrf.status != LRF.REQUESTDRAFTSTATUS)
                                {

                                    lrf.SetApprovalFields();
                                    lrf.SetCostCentersField();
                                    lrf.SetSecurity();

                                    bool AllowUnsafeUpdates = webPriv.AllowUnsafeUpdates;

                                    try
                                    {
                                        webPriv.AllowUnsafeUpdates = true;
                                        itemPriv["Editor"] = callingUserId;
                                        itemPriv.Update();
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Trace.WriteLine("lrf ItemAdded: Error: " + ex.ToString());
                                    }
                                    finally
                                    {

                                        webPriv.AllowUnsafeUpdates = AllowUnsafeUpdates;
                                    }

                                    // if a valid approval list was generated, then try to start workflow
                                    if (lrf.valid)
                                    {
                                        try
                                        {
                                            SPWorkflowAssociation workflowAssociation = itemPriv.ParentList.WorkflowAssociations.GetAssociationByName(Properties.Settings.Default.WorkFlowName, CultureInfo.InvariantCulture);

                                            if (workflowAssociation != null)
                                            {
                                                //start workflow 
                                                SPWorkflow siteWorkflow = sitePriv.WorkflowManager.StartWorkflow(itemPriv, workflowAssociation, workflowAssociation.AssociationData);
                                            }
                                            else
                                            {
                                                Log.WriteLogEntry(itemPriv, "Could not find list Workflow: " + Properties.Settings.Default.WorkFlowName, "");
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            Log.WriteLogEntry(itemPriv, "Could not start Workflow: " + Properties.Settings.Default.WorkFlowName, ex.ToString());

                                        }
                                    }
                                    else
                                    {
                                        Log.WriteLogEntry(itemPriv, "Error generating valid lrf. ", lrf.invalidDetails);
                                    }

                                }
                                else
                                {

                                    lrf.SetSecurity();


                                    bool AllowUnsafeUpdates = webPriv.AllowUnsafeUpdates;

                                    try
                                    {
                                        webPriv.AllowUnsafeUpdates = true;
                                        itemPriv["Editor"] = callingUserId;
                                        itemPriv.Update();
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Trace.WriteLine("lrf ItemAdded: Error: " + ex.ToString());
                                    }
                                    finally
                                    {

                                        webPriv.AllowUnsafeUpdates = AllowUnsafeUpdates;
                                    }
                                }


                                this.EventFiringEnabled = true;

                            }
                            catch (Exception ex)
                            {
                                Common.Log.WriteOnlyLogEntry(itemPriv, "lrf ItemAdded: Error: ", ex.ToString());

                            }
                            finally
                            {
                                this.EventFiringEnabled = true;
                            }
                        }
                    });
                }
            }

        }


        public bool LegalTeamChanged(SPWeb web, SPItemEventProperties properties, ref SPPrincipal previousLegalTeam, ref SPPrincipal newLegalTeam)
        {

            previousLegalTeam = null;
            newLegalTeam = null;

            string idLegalBefore = string.Empty;
            string idLegalAfter = string.Empty;

            SPField f = properties.ListItem.ParentList.Fields.GetFieldByInternalName(LRF.FieldLegalTeamAssigned);
            string fieldDisplayName = f.Title;

            // fetch previous and current group

            try
            {

                if (properties.BeforeProperties[fieldDisplayName] != null)
                {
                    idLegalBefore = properties.BeforeProperties[fieldDisplayName].ToString();
                    if (!String.IsNullOrEmpty(idLegalBefore))
                        previousLegalTeam = web.SiteGroups.GetByID(Int32.Parse(idLegalBefore));
                }
            }
            catch (Exception ex)
            {
                Log.WriteOnlyLogEntry(web, "LegalTeamChanged: BeforeLegalTeam: ", ex.ToString());
            }

            try
            {
                if (properties.AfterProperties[fieldDisplayName] != null)
                {
                    idLegalAfter = properties.AfterProperties[fieldDisplayName].ToString();
                    if (!String.IsNullOrEmpty(idLegalAfter))
                        newLegalTeam = web.SiteGroups.GetByID(Int32.Parse(idLegalAfter));
                }
            }
            catch (Exception ex)
            {
                Log.WriteOnlyLogEntry(web, "LegalTeamChanged: AfterLegalTeam: ", ex.ToString());
            }


            // group before/after different ==> change
            if ((previousLegalTeam != null) && (newLegalTeam != null))
                if (idLegalAfter != idLegalBefore)
                    return true;

            //ignore all other cases
            return false;
        }

        public bool LegalAssigneeChanged(SPListItem item, SPItemEventProperties properties, out string nameLegalBefore, out string nameLegalAfter)
        {

            bool changed = false;

            //before
            nameLegalBefore = string.Empty;
            try
            {
                foreach (DictionaryEntry entry in properties.BeforeProperties)
                {
                    string entryKey = entry.Key.ToString();
                    if (entryKey.Contains(LRF.FieldLegalOwner))
                    {
                        nameLegalBefore = entry.Value.ToString();
                        break;
                    }
                }
            }
            catch  (Exception ex) {
                Log.WriteOnlyLogEntry(item, "PriorLegalOwnerLookup Error: ", ex.ToString());
            }


            //after

            nameLegalAfter = string.Empty;
            try
            {
                foreach (DictionaryEntry entry in properties.AfterProperties)
                {
                    string entryKey = entry.Key.ToString();
                    if (entryKey.Contains(LRF.FieldLegalOwner))
                    {
                        nameLegalAfter = entry.Value.ToString();                          
                        break;
                    }
                }
            }
            catch (Exception ex)
            { 
                Log.WriteOnlyLogEntry(item, "NewLegalOwnerLookup Error: ", ex.ToString()); 
            }


            if ((!String.IsNullOrEmpty(nameLegalAfter)) && (nameLegalBefore != nameLegalAfter))
                changed = true;

            return changed;
        }

        //private bool StatusChangedToExecuted(SPWeb web, SPListItem item, SPItemEventProperties properties)
        //{

        //    bool changed = false;

        //    string statusBefore = string.Empty;
        //    string statusAfter = string.Empty;

        //    try
        //    {
        //        if (properties.BeforeProperties[LRF.requestStatusField] != null)
        //            statusBefore = Request.GetRequestStatusValue(properties.Web, properties.BeforeProperties[LRF.requestStatusField].ToString());

        //        if (properties.AfterProperties[LRF.requestStatusField] != null)
        //            statusAfter = Request.GetRequestStatusValue(properties.Web, properties.AfterProperties[LRF.requestStatusField].ToString());

        //        SPFieldLookupValue fullyExecutedLookup = LRF.FullyExecuted(web); // Request.GetRequestStatusLookupField(web, "Fully Executed Contract");

        //        changed = (statusBefore != statusAfter) && (string.Compare(statusAfter, fullyExecutedLookup.ToString()) == 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteOnlyLogEntry(item, "ItemUpdating GetRequestStatusValue Error: ", ex.ToString());
        //    }


        //    return changed;
        //}

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {

            SPListItem item = null;
            SPWeb web = null;

            base.ItemUpdating(properties);

            try
            {

                if (properties != null && properties.List != null)
                {
                    if (properties.List.Title == Settings.Default.ListTitleLRF)
                    {

                        item = properties.ListItem;
                        web = properties.Web;

                        if (item != null)
                        {

                            SPSecurity.RunWithElevatedPrivileges(delegate
                             {
                                 using (SPSite sitePriv = new SPSite(web.Site.ID))
                                 using (SPWeb webPriv = sitePriv.OpenWeb(web.ID))
                                 {
                                     SPListItem itemPriv = webPriv.Lists[Properties.Settings.Default.ListTitleLRF].GetItemByUniqueId(item.UniqueId);

                                     try
                                     {
                                         //   this.EventFiringEnabled = false;
                                         string nameLegalBefore;
                                         string nameLegalAfter;
                                         if (LegalAssigneeChanged(itemPriv, properties, out nameLegalBefore, out nameLegalAfter))
                                         {
                                             LRF lrf = new LRF(item);
                                             if (lrf.approved && lrf.activeRequest)
                                             {
                                                 string recipients = null;
                                                 string subject = null;
                                                 string body = null;

                                                 LRF.CreateEmailComponents(item,  nameLegalAfter, out recipients, out subject, out body);
                                              
                                                 SPUtility.SendEmail(web, false, false, recipients, subject, body);
                                             }
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         Common.Log.WriteOnlyLogEntry(item, "Error: SendLRFAssignedEmail ", ex.ToString());

                                     }
                                     finally
                                     {
                                         this.EventFiringEnabled = true;
                                     }


                                     // rjg 4/25/12 : remove dynamic legal security and security adjustment
                                     //try
                                     //{
                                     //    SPPrincipal previousLegalTeam = null;
                                     //    SPPrincipal newLegalTeam = null;

                                     //    if (LegalTeamChanged(webPriv, properties, ref previousLegalTeam, ref newLegalTeam))
                                     //    {
                                     //        // modify security based upon last and current legal team group
                                     //        LRF lrf = new LRF(itemPriv);

                                     //        this.EventFiringEnabled = false;
                                     //        lrf.ResetLegalSecurity(previousLegalTeam, newLegalTeam);

                                     //    }

                                     //}
                                     //catch (Exception ex)
                                     //{
                                     //    Common.Log.WriteOnlyLogEntry(item, "Error: ResetLRFLegalSecurity ", ex.ToString());
                                     //}
                                     //finally
                                     //{
                                     //    this.EventFiringEnabled = true;
                                     //}

                                 }
                             });

                        }

                    }
                }

            }
            catch (SPException ex)
            {
                Common.Log.WriteOnlyLogEntry(item, "LRF ItemUpdating: Error: ", ex.ToString());
            }
            finally
            {
                //                this.EventFiringEnabled = true;
            }

        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            SPListItem item = null;
            SPWeb web = null;


            if (properties != null && properties.List != null)
            {
                if (properties.List.Title == Settings.Default.ListTitleLRF)
                {
                    String callingUserId = properties.CurrentUserId.ToString();

                    item = properties.ListItem;
                    web = item.ParentList.ParentWeb;

                    if (item != null)
                    {

                        base.ItemUpdated(properties);

                        SPSecurity.RunWithElevatedPrivileges(delegate
                       {
                           using (SPSite sitePriv = new SPSite(web.Site.ID))
                           using (SPWeb webPriv = sitePriv.OpenWeb(web.ID))
                           {
                               SPListItem itemPriv = webPriv.Lists[Properties.Settings.Default.ListTitleLRF].GetItemByUniqueId(item.UniqueId);

                               try
                               {
                                   this.EventFiringEnabled = false;

                                   bool needsUpdate = false;

                                   LRF lrf = new LRF(itemPriv);

                                   if (lrf.activeRequest && (lrf.approved || lrf.terminatedRequest))
                                   {
                                       lrf.SetCostCentersField();
                                       lrf.SetSecurity();
                                       needsUpdate = true;
                                   }

                                   //make sure attorney assigned status is set when attorney assigned
                                   if (lrf.activeRequest && lrf.approved && lrf.legalOwnerAssigned)
                                   {
                                       lrf.SetAttorneyAssignedStatus();
                                       needsUpdate = true;
                                   }

                                   if (needsUpdate)
                                   {
                                       bool AllowUnsafeUpdates = webPriv.AllowUnsafeUpdates;

                                       try
                                       {
                                           webPriv.AllowUnsafeUpdates = true;
                                           itemPriv["Editor"] = callingUserId;
                                           itemPriv.Update();
                                       }
                                       catch (Exception ex)
                                       {
                                           System.Diagnostics.Trace.WriteLine("lrf ItemUpdated: Error: " + ex.ToString());
                                       }
                                       finally
                                       {
                                           webPriv.AllowUnsafeUpdates = AllowUnsafeUpdates;
                                       }
                                   }

                               }
                               catch (Exception ex)
                               {
                                   Log.WriteOnlyLogEntry(itemPriv, "lrf ItemUpdated: Error: ", ex.ToString());
                               }
                               finally
                               {
                                   this.EventFiringEnabled = true;
                               }
                           }

                       });
                    }
                }
            }

        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);
        }

    }
}
