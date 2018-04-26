using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Elan.SharePoint.LRFApproval.Common;
using Elan.SharePoint.LRFApproval.LRFApprovalListEventReceiver;

namespace LRFApprovalTest
{

    class Program
    {

        static void Main(string[] args)
        {
            using (SPSite site = new SPSite("http://ecms-dev"))
            {
                SPWebCollection sites = site.AllWebs;

                SPWeb web = sites[0];




        //           AgreementsEventReceiver.AgreementsLegacy_Requestor_False(web);


        //      LRFApprovalListEventReceiver.LegalAssigneeChanged_NoChange_False(web);
          //      LRFApprovalListEventReceiver.LegalAssigneeChanged_Change_True(web);

        //    LRFApprovalListEventReceiver.LegalTeamChanged_Change_True(web);
        //    LRFApprovalListEventReceiver.LegalTeamChanged_NoChange_False(web);
                                
       //       User.GetUserProfile_existing_return(web); 
       //       User.GetUserManagerProfile_existing_return(web);
       //       User.GetUserAttributes_existing_return(web);
       //       User.AddUserToField_valid_added(web);
       //       User.GetGroupFromField_existing_return(web);
       //       User.AddGroupToField_valid_added(web);

               // Security.AssignItemPermissions_Perms_Assigned(web);
               // Security.AssignItemPermissions_PrinRole_Assigned(web);
               // Security.ClearItemSecurity_Inherited_NoPerm(web);
               // Security.ClearItemSecurity_ItemSecurity_NoPerm(web);
               // Security.ClearItemSecurity_Member_Removed(web);
               // Security.GetSecurityList_Entries_Valid(web);
               // Security.SetItemContribute_Exists_Contribute(web);
               // Security.SetItemContribute_GroupDoesNotExists_NoAction(web);
               // Security.SetItemContribute_ItemDoesNotExists_NoAction(web);
               // Security.SetItemReadOnly_Contributor_Read(web);


              //  ApprovalDepartment.ApprovalDepartment_band3_notonworkflow(web);
              //  ApprovalDepartment.ApprovalDepartment_band5_onworkflow(web);
              //  ApprovalDepartment.ApprovalDepartment_proxyband3_onworkflow(web);
              //  ApprovalDepartment.ApprovalDepartment_proxyband5_onworkflow(web);
              //  ApprovalDepartment.ApprovalDepartment_selfnoapp_onworkflow(web);
              //  ApprovalDepartment.ApprovalDepartment_proxynoapp_onworkflow(web);

           //     ApprovalFinance.ApprovalFinancial_noapproval_none(web);
           //     ApprovalFinance.ApprovalFinancial_invalidcc_details(web);
           //     ApprovalFinance.ApprovalFinancial_firstlevel_oneapp(web);
           //     ApprovalFinance.ApprovalFinancial_firstlevelmax_oneapp(web);
           //     ApprovalFinance.ApprovalFinancial_secondlevel_twoapp(web);
           //     ApprovalFinance.ApprovalFinancial_secondlevelmax_twoapp(web);
           //     ApprovalFinance.ApprovalFinancial_thirdlevel_threeapp(web);
           //     ApprovalFinance.ApprovalFinancial_thirdlevelmax_threeapp(web);
           //     ApprovalFinance.ApprovalFinancial_high_all(web);
                  
    //          Approval.Approval_deptfin_merge(web);
    //          Approval.Approval_dupdeptfin_merge(web);
    //          Approval.Approval_nofin_merge(web);
    //          Approval.Approval_missingfin_invalid(web);

                //LRF.GetLRFListItem(web, lrfnumber)
                //LRF.SetMemberReadOnly(item, member);

         //       LRF.LRF_costcenterdetails_fetch(web);
         //       LRF.LRF_costcenter_assign(web);
         //       LRF.LRF_NoCostInfo_Null(web);
         //         LRF.LRF_OneValue_CC(web);
         //         LRF.LRF_MultiCC_MaxCC(web);
         //        LRF.GetMaxCostCenter_MultiCC_MaxCC(web);
         //        LRF.GetMaxCostCenter_NoCostInfo_Null(web);
         //      LRF.GetMaxCostCenter_OneValue_CC(web);
         //        LRF.SetCostCenterFields_Different_Set(web);
         //        LRF.SetCostCenterFields_NoValue_Set(web);

             //   LRF.LRF_draft_exist(web);
            //     LRF.LRF_submit_exist(web);
            //    LRF.LRF_canceled_exist(web);
            //    LRF.SetSecurity_Draft_Set(web);
            //    LRF.SetSecurity_Submit_Set(web);
            //    LRF.SetSecurity_Approved_Set(web);
           //     LRF.ResetLegalSecurity_New_Switched(web);
                //lrf.GetSubmitSecurityList()
                //lrf.GetCompletedSecurityList
                //lrf.GetDraftSecurityList
                //lrf.SetSecurity(); 

        //        LRF.LRF_SetCostCenters_Done(web);

              //     LRF.LRF_bylrfnumber_exist(web);
              //     LRF.SetMemberReadOnly_Exists_Read(web);

          //      LRF.PartyName_Party_Valid(web);

          //      LRF.CreateEmailComponents_Assigned_Parts(web);
          //      LRF.CreateEmailComponents_Executed_Parts(web);

                //  ApprovalTask.ApprovalTask_Exists_Values(web);
            }
        }
    }
}
