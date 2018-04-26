using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Elan.SharePoint.LRFApproval.Properties;


namespace Elan.SharePoint.LRFApproval.Common
{
    sealed class UserComparer : IEqualityComparer<SPUser>
    {
        public bool Equals(SPUser x, SPUser y)
        { return x.ID == y.ID; }
        public int GetHashCode(SPUser obj)
        { return obj.ID.GetHashCode(); }
    }

    public class ApprovalBase
    {
        protected StringBuilder _details = new StringBuilder();
        public string details = string.Empty;
        public bool valid = true;
    }

    public class ApprovalDepartment : ApprovalBase
    {

        static readonly int MINPARTICIPANTBAND = 5;
        static readonly int MINAPPROVERBAND = 6;

        public List<SPUser> approvers = new List<SPUser>();

        override public string ToString()
        {
            
            string app = string.Empty;
            if (approvers != null)
               foreach (SPUser a in approvers)
               {
                   app += a.Name + " ";
               }
             return app;
        }

        public ApprovalDepartment(SPWeb web, SPUser creator, SPUser requestor,
                                     int requestAmount)
        {

            if ((requestor == null) ||
                 (creator == null))
            {
                valid = false;
                return;
            }

            //in the case of proxy submission, always add requestor to approvers
            if (requestor.LoginName != creator.LoginName)
            {
                if (!approvers.Contains(requestor, new UserComparer()))
                    approvers.Add(requestor);
            }

            valid = GetDeptApprovers(web, requestor, requestAmount);

        }


        private bool GetDeptApprovers(SPWeb web, SPUser person, int requestAmount)
        {

            int band = 0;
            int AuthAmount = 0;
            SPUser manager = null;
            string costCenter = string.Empty;

            //fetch detais for current person
            bool validUser = User.GetUserAttributes(web, person, out band, out AuthAmount, out manager, out costCenter);

            if (!validUser)
            {
                _details.Append("User Profile not found for " + person.Name + ".");
                approvers.Clear();
                return false;
            }


            if (band >= MINPARTICIPANTBAND) //approver must be of min band
            {
                //add qualifying user (if they are not already on list)
                if (!approvers.Contains(person, new UserComparer()))
                    approvers.Add(person);

                //If user has sufficient authorization,

                if (AuthAmount >= requestAmount)
                {
                    //test that approver is valid approver band

                    if (band >= MINAPPROVERBAND)  // done
                        return true;
                }
            }

            //at this point, approver list is incomplete, so repeat tests with manager

            if (manager != null)
            {
                return GetDeptApprovers(web, manager, requestAmount);
            }
            else
            {
                Log.WriteOnlyLogEntry(web, "Fatal: Can't create manager from profile", "User " + person);
                _details.Append("Cannot access user information for manager in reporting structure.");
                approvers.Clear();
                return false;
            }

        }

    }

    public class ApprovalFinance : ApprovalBase
    {

        static readonly int LEVEL1AMT = 50000;
        static readonly int LEVEL2AMT = 100000;
        static readonly int LEVEL3AMT = 500000;
        static readonly int LEVEL4AMT = 1000000;

        public List<SPUser> approvers = new List<SPUser>();
        
        override public string ToString()
        {
            string app = string.Empty;
            if (approvers != null)
                foreach (SPUser a in approvers)
                {
                    app += a.Name + " ";
                }
            return app;
        }

        public ApprovalFinance(SPWeb web, string costCenterNumber, int requestAmount)
        {

            if (requestAmount <= LEVEL1AMT)
            {
                valid = true;
                return;
            }

            if (String.IsNullOrEmpty(costCenterNumber))
            {
                _details.Append("No cost center specified for financial approval.");
                valid = false;
                return;
            }


            if (requestAmount > LEVEL1AMT)
            {
                SPUser fa = null;
                fa = GetFinancialAssignment(web, costCenterNumber, Properties.Settings.Default.FieldValueAnalyst);
                if (fa == null)
                {
                    string msg = "Financial Analyst could not be found for Cost Center " + costCenterNumber + ". ";
                    _details.Append(msg);
                    valid = false;
                }
                else
                    if (!approvers.Contains(fa, new UserComparer()))
                        approvers.Add(fa); //all financial analyst on security/approval lists

            }


            if (requestAmount > LEVEL2AMT)
            {
                SPUser fm = null;
                fm = GetFinancialAssignment(web, costCenterNumber, Properties.Settings.Default.FieldValueManagement);
                if (fm == null)
                {
                    string msg = "Financial Manager could not be found for Cost Center " + costCenterNumber + ". ";
                    _details.Append(msg);
                    valid = false;
                }
                else

                    if (!approvers.Contains(fm, new UserComparer()))
                        approvers.Add(fm); //all financial managers on security/approval lists

            }


            if (requestAmount > LEVEL3AMT)
            {
                SPUser financevp = null;
                financevp = GetExecutiveApprover(web, Settings.Default.VPOfFinanceTitle);
                if (financevp == null)
                {
                    string msg = "Financial Executive could not be found. ";
                    _details.Append(msg);
                    valid = false;
                }
                else
                    if (!approvers.Contains(financevp, new UserComparer()))
                        approvers.Add(financevp);
            }

            if (requestAmount > LEVEL4AMT)
            {
                SPUser ceo = null;
                ceo = GetExecutiveApprover(web, Settings.Default.CFOTitle);
                if (ceo == null)
                {
                    string msg = "CFO could not be found. ";
                    _details.Append(msg);
                    valid = false;
                }
                else
                    if (!approvers.Contains(ceo, new UserComparer()))
                        approvers.Add(ceo);
            }

            details = _details.ToString();
        }


        private SPUser GetExecutiveApprover(SPWeb web, string title)
        {
            SPList list = web.Site.RootWeb.Lists.TryGetList(Settings.Default.ListTitleExecutiveApprovers);
            if (list == null)
            {
                Log.WriteOnlyLogEntry(web, "Fatal Exception, Missing list: " + Settings.Default.ListTitleExecutiveApprovers, "Please contact an administrator to validate process dependencies.");
                return null;
            }

            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>", title);
            SPListItemCollection itemCollection = list.GetItems(query);

            if (itemCollection != null && itemCollection.Count > 0)
            {
                SPFieldUser field = itemCollection.Fields[Settings.Default.FieldUser] as SPFieldUser;
                SPFieldUserValue fieldValue = field.GetFieldValue(itemCollection[0][Settings.Default.FieldUser].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    return fieldValue.User;
                }
            }


            return null;

        }

        protected SPUser GetFinancialAssignment(SPWeb web, string costCenterNumber, string userType)
        {
            SPList list = web.Site.RootWeb.Lists.TryGetList(Settings.Default.ListTitleFinancialAssignment);
            if (list == null)
            {
                Log.WriteOnlyLogEntry(web, "Fatal Exception: Missing list - " + Settings.Default.ListTitleFinancialAssignment, "Please contact an administrator to validate process dependencies. List must exist in the root site collection");
                return null;
            }

            string costCenterName = SAP.GetCostCenterName(web, costCenterNumber);
            if (string.IsNullOrEmpty(costCenterName))
            {
                return null;
            }

            SPQuery query = new SPQuery();
            query.Query = "<Where><And><Eq><FieldRef Name='" + Settings.Default.FieldCostCenter + "' /><Value Type='Text'>" + costCenterName + "</Value></Eq><Eq><FieldRef Name='"
                + Settings.Default.FieldUserType + "' /><Value Type='Choice'>" + userType + "</Value></Eq></And></Where>";

            SPListItemCollection itemCollection = list.GetItems(query);
            if (itemCollection != null && itemCollection.Count > 0)
            {
                SPFieldUser field = itemCollection.Fields[Settings.Default.FieldUser] as SPFieldUser;
                SPFieldUserValue fieldValue = field.GetFieldValue(itemCollection[0][Settings.Default.FieldUser].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    return fieldValue.User;
                }
            }

            return null;
        }
 
    }

    public class Approval : ApprovalBase
    {

        public ApprovalDepartment deptApproval;
        public ApprovalFinance finApproval;
        public List<SPUser> workflowApprovers;       
        public List<SPUser> implicitApprovers;         
        public SPGroup legalCostCenterGroup;

        public string workflowApproversToString()
        {
            string app = string.Empty;
            if (workflowApprovers != null)
                foreach (SPUser a in workflowApprovers)
                {
                    app += a.Name + " ";
                }
            return app;
        }

        public string implicitApproversToString()
        {
            string app = string.Empty;
            if (implicitApprovers != null)
                foreach (SPUser a in implicitApprovers)
                {
                    app += a.Name + " ";
                }
            return app;
        }

        override public string ToString()
        {
            return string.Format("Department Approvers {0} Financial Approvers {1} All Approvers {2} Workflow Approvers {3} LegalGroup {4}", deptApproval.ToString(), finApproval.ToString(), implicitApproversToString(), workflowApproversToString(), legalCostCenterGroup.ToString() );
        }

        public Approval(SPWeb web, SPUser creator, SPUser requestor, int requestAmount, string requestorCostCenter, string lrfCostCenter)
        {
            deptApproval = new ApprovalDepartment(web, creator, requestor, requestAmount);
            valid &= deptApproval.valid;

            finApproval = new ApprovalFinance(web, lrfCostCenter, requestAmount);
            valid &= finApproval.valid;

            valid &= GetLegalParticipant(web, requestorCostCenter);

            workflowApprovers = new List<SPUser>();
            implicitApprovers = new List<SPUser>();

            SetImplicitApprovers(creator);
            SetWorkflowApprovers(creator);

            details = deptApproval.details + finApproval.details + _details.ToString();
        }

        private void SetWorkflowApprovers(SPUser creator)
        {
            //unique persons from dept & fin approvers excluding creator (if self created)

            //dept approvers first
            foreach (SPUser member in  deptApproval.approvers)
            {
                if (!workflowApprovers.Contains(member, new UserComparer()) && (member.LoginName != creator.LoginName))
                    workflowApprovers.Add(member);
            }

            foreach (SPUser member in finApproval.approvers)
            {
                if (!workflowApprovers.Contains(member, new UserComparer()) && (member.LoginName != creator.LoginName))
                    workflowApprovers.Add(member);
            }
            
        }

        private void SetImplicitApprovers(SPUser creator)
        {

            //creator is first implicit approver
            if (!implicitApprovers.Contains(creator, new UserComparer()))
                implicitApprovers.Add(creator);   
            
            //unique persons from dept, fin approvers including creator
            foreach (SPUser member in deptApproval.approvers)
            {
                if (!implicitApprovers.Contains(member, new UserComparer()) && (member.LoginName != creator.LoginName) )
                    implicitApprovers.Add(member);
            }

            foreach (SPUser member in finApproval.approvers)
            {
                if (!implicitApprovers.Contains(member, new UserComparer()) && (member.LoginName !=creator.LoginName))
                    implicitApprovers.Add(member);
            }
            
 
        }


        public bool GetLegalParticipant(SPWeb web, string costCenterNumber)
        {
            legalCostCenterGroup = null;

            legalCostCenterGroup = GetLegalCostCenterAssignmentGroup(web, costCenterNumber);

            if (legalCostCenterGroup == null)
            {
                string msg = "Legal Group could not be found for Cost Center " + costCenterNumber + ". ";
                _details.Append(msg);
                return false;
            }
            else
                return true;
        }

        protected SPGroup GetLegalCostCenterAssignmentGroup(SPWeb web, string costCenterNumber)
        {
            SPList list = web.Site.RootWeb.Lists.TryGetList(Settings.Default.ListTitleLegalCostCenterAssignment);
            if (list == null)
            {
                Log.WriteOnlyLogEntry(web, "Error: Missing list - " + Settings.Default.ListTitleLegalCostCenterAssignment, "Please contact an administrator to validate process dependencies. List must exist in the root site collection");
                return null;
            }

            string costCenterName = SAP.GetCostCenterName(web, costCenterNumber);
            if (string.IsNullOrEmpty(costCenterName))
            {
               return null;
            }

            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='" + Settings.Default.FieldCostCenter + "' /><Value Type='Text'>" + costCenterName + "</Value></Eq></Where>";

            SPListItemCollection itemCollection = list.GetItems(query);
            if (itemCollection != null && itemCollection.Count > 0)
            {
                SPFieldUser field = itemCollection.Fields[Settings.Default.FieldLegalCostCenterAssignmentGroup] as SPFieldUser;
                SPFieldUserValue fieldValue = field.GetFieldValue(itemCollection[0][Settings.Default.FieldLegalCostCenterAssignmentGroup].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    if (web.SiteGroups[fieldValue.LookupValue] != null)
                        return web.SiteGroups[fieldValue.LookupValue];
                    else
                    {
                        Log.WriteOnlyLogEntry(web, "Warning: Could not find Legal Cost Center approver Group: " + fieldValue.LookupValue, "");
                    }
                }
            }
          return null;
        }
    }
}
