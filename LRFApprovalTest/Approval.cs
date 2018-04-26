using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.Office.Server.UserProfiles;
using Elan.SharePoint.LRFApproval.Common;

namespace LRFApprovalTest
{

    public class ApprovalFinance
    {


        internal static void ApprovalFinancial_invalidcc_details(SPWeb web)
        {
            string costCenterNumber = "00000";

            int requestAmount = 100000;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_noapproval_none(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 50000;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_firstlevel_oneapp(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 50001;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }
        internal static void ApprovalFinancial_firstlevelmax_oneapp(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 100000;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_secondlevel_twoapp(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 100001;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_secondlevelmax_twoapp(SPWeb web)
        {
            string costCenterNumber = "51427";
//            string costCenterNumber = "51404";

            int requestAmount = 500000;
//            int requestAmount = 200000;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }
        internal static void ApprovalFinancial_thirdlevel_threeapp(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 500001;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_thirdlevelmax_threeapp(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 1000000;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }

        internal static void ApprovalFinancial_high_all(SPWeb web)
        {
            string costCenterNumber = "51427";

            int requestAmount = 1000001;

            Elan.SharePoint.LRFApproval.Common.ApprovalFinance approvalFinance = new Elan.SharePoint.LRFApproval.Common.ApprovalFinance(web, costCenterNumber, requestAmount);

            string msg = string.Format("CostCenter {0} Amt {1} Apps {2} Details {3} Valid {4}", costCenterNumber, requestAmount, approvalFinance.ToString(), approvalFinance.details, approvalFinance.valid);

            Console.WriteLine(msg);
            Console.ReadKey();
        }


    }

    public class ApprovalDepartment
    {


        internal static void ApprovalDepartment_band5_onworkflow(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest3"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest3"];
            int requestAmount = 0;

            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);


            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void ApprovalDepartment_proxyband5_onworkflow(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest1"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest3"];
            int requestAmount = 0;
          
            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);

            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void ApprovalDepartment_proxyband3_onworkflow(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest1"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest2"]; 
            int requestAmount = 0;

            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);


            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void ApprovalDepartment_band3_notonworkflow(SPWeb web)
        {
            SPUser creator = web.Users[@"ecorp\webgrouptest2"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest2"];

            int requestAmount = 100000;

            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);


            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void ApprovalDepartment_proxynoapp_onworkflow(SPWeb web)
        {
            SPUser creator = web.Users[@"ecorp\webgrouptest1"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest2"];

            int requestAmount = 10000000;

            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);


            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void ApprovalDepartment_selfnoapp_onworkflow(SPWeb web)
        {
            SPUser creator = web.Users[@"ecorp\webgrouptest1"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest1"]; 
            
            int requestAmount = 10000000;

            Elan.SharePoint.LRFApproval.Common.ApprovalDepartment approvalDept = new Elan.SharePoint.LRFApproval.Common.ApprovalDepartment(web, creator, requestor, requestAmount);

            string msg = string.Format("Creator {0} Requestor {1} Amt {2} Approvers {3} Details {4} Valid {5}", creator.Name, requestor.Name, requestAmount, approvalDept.ToString(), approvalDept.details, approvalDept.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

    
    }

    public class Approval
    {

        internal static void Approval_dupdeptfin_merge(SPWeb web)
        {

            SPUser creator = web.EnsureUser(@"ecorp\webgrouptest1");
            SPUser requestor = web.EnsureUser(@"ecorp\webgrouptest10");
            web.Update();            

            int requestAmount = 1000001;
            string requestorCostCenterNumber = "51427";
            string lrfCostCenterNumber = "51427";

            Elan.SharePoint.LRFApproval.Common.Approval approval = new Elan.SharePoint.LRFApproval.Common.Approval(web, creator, requestor, requestAmount, requestorCostCenterNumber, lrfCostCenterNumber);



            string msg = string.Format("Creator {0} Requestor {1} Amt {2} ImpApp {3} WkfApp {4} Dept {5} Fin {6} Legal {7} Details {8} Valid {9}",
                                     creator.Name, requestor.Name, requestAmount, approval.implicitApproversToString(), approval.workflowApproversToString(), approval.deptApproval.ToString(), approval.finApproval.ToString(), approval.legalCostCenterGroup.Name, approval.details, approval.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }  
    
        internal static void Approval_deptfin_merge(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest3"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest3"];
            int requestAmount = 1000001;
            string requestorCostCenterNumber = "51427";
            string lrfCostCenterNumber = "51427";

            Elan.SharePoint.LRFApproval.Common.Approval approval = new Elan.SharePoint.LRFApproval.Common.Approval(web, creator, requestor, requestAmount, requestorCostCenterNumber, lrfCostCenterNumber);

            string msg = string.Format("Creator {0} Requestor {1} Amt {2} ImpApp {3} WkfApp {4} Dept {5} Fin {6} Legal {7} Details {8} Valid {9}",
                                            creator.Name, requestor.Name, requestAmount, approval.implicitApproversToString(), approval.workflowApproversToString(), approval.deptApproval.ToString(), approval.finApproval.ToString(), approval.legalCostCenterGroup.Name, approval.details, approval.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }
    
        internal static void Approval_nofin_merge(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest3"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest3"];
            int requestAmount = 0;
            string requestorCostCenterNumber = "51427";
            string lrfCostCenterNumber = "51427";

            Elan.SharePoint.LRFApproval.Common.Approval approval = new Elan.SharePoint.LRFApproval.Common.Approval(web, creator, requestor, requestAmount, requestorCostCenterNumber, lrfCostCenterNumber);

            string msg = string.Format("Creator {0} Requestor {1} Amt {2} ImpApp {3} WkfApp {4} Dept {5} Fin {6} Legal {7} Details {8} Valid {9}",
                                            creator.Name, requestor.Name, requestAmount, approval.implicitApproversToString(), approval.workflowApproversToString(), approval.deptApproval.ToString(), approval.finApproval.ToString(), approval.legalCostCenterGroup.Name, approval.details, approval.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }

        internal static void Approval_missingfin_invalid(SPWeb web)
        {

            SPUser creator = web.Users[@"ecorp\webgrouptest3"];
            SPUser requestor = web.Users[@"ecorp\webgrouptest3"];
            int requestAmount = 200000;
            string requestorCostCenterNumber = "51935";
            string lrfCostCenterNumber = "51404";

            Elan.SharePoint.LRFApproval.Common.Approval approval = new Elan.SharePoint.LRFApproval.Common.Approval(web, creator, requestor, requestAmount, requestorCostCenterNumber, lrfCostCenterNumber);

            string msg = string.Format("Creator {0} Requestor {1} Amt {2} ImpApp {3} WkfApp {4} Dept {5} Fin {6} Legal {7} Details {8} Valid {9}",
                                            creator.Name, requestor.Name, requestAmount, approval.implicitApproversToString(), approval.workflowApproversToString(), approval.deptApproval.ToString(), approval.finApproval.ToString(), approval.legalCostCenterGroup.Name, approval.details, approval.valid);
            Console.WriteLine(msg);

            Console.ReadKey();
        }  
    
    }
}
