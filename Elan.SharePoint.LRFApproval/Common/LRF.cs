using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;

namespace Elan.SharePoint.LRFApproval.Common
{

    public class LRF
    {

        public static readonly string REQUESTDRAFTSTATUS = "draft";

        private static readonly string CREATORFIELDNAME = "Created By";
        private static readonly string REQUESTORFIELDNAME = "Requestor";

        public static readonly string FieldContractingParty        = "PartyName"; //internal name
        public static readonly string FieldContractingPartyFN      = "OtherPartyFirstName"; //internal name
        public static readonly string FieldContractingPartyLN      = "OtherPartyLastName"; //internal name
        public static readonly string FieldContractingPartyWeb     = "OtherPartyWebsite"; //internal name

        public static readonly string FieldRequestorCostCenter     = "Requestor_x0020_Cost_x0020_Center"; //internal name
        public static readonly string FieldLRFCostCostCenter       = "Assigned_x0020_Cost_x0020_Center"; // internal name
        public static readonly string FieldLRFEnteredCostCenters   = "CostCenters"; //internal name
        public static readonly string ListLRFCostCenters           = "LRF Cost Centers";
        public static readonly string FieldLRFCostCentersTitle     = "Title";
        public static readonly string FieldLegalTeamAssigned       = "Assigned_x0020_To_x0020_Legal"; //internal name
        public static readonly string FieldLegalOwner              = "Legal_x0020_Owner";  //internal name

        //    public static readonly string LegalTeamField = "Assigned To Legal"; //display name
        public static readonly string requestStatusField = "Request_x0020_Status"; //internal name
        //   public static readonly string legalOwnerField = "Legal Team Contact"; //display name

        public static readonly string estimatedAmountField = "EstimatedAmount"; //internal name
        public static readonly string FieldFileName = "Name";

        private static readonly string FORMSERVERPAGE = "/_layouts/FormServer.aspx?XmlLocation=/";
        private static readonly string FORMITEMPAGE = "/Legal%2520Request%2520Forms%2FForms%2FAllItems%2Easpx&DefaultItemOpen=1";


        public static SPListItem GetItemById(SPWeb web, int lrfnumber)
        {

            SPListItem item = null;

            if (lrfnumber <= 0)
                return item;

            try
            {
                if (web.Lists[Settings.Default.ListTitleLRF] != null)
                    item = web.Lists.TryGetList(Settings.Default.ListTitleLRF).GetItemById(lrfnumber);
            }
            catch { }

            return item;

        }

        public static SPListItem GetItemById(SPWeb web, string lrfnumber)
        {

            SPListItem item = null;

            try
            {
                if (lrfnumber.Contains("#"))
                    lrfnumber = lrfnumber.Substring(lrfnumber.IndexOf("#") + 1);

                if (!string.IsNullOrEmpty(lrfnumber))
                {
                    int id = 0;
                    id = Convert.ToInt32(lrfnumber);
                    if (id > 0)
                        item = LRF.GetItemById(web, id);
                }
            }
            catch { }

            return item;
        }

        public static string GetPartyName(SPListItem lrf)
        {

            string party = string.Empty;

            if ((lrf[FieldContractingParty] != null) && (!String.IsNullOrEmpty(lrf[FieldContractingParty].ToString())))
                party = lrf[FieldContractingParty].ToString();
            else if ((lrf[FieldContractingPartyWeb] != null) && (!String.IsNullOrEmpty(lrf[FieldContractingPartyWeb].ToString())))
            {
                party = lrf[FieldContractingPartyWeb].ToString();
            }
            else if ((lrf[FieldContractingPartyLN] != null) && (!String.IsNullOrEmpty(lrf[FieldContractingPartyLN].ToString())))
            {
                if ((lrf[FieldContractingPartyFN] != null) && (!String.IsNullOrEmpty(lrf[FieldContractingPartyFN].ToString())))
                {
                    party = lrf[FieldContractingPartyFN].ToString() + " " + lrf[FieldContractingPartyLN].ToString();
                }
                else
                    party = lrf[FieldContractingPartyLN].ToString();
            }
            else
                party = "Unspecified Party";

            return party;

        }


        public static SPFieldLookupValue FullyExecuted(SPWeb web)
        {
            return Request.GetRequestStatusLookupField(web, "Fully Executed Contract");     
        }


        public static void CreateEmailComponents(SPListItem lrf, string currentLegalOwner,
                                                      out string recipients, out string subject, out string body)
        {
            recipients = string.Empty;
            subject = String.Empty;
            body = String.Empty;

            //set recipients list
            SPUser submitter = Common.User.GetUserFromField(lrf, CREATORFIELDNAME);
            if ((submitter != null) && (!String.IsNullOrEmpty(submitter.Email)))
                recipients += submitter.Email + ";";

            SPUser requestor = Common.User.GetUserFromField(lrf, REQUESTORFIELDNAME);
            if ((requestor != null) && (!String.IsNullOrEmpty(requestor.Email)))
                recipients += requestor.Email + ";";

            SPUser lrfLegalOwner = User.GetUserFromField(lrf, LRF.FieldLegalOwner);

            if ((lrfLegalOwner != null) && (!String.IsNullOrEmpty(lrfLegalOwner.Email)))
                recipients += lrfLegalOwner.Email + ";";

            //add current owner to recipient list and owner name
            string legalOwnerName = string.Empty; 
            if (!string.IsNullOrEmpty(currentLegalOwner))
            {
                recipients += currentLegalOwner;
                legalOwnerName = currentLegalOwner;
            } else
                legalOwnerName =  "N/A";


            // find party name for subject line: party name OR website OR first/last
            string party = LRF.GetPartyName(lrf);

            string subjectFormat = "{0} with {1} is assigned to {2}";
            subject = String.Format(subjectFormat, lrf.Name, party, legalOwnerName);

            //create body
            SPWeb web = lrf.ParentList.ParentWeb;
            string link = web.Url + FORMSERVERPAGE + lrf.Url + "&Source=" + web.Url + FORMITEMPAGE;

            body = subject + "<br/><br/>";
            if ((lrf[estimatedAmountField] != null) && (!String.IsNullOrEmpty(lrf[estimatedAmountField].ToString())))
                body += "Estimated Value of Approval is " + lrf[estimatedAmountField].ToString() + "<br/><br/>";

            body += String.Format("Click this <a href=\"{0}\">link</a> to review the request. ", link);

        }

        //instance variables

        public string status = string.Empty;
        public string requestStatus = string.Empty;

        public bool activeRequest = false;
        public bool terminatedRequest = false;
        public bool approved = false;
        public bool valid = true;
        public string invalidDetails = string.Empty;
        public bool legalOwnerAssigned = false;

        public int requestAmount = 0;
        public SPUser creator = null;
        public SPUser requestor = null;

        public string requestorCostCenterNumber = string.Empty;
        public CostCenterCharge[] costCenters;
        public decimal maxCostCenterValue = 0;
        public string lrfCostCenterNumber = string.Empty;

        private SPWeb _web;
        private SPListItem _item;


        public LRF(SPListItem item)
        {

            try
            {
                _item = item;
                _web = _item.Web;

                GetAllFields();

            }
            catch (Exception ex)
            {
                Log.WriteOnlyLogEntry(_item, "Error: Creating LRF object: ", ex.ToString());
                valid = false;
            }
        }

        public bool IsDraft()
        {
            return status == REQUESTDRAFTSTATUS;
        }

        public bool IsSubmitted()
        {
            SPFieldLookupValue submitLookup = Request.GetRequestStatusLookupField(_web, "LRF Submitted");
            SPFieldLookupValue createdLookup = Request.GetRequestStatusLookupField(_web, "LRF Created");

            return ((string.Compare(requestStatus, submitLookup.ToString()) == 0)
                     ||
                     (string.Compare(requestStatus, createdLookup.ToString()) == 0)
                     );
        }

        public bool IsCompleted()
        {
            SPFieldLookupValue canceledLookup = Request.GetRequestStatusLookupField(_web, "LRF Canceled");
            SPFieldLookupValue rejectedLookup = Request.GetRequestStatusLookupField(_web, "LRF Rejected");
            SPFieldLookupValue assignedLookup = Request.GetRequestStatusLookupField(_web, "Attorney/Paralegal Assigned");
            SPFieldLookupValue fullyExecutedLookup = Request.GetRequestStatusLookupField(_web, "Fully Executed Contract");

            return (string.Compare(requestStatus, canceledLookup.ToString()) == 0)
                           || (string.Compare(requestStatus, rejectedLookup.ToString()) == 0)
                           || (string.Compare(requestStatus, fullyExecutedLookup.ToString()) == 0)
                           || (string.Compare(requestStatus, assignedLookup.ToString()) == 0)
                           || (approved);
        }

        public void SetAttorneyAssignedStatus()
        {
            SPFieldLookupValue assignedLookup = Request.GetRequestStatusLookupField(_web, "Attorney/Paralegal Assigned");

            if (assignedLookup != null)
            {
                if (string.Compare(requestStatus, assignedLookup.ToString()) != 0)
                {
                    _item[Settings.Default.FieldRequestStatus] = assignedLookup;

                }
            }

        }

        private void GetAllFields()
        {

            GetItemFields();
            GetFormFields(); // must GetItemFields first

            if (maxCostCenterValue == 0)
                lrfCostCenterNumber = requestorCostCenterNumber;
        }

        private void GetFormFields()
        {

            if (_item.File == null || !_item.File.Exists)
            {
                Log.WriteOnlyLogEntry(_web, "LRF Error", "Can't access form file");
                valid = false;
                return;
            }

            SPFile lrfFormFile = _item.File;

            MemoryStream myInStream = new MemoryStream(lrfFormFile.OpenBinary());
            XmlDocument doc = new XmlDocument();
            doc.Load(myInStream);

            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(doc.NameTable);
            nameSpaceManager.AddNamespace("my", "http://schemas.microsoft.com/office/infopath/2003/myXSD/2011-10-05T00:10:10");
            nameSpaceManager.AddNamespace("pc", "http://schemas.microsoft.com/office/infopath/2007/PartnerControls");

            XmlElement root = doc.DocumentElement;

            XmlNodeList selectedNodes = root.SelectNodes("/my:myFields/my:group15/my:group21/my:group22/my:group23", nameSpaceManager);

            if ((selectedNodes == null) || (selectedNodes.Count < 1))
                return;

            //check that a nonNull item is found
            bool ccItemFound = false;

            costCenters = new CostCenterCharge[selectedNodes.Count];

            int costCenterIndex = 0;
            foreach (XmlNode selectedNode in selectedNodes)
            {

                CostCenterCharge costCenterCharge = new CostCenterCharge();

                costCenterCharge.CostCenter = selectedNode.SelectSingleNode("my:ItemCostCenter", nameSpaceManager).InnerXml;

                XmlNodeList CCChargeDetails = selectedNode.SelectNodes("my:group24/my:group25", nameSpaceManager);
                if (CCChargeDetails.Count > 0)
                {
                    int detailIndex = 0;
                    CostCenterDetails[] ccDetails = new CostCenterDetails[CCChargeDetails.Count];
                    foreach (XmlNode node in CCChargeDetails)
                    {
                        string amt = node.SelectSingleNode("my:ItemAmount", nameSpaceManager).InnerXml;
                        if (!String.IsNullOrEmpty(amt))
                        {
                            ccItemFound = true;
                            CostCenterDetails ccdetail = new CostCenterDetails();
                            ccdetail.Amount = amt;
                            ccdetail.Description = node.SelectSingleNode("my:ItemDescription", nameSpaceManager).InnerXml;
                            ccdetail.ProductCategory = node.SelectSingleNode("my:ItemProductCategory", nameSpaceManager).InnerXml;
                            ccDetails[detailIndex] = ccdetail;
                            detailIndex = detailIndex + 1;
                        }
                    }
                    costCenterCharge.CostCenterEntry = ccDetails;
                }
                costCenters[costCenterIndex] = costCenterCharge;
                costCenterIndex = costCenterIndex + 1;
            }

            if (!ccItemFound)
                costCenters = null;
            else
            {
                lrfCostCenterNumber = GetMaxCostCenter(out maxCostCenterValue);
            }

        }

        private string GetMaxCostCenter(out decimal maxCostCenterValue)
        {

            string maxCostCenter = string.Empty;
            maxCostCenterValue = 0;

            if (costCenters != null)
            {
                foreach (CostCenterCharge ccc in costCenters)
                {
                    decimal curCostCenterValue = ccc.CostCenterEntry.Sum(cce => Decimal.Parse(cce.Amount));

                    if (curCostCenterValue > maxCostCenterValue)
                    {
                        maxCostCenter = ccc.CostCenter;
                        maxCostCenterValue = curCostCenterValue;
                    }
                }
            }

            return maxCostCenter;

        }

        private void GetItemFields()
        {

            string requestorusername = string.Empty;
            if (_item[Settings.Default.FieldLRRequestor] != null && !string.IsNullOrEmpty(_item[Settings.Default.FieldLRRequestor].ToString()))
            {
                requestorusername = _item[Settings.Default.FieldLRRequestor].ToString();
                requestor = _web.EnsureUser(requestorusername);
            }

            if (requestor == null)
            {
                Log.WriteOnlyLogEntry(_web, "Error: Could not find user for requestor. Name: ", requestorusername);
                valid = false;
                return;
            }


            try
            {
                if (_item[LRF.estimatedAmountField] != null && !string.IsNullOrEmpty(_item[LRF.estimatedAmountField].ToString()))
                    requestAmount = Util.MakeInt(_item[LRF.estimatedAmountField].ToString());
            }
            catch { }

            int band;
            int AuthAmount;
            SPUser manager;

            User.GetUserAttributes(_web, requestor, out band, out AuthAmount, out manager, out requestorCostCenterNumber);
            if (string.IsNullOrEmpty(requestorCostCenterNumber))
            {
                Log.WriteOnlyLogEntry(_web, "Error: Could not find user Cost Center Number", requestor.Name);
                valid = false;
            }


            if (_item[CREATORFIELDNAME] != null && !string.IsNullOrEmpty(_item[CREATORFIELDNAME].ToString()))
            {
                creator = User.GetUserFromField(_item, CREATORFIELDNAME);
            }
            if (creator == null)
            {
                Log.WriteLogEntry(_item, "Error: Invalid or Missing Required field: RequestCreator", "Missing Required field: Request Creator.");
                valid = false;
            }

            if (_item[Settings.Default.FieldLRFStatus] != null && !string.IsNullOrEmpty(_item[Settings.Default.FieldLRFStatus].ToString()))
            {
                status = _item[Settings.Default.FieldLRFStatus].ToString().ToLower();
            }
            else
            {
                valid = false;
            }


            //set various LRF status variables


            if (_item[Settings.Default.FieldRequestStatus] != null)
            {
                requestStatus = _item[Settings.Default.FieldRequestStatus].ToString();

                SPFieldLookupValue fullyExecutedLookup = Request.GetRequestStatusLookupField(_web, "Fully Executed Contract");

                activeRequest = string.Compare(requestStatus, fullyExecutedLookup.ToString()) != 0;

                SPFieldLookupValue canceledLookup = Request.GetRequestStatusLookupField(_web, "LRF Canceled");
                SPFieldLookupValue rejectedLookup = Request.GetRequestStatusLookupField(_web, "LRF Rejected");

                terminatedRequest = (string.Compare(requestStatus, canceledLookup.ToString()) == 0)
                                                                ||
                                    (string.Compare(requestStatus, rejectedLookup.ToString()) == 0);

            }

            if (_item[Settings.Default.FieldApproved] != null)
            {
                string approvedValue = _item[Settings.Default.FieldApproved].ToString();
                if (!string.IsNullOrEmpty(approvedValue))
                {
                    if (string.Compare(approvedValue, "Yes") == 0)
                    {
                        approved = true;
                    }
                }
            }

            legalOwnerAssigned = (_item[Settings.Default.FieldLegalOwner] != null) && (!string.IsNullOrEmpty(_item[Settings.Default.FieldLegalOwner].ToString()));

        }

        public void SetCostCentersField() 
        {
            if (costCenters == null)
                return;

            SPList list = _web.Lists[ListLRFCostCenters]; //cost center value list

            if (list == null)
                return;
           
            SPFieldLookupValueCollection ccLookupCollection = new SPFieldLookupValueCollection();
          
            foreach (CostCenterCharge cc in costCenters)
            {
                SPListItem item = Util.InsertItemUnique(list, FieldLRFCostCentersTitle, cc.CostCenter);
                SPFieldLookupValue ccLookupValue = new SPFieldLookupValue(item.ID, cc.CostCenter);
                ccLookupCollection.Add(ccLookupValue);
            }

            if (ccLookupCollection.Count > 0)
                _item[FieldLRFEnteredCostCenters] = ccLookupCollection.ToString();
        }

        public void SetApprovalFields()
        {

            Approval approval = null;

            // never set approvers at draft stage
            if (status == REQUESTDRAFTSTATUS)
                return;

            //set cost center fields

            if (_item[FieldRequestorCostCenter] == null)
                if (requestorCostCenterNumber != null)
                {
                    try
                    {
                        _item[FieldRequestorCostCenter] = requestorCostCenterNumber;

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("lrf FieldRequestorCostCenter: Error: " + ex.ToString());
                    }
                }

            if (_item[FieldLRFCostCostCenter] == null)
                if (lrfCostCenterNumber != null)
                {
                    try
                    {
                        _item[FieldLRFCostCostCenter] = lrfCostCenterNumber;

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("lrf FieldLRFCostCenter: Error: " + ex.ToString());
                    }
                }



            // if approvers have already been set, don't overwrite
            // (NOTE: there should always be dept approver, so this is the check)
            if (_item[Settings.Default.FieldLastDeptApprover] != null)
                return;


            // determine approvers

            approval = new Approval(_web, creator, requestor, requestAmount, requestorCostCenterNumber, lrfCostCenterNumber);

            if ((!approval.valid) && (!String.IsNullOrEmpty(approval.details)))
                invalidDetails = approval.details;

            //keep going even if approvers not all valid (caller can decide what to do)
            valid &= approval.valid;

            // set approval derived fields

            List<SPUser> approversDept = approval.deptApproval.approvers;
            if (approversDept != null && approversDept.Count > 0)
            {
                SPFieldUserValueCollection approversDeptField = new SPFieldUserValueCollection();
                foreach (SPUser user in approversDept)
                {
                    Common.User.AddUserToField(_web, approversDeptField, user);
                }
                _item[Settings.Default.FieldLRFDeptApprovers] = approversDeptField;
                _item[Settings.Default.FieldLastDeptApprover] = approversDeptField[approversDeptField.Count - 1];

            }


            List<SPUser> approversFinancial = approval.finApproval.approvers;
            if (approversFinancial != null && approversFinancial.Count > 0)
            {
                SPFieldUserValueCollection approversFinancialField = new SPFieldUserValueCollection();
                foreach (SPUser user in approversFinancial)
                {
                    Common.User.AddUserToField(_web, approversFinancialField, user);
                }
                _item[Settings.Default.FieldLRFFinancialApprovers] = approversFinancialField;
                _item[Settings.Default.FieldLastFinancialApprover] = approversFinancialField[approversFinancialField.Count - 1];


            }

            List<SPUser> approversWorkflow = approval.workflowApprovers;
            if (approversWorkflow != null && approversWorkflow.Count > 0)
            {
                SPFieldUserValueCollection approversWorkflowField = new SPFieldUserValueCollection();
                foreach (SPUser user in approversWorkflow)
                {
                    Common.User.AddUserToField(_web, approversWorkflowField, user);
                }
                _item[Settings.Default.FieldLRFFormApprovers] = approversWorkflowField;


            }


            if (approval.legalCostCenterGroup != null)
            {

                _item[LRF.FieldLegalTeamAssigned] = approval.legalCostCenterGroup;

            }


        }

        
        
        public void LegacyLRFCostCenterFields(bool reset)
        {

            bool needUpdate = false;

            if ( (_item[FieldRequestorCostCenter] == null) || reset)
                if (requestorCostCenterNumber != null)
                {
                    try
                    {
                        _item[FieldRequestorCostCenter] = requestorCostCenterNumber;
                        needUpdate = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("lrf FieldRequestorCostCenter: Error: " + ex.ToString());
                    }
                }

            if (_item[FieldLRFCostCostCenter] == null || reset)
                if (lrfCostCenterNumber != null)
                {
                    try
                    {
                        _item[FieldLRFCostCostCenter] = lrfCostCenterNumber;
                        needUpdate = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("lrf FieldLRFCostCenter: Error: " + ex.ToString());
                    }
                }



            if ( needUpdate && (costCenters != null) )
            {

                SPList list = _web.Lists[ListLRFCostCenters]; //cost center value list

                if (list == null)
                    return;

                SPFieldLookupValueCollection ccLookupCollection = new SPFieldLookupValueCollection();

                foreach (CostCenterCharge cc in costCenters)
                {
                    SPListItem item = Util.InsertItemUnique(list, FieldLRFCostCentersTitle, cc.CostCenter);
                    SPFieldLookupValue ccLookupValue = new SPFieldLookupValue(item.ID, cc.CostCenter);
                    ccLookupCollection.Add(ccLookupValue);
                }

                if (ccLookupCollection.Count > 0)
                {
                    _item[FieldLRFEnteredCostCenters] = ccLookupCollection.ToString();
                    
                }
            }
            
            if (needUpdate)
                _item.Update();
        }

        public void ResetLegalSecurity(SPPrincipal previousLegalTeam, SPPrincipal newLegalTeam)
        {

            if ((previousLegalTeam == null) && (newLegalTeam == null))
                return;

            //only change when group is changing

            if (previousLegalTeam != null)
                Security.ClearItemSecurity(_item, previousLegalTeam);

            if (newLegalTeam != null)
                Security.SetItemContribute(_item, newLegalTeam);

        }

        public void SetSecurity()
        {

            List<SPPrincipal> readers = new List<SPPrincipal>();
            List<SPPrincipal> contributors = new List<SPPrincipal>();

            if (IsDraft())
                GetDraftSecurity(readers, contributors);
            else if (IsSubmitted())
                GetSubmittedSecurity(readers, contributors);
            else if (IsCompleted() || approved)
                GetCompletedSecurity(readers, contributors);

            Dictionary<string, UserPermission> perms = new Dictionary<string, UserPermission>();

            perms = Security.GetSecurityList(_web, readers, contributors);

            if (perms != null)
            {
                Security.ClearItemSecurity(_item);
                Security.AssignItemPermissions(_item, perms);
            }

        }

        private void GetDraftSecurity(List<SPPrincipal> readers, List<SPPrincipal> contributors)
        {


            //Assigned Legal group --> Contribute
            if (creator != null)
                contributors.Add(creator);

            //Assigned Legal group --> Contribute
            if (requestor != null)
                contributors.Add(requestor);

        }

        private SPGroup GetCostCenterSuperUsers()
        {
            SPGroup costCenterSuperUsers = null;
            if (!String.IsNullOrEmpty(requestorCostCenterNumber))
            {

                string groupCostCenterSuperUser = Properties.Settings.Default.GroupCostCenterSuperUserPrefix + requestorCostCenterNumber;
                try
                {
                    costCenterSuperUsers = _web.SiteGroups[groupCostCenterSuperUser];
                    return costCenterSuperUsers;
                }
                catch
                {
                    Log.WriteOnlyLogEntry(_web, "Error: Could not find Cost Center Super User Group. ", groupCostCenterSuperUser);
                }
            }

            return null;
        }

        private SPGroup GetLegalGroup()
        {
            SPGroup legal = null;

            try
            {
                //rjg: 4/25/12 : revert to fixed legal team security group
                //if (_item[LRF.FieldLegalTeamAssigned] != null)
                //{
                //    if (!String.IsNullOrEmpty(_item[LRF.FieldLegalTeamAssigned].ToString()))
                //    {
                //        string legalId = _item[LRF.FieldLegalTeamAssigned].ToString();
                //        if (legalId.IndexOf("#") > 0) {
                //            legalId = legalId.Substring(0, legalId.IndexOf(";") );
                //        }
                //        legal = _web.SiteGroups.GetByID(Int32.Parse(legalId));
                //    }
                //}
                legal = _web.SiteGroups[Settings.Default.GroupLegalTeam]; 
            }
            catch (Exception ex)
            {
                Log.WriteOnlyLogEntry(_web, "GetLegalGroup: ", ex.ToString());
            }

            return legal;
        }

        private void GetSubmittedSecurity(List<SPPrincipal> readers, List<SPPrincipal> contributors)
        {

            // cost center superuser
            SPGroup costCenterSuperUsers = GetCostCenterSuperUsers();

            SPGroup legal = GetLegalGroup();

            List<SPUser> workflowApprovers = null;

            // read the approvers list from the listItem
            workflowApprovers = User.GetUsersFromField(_item, Settings.Default.FieldLRFFormApprovers);

            //for submit, creator/requestor read, approvers edit

            //Cost center Super users --> Read
            if (costCenterSuperUsers != null)
                readers.Add(costCenterSuperUsers);

            //Purchasing Team Group --> Read
            SPGroup purchasing = _web.SiteGroups[Properties.Settings.Default.GroupPurchasingTeam];
            if (purchasing != null)
                readers.Add(purchasing);

            //ComplianceTeam Group --> Read
            SPGroup compliance = _web.SiteGroups[Properties.Settings.Default.GroupComplianceTeam];
            if (compliance != null)
                readers.Add(compliance);


            //Finance Group --> Contribute
            SPGroup finance = _web.SiteGroups[Properties.Settings.Default.GroupFinanceTeam];
            if (finance != null)
                contributors.Add(finance);

            //Assigned Legal group --> Contribute
            if (legal != null)
                contributors.Add(legal);

            //LRF Super users --> Contribute
            SPGroup su = _web.SiteGroups[Properties.Settings.Default.GroupLRFSuperUsers];
            if (su != null)
                contributors.Add(su);

            //creator and requestor always get read access
            readers.Add(creator);
            readers.Add(requestor); //if requestor is workflow approvers, they will get contribute access later

            if (workflowApprovers != null)
                foreach (SPPrincipal p in workflowApprovers)
                {
                    contributors.Add(p);
                }


        }

        private void GetCompletedSecurity(List<SPPrincipal> readers, List<SPPrincipal> contributors)
        {

            // cost center superuser
            SPGroup costCenterSuperUsers = GetCostCenterSuperUsers();

            SPGroup legal = GetLegalGroup();

            //for completed, legal group edit, other read

            //Cost center Super users --> Read
            if (costCenterSuperUsers != null)
                readers.Add(costCenterSuperUsers);

            //Purchasing Team Group --> Read
            SPGroup purchasing = _web.SiteGroups[Properties.Settings.Default.GroupPurchasingTeam];
            if (purchasing != null)
                readers.Add(purchasing);

            //ComplianceTeam Group --> Read

            SPGroup compliance = _web.SiteGroups[Properties.Settings.Default.GroupComplianceTeam];
            if (compliance != null)
                readers.Add(compliance);

            //Finance Group --> Contribute
            SPGroup finance = _web.SiteGroups[Properties.Settings.Default.GroupFinanceTeam];
            if (finance != null)
                readers.Add(finance);


            //Assigned Legal group --> Contribute
            if (legal != null)
                contributors.Add(legal);

            //LRF Super users --> Contribute
            SPGroup su = _web.SiteGroups[Properties.Settings.Default.GroupLRFSuperUsers];
            if (su != null)
                contributors.Add(su);

            //creator and requestor always get read access
            readers.Add(creator);
            readers.Add(requestor);

            // read the approvers list from the listItem
            List<SPUser> workflowApprovers = null;
            workflowApprovers = User.GetUsersFromField(_item, Settings.Default.FieldLRFFormApprovers);
            if (workflowApprovers != null)
                foreach (SPPrincipal p in workflowApprovers)
                {
                    readers.Add(p);
                }


        }


    }
}
